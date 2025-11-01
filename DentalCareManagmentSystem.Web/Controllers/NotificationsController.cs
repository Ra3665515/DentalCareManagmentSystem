using DentalCareManagmentSystem.Application.Interfaces;
using DentalCareManagmentSystem.Web.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace DentalCareManagmentSystem.Web.Controllers;

[Authorize(Roles = "Receptionist,Doctor,SystemAdmin")]
public class NotificationsController : Controller
{
    private readonly INotificationService _notificationService;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IAppointmentService _appointmentService;

    public NotificationsController(IAppointmentService appointmentService, IHubContext<NotificationHub> hubContext, INotificationService notificationService)
    {
        _appointmentService = appointmentService;
        _hubContext = hubContext;
        _notificationService = notificationService;
    }

    [HttpPost]
    public async Task<IActionResult> NotifyDoctor(Guid appointmentId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var appointment = _appointmentService.GetById(appointmentId);

            if (appointment == null)
                return Json(new { success = false, message = "Appointment not found" });

            // تحديث الحالة إلى Notified
            _appointmentService.UpdateStatus(appointmentId, "Notified");

            // إرسال الإشعار للدكتور عبر SignalR
            // هنا افترضنا أن الدكتور له ID ثابت أو يمكن جلبها من اليوزر
            var doctorId = User.FindFirst("DoctorId")?.Value ?? "default-doctor";

            await _hubContext.Clients.Group($"doctor-{doctorId}")
                .SendAsync("ReceivePatient",
                    appointment.PatientName,
                    appointment.Id,
                    appointment.StartTime.ToString(@"hh\:mm"),
                    DateTime.Now.ToString("hh:mm tt"));

            return Json(new
            {
                success = true,
                message = "Doctor notified successfully",
                status = "Notified"
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
    public async Task<IActionResult> Index()
    {
        var todayAppointments = await _notificationService.GetTodayAppointmentsAsync();
        return View(todayAppointments);
    }

    [HttpGet]
    public async Task<IActionResult> Today()
    {
        // استخدام الدالة الجديدة Async
        var notifications = await _notificationService.GetTodayNotificationsAsync();
        return PartialView("_TodayNotifications", notifications);
    }

    // للحفاظ على التوافق مع الكود الحالي
    [HttpGet]
    public IActionResult TodaySync()
    {
        var notifications = _notificationService.GetTodayNotifications();
        return PartialView("_TodayNotifications", notifications);
    }

    [HttpPost]
    public async Task<IActionResult> MarkDone(Guid id)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            // استخدام الدالة الجديدة Async
            var result = await _notificationService.MarkAsDoneAsync(
                id,
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value
            );

            if (result)
                return Ok();

            return BadRequest("Failed to mark appointment as done.");
        }
        return Unauthorized();
    }

    // للحفاظ على التوافق مع الكود الحالي
    [HttpPost]
    public IActionResult MarkDoneSync(Guid id)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            _notificationService.MarkAsDone(
                id,
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value
            );
            return Ok();
        }
        return Unauthorized();
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var appointment = await _notificationService.GetAppointmentByIdAsync(id);
        if (appointment == null)
        {
            return NotFound();
        }
        return View(appointment);
    }

    [HttpGet]
    public IActionResult GetQueue()
    {
        var notifiedAppointments = _appointmentService.GetAll()
            .Where(a => a.Status == "Notified")
            .OrderBy(a => a.Date)
            .ToList();
        return Json(notifiedAppointments);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToQueue(Guid appointmentId)
    {
        // Update the status to Notified
        _appointmentService.UpdateStatus(appointmentId, "Notified");

        // Get the updated list of all notified patients
        var notifiedAppointments = _appointmentService.GetAll()
            .Where(a => a.Status == "Notified")
            .OrderBy(a => a.Date)
            .ToList();

        // Notify other clients via SignalR in case they need to update their views
        await _hubContext.Clients.All.SendAsync("AddPatientToQueue", notifiedAppointments);

        // Return a partial view with the queue
        return Content("<div>Test HTML from server</div>");
    }

    [HttpPost]
    public async Task<IActionResult> CompletePatient(Guid appointmentId)
    {
        try
        {
            var appointment = _appointmentService.GetById(appointmentId);
            if (appointment == null)
                return Json(new { success = false, message = "Appointment not found" });

            // تحديث حالة الموعد
            _appointmentService.UpdateStatus(appointmentId, "Completed");

            // إعلام بأن المريض انتهى
            await _hubContext.Clients.All.SendAsync("PatientCompleted", appointmentId);

            return Json(new
            {
                success = true,
                message = "Patient marked as completed"
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CallNextPatient()
    {
        try
        {
            await _hubContext.Clients.All.SendAsync("CallNextPatient");
            return Json(new { success = true, message = "Next patient called" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Search(string date)
    {
        var appointments = await _notificationService.GetAppointmentsByDateAsync(
            date ?? DateTime.Today.ToString("yyyy-MM-dd"));
        return View("Index", appointments);
    }
}