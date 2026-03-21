using AutoMapper;
using GPMS.Application.DTOs;
using GPMS.Application.Interfaces.Repositories;
using GPMS.Application.Interfaces.Services;
using GPMS.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GPMS.Application.Services;

public class ChecklistService : IChecklistService
{
    private readonly IChecklistRepository _checklistRepository;
    private readonly IReviewRoundRepository _roundRepository;
    private readonly IMapper _mapper;

    public ChecklistService(
        IChecklistRepository checklistRepository, 
        IReviewRoundRepository roundRepository,
        IMapper mapper)
    {
        _checklistRepository = checklistRepository;
        _roundRepository = roundRepository;
        _mapper = mapper;
    }

    public async Task<ChecklistDto?> GetByRoundIdAsync(int roundId)
    {
        var checklist = await _checklistRepository.GetByRoundIdAsync(roundId);
        if (checklist == null) return null;

        return new ChecklistDto
        {
            ChecklistID = checklist.ChecklistID,
            ReviewRoundID = checklist.ReviewRoundID,
            ReviewRoundTitle = $"Round {checklist.ReviewRound?.RoundNumber} ({checklist.ReviewRound?.RoundType})",
            Title = checklist.Title,
            Description = checklist.Description,
            Items = checklist.ChecklistItems.OrderBy(i => i.OrderIndex).Select(i => new ChecklistItemDto
            {
                ItemID = i.ItemID,
                ItemCode = i.ItemCode,
                ItemContent = i.ItemContent,
                ItemName = i.ItemName,
                ItemType = i.ItemType,
                Section = i.Section,
                OrderIndex = i.OrderIndex,
                Rubrics = i.RubricDescriptions.Select(r => new RubricDescriptionDto
                {
                    RubricID = r.RubricID,
                    GradeLevel = r.GradeLevel,
                    Description = r.Description
                }).ToList()
            }).ToList()
        };
    }

    public async Task<(bool Success, string Message)> SaveChecklistAsync(SaveChecklistDto dto)
    {
        var checklist = await _checklistRepository.GetByRoundIdAsync(dto.ReviewRoundID);
        
        if (checklist == null)
        {
            checklist = new ReviewChecklist
            {
                ReviewRoundID = dto.ReviewRoundID,
                Title = dto.Title,
                Description = dto.Description
            };
            await _checklistRepository.AddAsync(checklist);
        }
        else
        {
            checklist.Title = dto.Title;
            checklist.Description = dto.Description;
        }

        // Sync Items
        var currentItems = checklist.ChecklistItems.ToList();
        var updatedItems = dto.Items ?? new List<ChecklistItemDto>();

        // Remove deleted
        foreach (var item in currentItems)
        {
            if (!updatedItems.Any(i => i.ItemID == item.ItemID && i.ItemID != 0))
            {
                checklist.ChecklistItems.Remove(item);
            }
        }

        // Add or Update
        foreach (var itemDto in updatedItems)
        {
            if (itemDto.ItemID == 0)
            {
                var newItem = new ChecklistItem
                {
                    ItemCode = itemDto.ItemCode,
                    ItemContent = itemDto.ItemContent,
                    ItemName = itemDto.ItemName,
                    ItemType = itemDto.ItemType,
                    Section = itemDto.Section,
                    OrderIndex = itemDto.OrderIndex
                };

                if (itemDto.Rubrics != null && itemDto.Rubrics.Any())
                {
                    foreach (var r in itemDto.Rubrics)
                    {
                        newItem.RubricDescriptions.Add(new RubricDescription
                        {
                            GradeLevel = r.GradeLevel,
                            Description = r.Description
                        });
                    }
                }
                checklist.ChecklistItems.Add(newItem);
            }
            else
            {
                var existingItem = checklist.ChecklistItems.FirstOrDefault(i => i.ItemID == itemDto.ItemID);
                if (existingItem != null)
                {
                    existingItem.ItemCode = itemDto.ItemCode;
                    existingItem.ItemContent = itemDto.ItemContent;
                    existingItem.ItemName = itemDto.ItemName;
                    existingItem.ItemType = itemDto.ItemType;
                    existingItem.Section = itemDto.Section;
                    existingItem.OrderIndex = itemDto.OrderIndex;

                    // Sync Rubrics
                    existingItem.RubricDescriptions.Clear();
                    if (itemDto.Rubrics != null)
                    {
                        foreach (var r in itemDto.Rubrics)
                        {
                            existingItem.RubricDescriptions.Add(new RubricDescription
                            {
                                GradeLevel = r.GradeLevel,
                                Description = r.Description
                            });
                        }
                    }
                }
            }
        }

        await _checklistRepository.SaveChangesAsync();
        return (true, "Checklist saved successfully.");
    }

    public async Task<(bool Success, string Message)> CopyChecklistAsync(int fromSemesterId, int toSemesterId, List<int> roundNumbers)
    {
        // 1. Get all checklists from source semester
        var sourceChecklists = await _checklistRepository.GetChecklistsBySemesterIdAsync(fromSemesterId);
        
        // 2. Get target rounds in destination semester
        var targetRounds = await _roundRepository.GetBySemesterAsync(toSemesterId);

        foreach (var roundNum in roundNumbers)
        {
            var sourceChecklist = sourceChecklists.FirstOrDefault(c => c.ReviewRound?.RoundNumber == roundNum);
            var targetRound = targetRounds.FirstOrDefault(r => r.RoundNumber == roundNum);

            if (sourceChecklist != null && targetRound != null)
            {
                // Delete existing in target if any
                var existingTarget = await _checklistRepository.GetByRoundIdAsync(targetRound.ReviewRoundID);
                if (existingTarget != null)
                {
                    _checklistRepository.Delete(existingTarget);
                }

                // Deep copy
                var newChecklist = new ReviewChecklist
                {
                    ReviewRoundID = targetRound.ReviewRoundID,
                    Title = sourceChecklist.Title,
                    Description = sourceChecklist.Description,
                    ChecklistItems = sourceChecklist.ChecklistItems.Select(i => new ChecklistItem
                    {
                        ItemCode = i.ItemCode,
                        ItemContent = i.ItemContent,
                        ItemName = i.ItemName,
                        ItemType = i.ItemType,
                        Section = i.Section,
                        OrderIndex = i.OrderIndex,
                        RubricDescriptions = i.RubricDescriptions.Select(r => new RubricDescription
                        {
                            GradeLevel = r.GradeLevel,
                            Description = r.Description
                        }).ToList()
                    }).ToList()
                };
                await _checklistRepository.AddAsync(newChecklist);
            }
        }

        await _checklistRepository.SaveChangesAsync();
        return (true, "Copy completed successfully.");
    }

    public async Task<IEnumerable<ChecklistDto>> GetBySemesterIdAsync(int semesterId)
    {
        var checklists = await _checklistRepository.GetChecklistsBySemesterIdAsync(semesterId);
        return checklists.Select(c => new ChecklistDto
        {
            ChecklistID = c.ChecklistID,
            ReviewRoundID = c.ReviewRoundID,
            ReviewRoundTitle = $"Round {c.ReviewRound?.RoundNumber}",
            Title = c.Title,
            Description = c.Description,
            Items = c.ChecklistItems.Select(i => new ChecklistItemDto 
            { 
                ItemID = i.ItemID,
                ItemCode = i.ItemCode,
                ItemContent = i.ItemContent,
                ItemName = i.ItemName,
                ItemType = i.ItemType,
                Section = i.Section,
                OrderIndex = i.OrderIndex
            }).ToList()

        });
    }
}
