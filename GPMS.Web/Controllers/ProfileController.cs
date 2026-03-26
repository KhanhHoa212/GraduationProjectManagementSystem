using GPMS.Application.DTOs;
using GPMS.Application.Interfaces.Services;
using GPMS.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GPMS.Web.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly IUserService _userService;
    private readonly IFileService _fileService;

    public ProfileController(IUserService userService, IFileService fileService)
    {
        _userService = userService;
        _fileService = fileService;
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // Theo yêu cầu của người dùng, admin sẽ không được sử dụng tính năng này 
        if (User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null) return NotFound();

        var model = new ProfileViewModel
        {
            UserID = user.UserID,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            AvatarUrl = user.AvatarUrl,
            Roles = user.Roles
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        if (User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null) return NotFound();

        var model = new ProfileViewModel
        {
            UserID = user.UserID,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            AvatarUrl = user.AvatarUrl,
            Roles = user.Roles
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProfileViewModel model)
    {
        if (User.IsInRole("Admin"))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = GetCurrentUserId();
        if (userId == null || userId != model.UserID) return Unauthorized();

        try
        {
            if (model.AvatarFile != null && model.AvatarFile.Length > 0)
            {
                // Upload to Cloudinary
                model.AvatarUrl = await _fileService.UploadFileAsync(model.AvatarFile, "avatars");
            }

            var dto = new UpdateProfileDto
            {
                UserID = model.UserID,
                FullName = model.FullName,
                Email = model.Email,
                Phone = model.Phone,
                AvatarUrl = model.AvatarUrl
            };

            await _userService.UpdateProfileAsync(dto);
            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (System.Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }
}
