using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GPMS.Application.DTOs;
using GPMS.Domain.Entities;
using GPMS.Application.Interfaces.Repositories;
using GPMS.Domain.Enums;
using GPMS.Application.Interfaces.Services;

namespace GPMS.Application.Services;

public class CommitteeService : ICommitteeService
{
    private readonly ICommitteeRepository _committeeRepository;
    private readonly ISemesterRepository _semesterRepository;
    private readonly IUserRepository _userRepository;

    public CommitteeService(
        ICommitteeRepository committeeRepository,
        ISemesterRepository semesterRepository,
        IUserRepository userRepository)
    {
        _committeeRepository = committeeRepository;
        _semesterRepository = semesterRepository;
        _userRepository = userRepository;
    }

    public async Task<ManageCommitteeViewModel> GetManageCommitteeDataAsync()
    {
        var activeSemester = (await _semesterRepository.GetAllAsync())
            .FirstOrDefault(s => s.Status == SemesterStatus.Active);

        if (activeSemester == null) return new ManageCommitteeViewModel();

        var committees = await _committeeRepository.GetBySemesterAsync(activeSemester.SemesterID);
        var lecturers = await _userRepository.GetByRoleAsync(RoleName.Lecturer);

        return new ManageCommitteeViewModel
        {
            Committees = committees.Select(c => new CommitteeDto
            {
                CommitteeID = c.CommitteeID,
                CommitteeName = c.CommitteeName,
                ChairpersonName = c.Chairperson?.FullName ?? "Unknown",
                SecretaryName = c.Secretary?.FullName ?? "Unknown",
                ReviewerName = c.Reviewer?.FullName ?? "Unknown",
                Reviewer2Name = c.Reviewer2?.FullName,
                Reviewer3Name = c.Reviewer3?.FullName,
                ChairpersonID = c.ChairpersonID,
                SecretaryID = c.SecretaryID,
                ReviewerID = c.ReviewerID,
                Reviewer2ID = c.Reviewer2ID,
                Reviewer3ID = c.Reviewer3ID
            }).ToList(),
            Lecturers = lecturers.Select(u => new UserDto
            {
                UserID = u.UserID,
                FullName = u.FullName
            }).ToList()
        };
    }

    public async Task<bool> CreateCommitteeAsync(CreateCommitteeRequest request)
    {
        var activeSemester = (await _semesterRepository.GetAllAsync())
            .FirstOrDefault(s => s.Status == SemesterStatus.Active);

        if (activeSemester == null) return false;

        var entity = new Committee
        {
            CommitteeName = request.CommitteeName,
            SemesterID = activeSemester.SemesterID,
            ChairpersonID = request.ChairpersonID,
            SecretaryID = request.SecretaryID,
            ReviewerID = request.ReviewerID,
            Reviewer2ID = request.Reviewer2ID,
            Reviewer3ID = request.Reviewer3ID
        };

        await _committeeRepository.AddAsync(entity);
        return true;
    }

    public async Task<bool> DeleteCommitteeAsync(int id)
    {
        var committee = await _committeeRepository.GetByIdAsync(id);
        if (committee == null) return false;
        await _committeeRepository.DeleteAsync(committee);
        return true;
    }

    public async Task<CommitteeDto> GetCommitteeByIdAsync(int id)
    {
        var c = await _committeeRepository.GetByIdAsync(id);
        if (c == null) return null;
        return new CommitteeDto
        {
            CommitteeID = c.CommitteeID,
            CommitteeName = c.CommitteeName,
            ChairpersonID = c.ChairpersonID,
            SecretaryID = c.SecretaryID,
            ReviewerID = c.ReviewerID,
            Reviewer2ID = c.Reviewer2ID,
            Reviewer3ID = c.Reviewer3ID
        };
    }

    public async Task<bool> UpdateCommitteeAsync(int id, CreateCommitteeRequest request)
    {
        var c = await _committeeRepository.GetByIdAsync(id);
        if (c == null) return false;

        c.CommitteeName = request.CommitteeName;
        c.ChairpersonID = request.ChairpersonID;
        c.SecretaryID = request.SecretaryID;
        c.ReviewerID = request.ReviewerID;
        c.Reviewer2ID = request.Reviewer2ID;
        c.Reviewer3ID = request.Reviewer3ID;

        await _committeeRepository.UpdateAsync(c);
        return true;
    }
}
