using GPMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GPMS.Web.ViewComponents;

public class NotificationDropdownViewComponent : ViewComponent
{
    private readonly INotificationService _notificationService;

    public NotificationDropdownViewComponent(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var userId = ((ClaimsPrincipal)User).FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Content(string.Empty);
        }

        var notifications = await _notificationService.GetNotificationsByRecipientAsync(userId);
        var unreadCount = await _notificationService.GetUnreadCountAsync(userId);

        var last10 = notifications.OrderByDescending(n => n.CreatedAt).Take(10).ToList();

        ViewBag.UnreadCount = unreadCount;

        return View(last10);
    }
}
