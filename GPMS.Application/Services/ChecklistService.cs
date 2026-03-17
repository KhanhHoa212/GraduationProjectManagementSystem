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
    private readonly IMapper _mapper;

    public ChecklistService(IChecklistRepository checklistRepository, IMapper mapper)
    {
        _checklistRepository = checklistRepository;
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
                MaxScore = i.MaxScore,
                Weight = i.Weight,
                OrderIndex = i.OrderIndex
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
                checklist.ChecklistItems.Add(new ChecklistItem
                {
                    ItemCode = itemDto.ItemCode,
                    ItemContent = itemDto.ItemContent,
                    MaxScore = itemDto.MaxScore,
                    Weight = itemDto.Weight,
                    OrderIndex = itemDto.OrderIndex
                });
            }
            else
            {
                var existingItem = checklist.ChecklistItems.FirstOrDefault(i => i.ItemID == itemDto.ItemID);
                if (existingItem != null)
                {
                    existingItem.ItemCode = itemDto.ItemCode;
                    existingItem.ItemContent = itemDto.ItemContent;
                    existingItem.MaxScore = itemDto.MaxScore;
                    existingItem.Weight = itemDto.Weight;
                    existingItem.OrderIndex = itemDto.OrderIndex;
                }
            }
        }

        await _checklistRepository.SaveChangesAsync();
        return (true, "Checklist saved successfully.");
    }
}
