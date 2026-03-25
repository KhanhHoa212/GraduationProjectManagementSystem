using AutoMapper;
using GPMS.Application.DTOs;
using GPMS.Application.Interfaces.Repositories;
using GPMS.Application.Interfaces.Services;
using GPMS.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GPMS.Application.Services;

public class ReviewRoundService : IReviewRoundService
{
    private readonly IReviewRoundRepository _repository;
    private readonly ISemesterRepository _semesterRepository;
    private readonly IMapper _mapper;

    public ReviewRoundService(IReviewRoundRepository repository, ISemesterRepository semesterRepository, IMapper mapper)
    {
        _repository = repository;
        _semesterRepository = semesterRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ReviewRoundDto>> GetReviewRoundsBySemesterAsync(int semesterId)
    {
        var rounds = await _repository.GetBySemesterAsync(semesterId);

        var now = System.DateTime.Now;
        bool anyChanged = false;
        foreach (var r in rounds)
        {
            var expectedStatus = r.StartDate.Date > now.Date ? GPMS.Domain.Enums.RoundStatus.Planned : 
                                 r.EndDate.Date < now.Date ? GPMS.Domain.Enums.RoundStatus.Completed : 
                                 GPMS.Domain.Enums.RoundStatus.Ongoing;
            if (r.Status != expectedStatus)
            {
                r.Status = expectedStatus;
                _repository.Update(r);
                anyChanged = true;
            }
        }
        if (anyChanged) await _repository.SaveChangesAsync();

        var mappedRounds = _mapper.Map<IEnumerable<ReviewRoundDto>>(rounds);
        return mappedRounds.OrderBy(r => r.RoundNumber);
    }

    public async Task<ReviewRoundDto?> GetReviewRoundByIdAsync(int id)
    {
        var r = await _repository.GetByIdAsync(id);
        if (r == null) return null;

        var now = System.DateTime.Now;
        var expectedStatus = r.StartDate.Date > now.Date ? GPMS.Domain.Enums.RoundStatus.Planned : 
                             r.EndDate.Date < now.Date ? GPMS.Domain.Enums.RoundStatus.Completed : 
                             GPMS.Domain.Enums.RoundStatus.Ongoing;
        if (r.Status != expectedStatus)
        {
            r.Status = expectedStatus;
            _repository.Update(r);
            await _repository.SaveChangesAsync();
        }

        return _mapper.Map<ReviewRoundDto>(r);
    }

    public async Task CreateReviewRoundAsync(CreateReviewRoundDto dto)
    {
        var existingRounds = await _repository.GetBySemesterAsync(dto.SemesterID);
        if (existingRounds.Count() >= 3)
        {
            throw new System.InvalidOperationException("Mỗi học kỳ chỉ được phép có tối đa 3 vòng review.");
        }

        var entity = _mapper.Map<ReviewRound>(dto);
        
        var now = System.DateTime.Now;
        entity.Status = entity.StartDate.Date > now.Date ? GPMS.Domain.Enums.RoundStatus.Planned : 
                        entity.EndDate.Date < now.Date ? GPMS.Domain.Enums.RoundStatus.Completed : 
                        GPMS.Domain.Enums.RoundStatus.Ongoing;

        if (dto.SubmissionRequirements != null && dto.SubmissionRequirements.Any())
        {
            foreach (var reqDto in dto.SubmissionRequirements)
            {
                entity.SubmissionRequirements.Add(new SubmissionRequirement
                {
                    DocumentName = reqDto.DocumentName,
                    Description = reqDto.Description,
                    AllowedFormats = reqDto.AllowedFormats,
                    MaxFileSizeMB = reqDto.MaxFileSizeMB,
                    IsRequired = reqDto.IsRequired
                });
            }
        }

        await _repository.AddAsync(entity);
        await _repository.SaveChangesAsync();
    }

    public async Task UpdateReviewRoundAsync(int id, CreateReviewRoundDto dto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) throw new KeyNotFoundException("Review round not found.");

        _mapper.Map(dto, existing);

        var now = System.DateTime.Now;
        existing.Status = existing.StartDate.Date > now.Date ? GPMS.Domain.Enums.RoundStatus.Planned : 
                          existing.EndDate.Date < now.Date ? GPMS.Domain.Enums.RoundStatus.Completed : 
                          GPMS.Domain.Enums.RoundStatus.Ongoing;

        // Sync requirements explicitly
        var currentReqs = existing.SubmissionRequirements.ToList();
        var updatedReqs = dto.SubmissionRequirements ?? new List<SubmissionRequirementDto>();

        // Remove deleted
        foreach (var checkDelete in currentReqs)
        {
            if (!updatedReqs.Any(r => r.RequirementID == checkDelete.RequirementID))
            {
                existing.SubmissionRequirements.Remove(checkDelete);
            }
        }

        // Add or Update
        foreach (var reqDto in updatedReqs)
        {
            if (reqDto.RequirementID == 0)
            {
                // New Requirement
                existing.SubmissionRequirements.Add(new SubmissionRequirement
                {
                    DocumentName = reqDto.DocumentName,
                    Description = reqDto.Description,
                    AllowedFormats = reqDto.AllowedFormats,
                    MaxFileSizeMB = reqDto.MaxFileSizeMB,
                    IsRequired = reqDto.IsRequired
                });
            }
            else
            {
                // Existing Requirement
                var existingReq = existing.SubmissionRequirements.FirstOrDefault(r => r.RequirementID == reqDto.RequirementID);
                if (existingReq != null)
                {
                    existingReq.DocumentName = reqDto.DocumentName;
                    existingReq.Description = reqDto.Description;
                    existingReq.AllowedFormats = reqDto.AllowedFormats;
                    existingReq.MaxFileSizeMB = reqDto.MaxFileSizeMB;
                    existingReq.IsRequired = reqDto.IsRequired;
                }
            }
        }

        _repository.Update(existing);
        await _repository.SaveChangesAsync();
    }

    public async Task DeleteReviewRoundAsync(int id)
    {
        throw new System.InvalidOperationException("Không thể xóa các vòng review mặc định của học kỳ.");
    }

    public async Task<bool> InitializeDefaultRoundsAsync(int semesterId)
    {
        var existingRounds = await _repository.GetBySemesterAsync(semesterId);
        if (existingRounds.Any()) return false;

        var semester = await _semesterRepository.GetByIdAsync(semesterId);
        if (semester == null) return false;

        var now = System.DateTime.Now;
        var semesterStart = semester.StartDate;
        
        // Define 3 rounds with 4-week intervals (default)
        for (int i = 1; i <= 3; i++)
        {
            var roundStart = semesterStart.AddDays((i - 1) * 28); // Every 4 weeks
            var roundEnd = roundStart.AddDays(27).AddHours(23).AddMinutes(59);
            
            var round = new ReviewRound
            {
                SemesterID = semesterId,
                RoundNumber = i,
                RoundType = i == 3 ? Domain.Enums.RoundType.Offline : Domain.Enums.RoundType.Online, // Final round usually Presentation
                StartDate = roundStart,
                EndDate = roundEnd,
                SubmissionDeadline = roundEnd.AddHours(-2), // 2 hours before round end
                Description = $"Vòng Review {i} cho học kỳ {semester.SemesterCode}",
                Status = roundStart.Date > now.Date ? Domain.Enums.RoundStatus.Planned : 
                         roundEnd.Date < now.Date ? Domain.Enums.RoundStatus.Completed : 
                         Domain.Enums.RoundStatus.Ongoing
            };

            await _repository.AddAsync(round);
        }

        await _repository.SaveChangesAsync();
        return true;
    }
}
