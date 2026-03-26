using GPMS.Application.DTOs;
using GPMS.Application.Interfaces.Services;
using GPMS.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace GPMS.Web.Controllers;

[Authorize(Roles = "Admin")]
public class MajorController : Controller
{
    private readonly IMajorService _majorService;

    public MajorController(IMajorService majorService)
    {
        _majorService = majorService;
    }

    public async Task<IActionResult> Index()
    {
        var majors = await _majorService.GetAllMajorsAsync();
        var vm = majors.Select(m => new MajorViewModel
        {
            MajorID = m.MajorID,
            MajorCode = m.MajorCode,
            MajorName = m.MajorName,
            FacultyName = m.FacultyName,
            FacultyID = m.FacultyID
        }).ToList();

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var faculties = await _majorService.GetAllFacultiesAsync();
        var vm = new EditMajorViewModel
        {
            Faculties = faculties
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EditMajorViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Faculties = await _majorService.GetAllFacultiesAsync();
            return View(model);
        }

        var dto = new CreateMajorDto
        {
            MajorCode = model.MajorCode.ToUpper(),
            MajorName = model.MajorName,
            FacultyID = model.FacultyID
        };

        var error = await _majorService.CreateMajorAsync(dto);
        if (error != null)
        {
            ModelState.AddModelError("", error);
            model.Faculties = await _majorService.GetAllFacultiesAsync();
            return View(model);
        }

        TempData["SuccessMessage"] = "Major created successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var major = await _majorService.GetMajorByIdAsync(id);
        if (major == null) return NotFound();

        var faculties = await _majorService.GetAllFacultiesAsync();
        var vm = new EditMajorViewModel
        {
            MajorID = major.MajorID,
            MajorCode = major.MajorCode.ToUpper(),
            MajorName = major.MajorName,
            FacultyID = major.FacultyID,
            Faculties = faculties
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditMajorViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Faculties = await _majorService.GetAllFacultiesAsync();
            return View(model);
        }

        var dto = new UpdateMajorDto
        {
            MajorID = model.MajorID,
            MajorCode = model.MajorCode.ToUpper(),
            MajorName = model.MajorName,
            FacultyID = model.FacultyID
        };

        var error = await _majorService.UpdateMajorAsync(dto);
        if (error != null)
        {
            ModelState.AddModelError("", error);
            model.Faculties = await _majorService.GetAllFacultiesAsync();
            return View(model);
        }

        TempData["SuccessMessage"] = "Major updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _majorService.DeleteMajorAsync(id);
        if (!result)
        {
            TempData["ErrorMessage"] = "Cannot delete major because it has associated projects or was not found.";
        }
        else
        {
            TempData["SuccessMessage"] = "Major deleted successfully!";
        }
        return RedirectToAction(nameof(Index));
    }
}
