using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HotelManagementSystem.Business.service;

public class NotificationViewComponent : ViewComponent
{
    private readonly NotificationService _service;
    public NotificationViewComponent(NotificationService service) => _service = service;

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