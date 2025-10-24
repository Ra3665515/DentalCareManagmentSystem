
using DentalCareManagmentSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DentalCareManagmentSystem.Web.Controllers;

[Authorize]
public class NotificationsController : Controller
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public IActionResult Today()
    {
        var notifications = _notificationService.GetTodayNotifications();
        return PartialView("_TodayNotifications", notifications);
    }

    [HttpPost]
    public IActionResult MarkDone(Guid id)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            _notificationService.MarkAsDone(id, User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            return Ok();
        }
        return Unauthorized();
    }
}
