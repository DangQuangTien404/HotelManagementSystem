using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HotelManagementSystem.Business.service;
using HotelManagementSystem.Business.interfaces;

public class NotificationViewComponent : ViewComponent
{
    private readonly INotificationService _service;
    public NotificationViewComponent(INotificationService service) => _service = service;

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var userIdClaim = ((ClaimsPrincipal)User).FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Content("");

        int userId = int.Parse(userIdClaim.Value);
        var unreadCount = await _service.GetUnreadCount(userId);
        var notifications = await _service.GetLatestNotifications(userId);

        ViewBag.UnreadCount = unreadCount;
        return View(notifications);
    }
}