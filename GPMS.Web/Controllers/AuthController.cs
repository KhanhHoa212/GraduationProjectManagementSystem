using GPMS.Application.DTOs;
using GPMS.Application.Interfaces.Services;
using GPMS.Web.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GPMS.Web.Controllers;

public class AuthController : Controller
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    private IActionResult GetRedirect(UserDto user)
    {
        if (user.Roles.Contains("Admin") || user.Roles.Contains("HeadOfDept"))
            return RedirectToAction("Index", "Admin");
        if (user.Roles.Contains("Lecturer"))
            return RedirectToAction("Dashboard", "Lecturer");
        if (user.Roles.Contains("Student"))
            return RedirectToAction("Index", "Student");
        
        return RedirectToAction("Index", "Home");
    }

    private async Task SignInUserAsync(UserDto user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserID),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Name, user.FullName)
        };

        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var authProps = new AuthenticationProperties { IsPersistent = true };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProps);
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _authService.LoginAsync(new LoginDto
        {
            Identifier = model.Identifier,
            Password = model.Password
        });

        if (result.Success && result.User != null)
        {
            await SignInUserAsync(result.User);
            return GetRedirect(result.User);
        }

        ModelState.AddModelError("", result.ErrorMessage ?? "Invalid login attempt.");
        return View(model);
    }

    public IActionResult GoogleLogin()
    {
        var redirectUrl = Url.Action("GoogleCallback", "Auth");
        var props = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(props, GoogleDefaults.AuthenticationScheme);
    }

    public async Task<IActionResult> GoogleCallback()
    {
        var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        var fullName = User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
        var picture = User.FindFirstValue("urn:google:picture");

        var result = await _authService.GoogleLoginOrRegisterAsync(email, fullName, picture);

        if (result.Success && result.User != null)
        {
            await SignInUserAsync(result.User);
            return GetRedirect(result.User);
        }

        TempData["ErrorMessage"] = result.ErrorMessage;
        return RedirectToAction(nameof(Login));
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _authService.ForgotPasswordAsync(model.Email);
        ViewData["SuccessMessage"] = "If the email exists in the system, we have sent a password reset link.";
        return View();
    }

    [HttpGet]
    public IActionResult ResetPassword(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return RedirectToAction(nameof(Login));

        return View(new ResetPasswordViewModel { Token = token });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _authService.ResetPasswordAsync(new ResetPasswordDto
        {
            Token = model.Token,
            NewPassword = model.NewPassword
        });

        if (result.Success)
        {
            TempData["SuccessMessage"] = "Password reset successfully! Please login!";
            return RedirectToAction(nameof(Login));
        }

        ModelState.AddModelError("", result.ErrorMessage ?? "Invalid or expired link.");
        return View(model);
    }
}
