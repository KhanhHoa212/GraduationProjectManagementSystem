using GPMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GPMS.Web.Controllers;

[Authorize]
public class NotificationController : Controller
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpPost]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        await _notificationService.MarkAsReadAsync(id);
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> MarkAsUnread(int id)
    {
        // Assuming there's a MarkAsUnreadAsync or similar. 
        // Based on INotificationService, there is ToggleReadStatusAsync.
        await _notificationService.ToggleReadStatusAsync(id);
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> ToggleReadStatus(int id)
    {
        await _notificationService.ToggleReadStatusAsync(id);
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return BadRequest();

        await _notificationService.MarkAllAsReadAsync(userId);
        return Ok();
    }
}
