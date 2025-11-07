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
        try
        {
            var notifiedAppointments = _appointmentService.GetAll()
                .Where(a => a.Status == "Notified" && a.Date.Date == DateTime.Today)
                .OrderBy(a => a.StartTime)
                .ToList();
            return Json(notifiedAppointments);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddToQueueTest(Guid appointmentId)
    {
        try
        {
            Console.WriteLine($"AddToQueue called with appointmentId: {appointmentId}");

            // Get the appointment
            var appointment = _appointmentService.GetById(appointmentId);
            if (appointment == null)
            {
                Console.WriteLine($"Appointment not found: {appointmentId}");
                return Json(new { success = false, message = "Appointment not found" });
            }

            Console.WriteLine($"Appointment found: {appointment.PatientName}, Status: {appointment.Status}");

            // Update the status to Notified (patient sent to doctor)
            _appointmentService.UpdateStatus(appointmentId, "Notified");
            Console.WriteLine($"Status updated to Notified");

            // Get the updated list of all notified patients for today (patients in queue)
            var notifiedAppointments = _appointmentService.GetAll()
                .Where(a => a.Status == "Notified" && a.Date.Date == DateTime.Today)
                .OrderBy(a => a.StartTime)
                .Select(a => new
                {
                    id = a.Id,
                    patientId = a.PatientId,
                    patientName = a.PatientName,
                    patientPhone = a.PatientPhone,
                    startTime = a.StartTime,
                    endTime = a.EndTime,
                    status = a.Status,
                    date = a.Date
                })
                .ToList();

            Console.WriteLine($"Queue count: {notifiedAppointments.Count}");

            // Notify all clients (receptionists and doctors) via SignalR
            await _hubContext.Clients.All.SendAsync("PatientSentToDoctor", notifiedAppointments);
            Console.WriteLine($"SignalR notification sent");

            // Return success response
            return Json(new
            {
                success = true,
                message = $"Patient {appointment.PatientName} sent to doctor successfully",
                appointmentId = appointmentId,
                patientName = appointment.PatientName,
                queueData = notifiedAppointments
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR in AddToQueue: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return Json(new { success = false, message = $"Error: {ex.Message}" });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompletePatient(Guid appointmentId)
    {
        try
        {
            var appointment = _appointmentService.GetById(appointmentId);
            if (appointment == null)
                return Json(new { success = false, message = "Appointment not found" });

            // Update appointment status to Completed
            _appointmentService.UpdateStatus(appointmentId, "Completed");

            // Get updated queue (remaining patients in queue)
            var notifiedAppointments = _appointmentService.GetAll()
                .Where(a => a.Status == "Notified" && a.Date.Date == DateTime.Today)
                .OrderBy(a => a.StartTime)
                .Select(a => new
                {
                    id = a.Id,
                    patientId = a.PatientId,
                    patientName = a.PatientName,
                    patientPhone = a.PatientPhone,
                    startTime = a.StartTime,
                    endTime = a.EndTime,
                    status = a.Status,
                    date = a.Date
                })
                .ToList();

            // Notify all clients that the patient is completed
            await _hubContext.Clients.All.SendAsync("PatientCompleted", appointmentId, appointment.PatientName, notifiedAppointments);

            return Json(new
            {
                success = true,
                message = "Patient session completed",
                appointmentId = appointmentId,
                patientName = appointment.PatientName,
                queueData = notifiedAppointments
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToQueue(Guid appointmentId)
    {
        try
        {
            Console.WriteLine($"AddToQueue called with appointmentId: {appointmentId}");

            // Get the appointment
            var appointment = _appointmentService.GetById(appointmentId);
            if (appointment == null)
            {
                Console.WriteLine($"Appointment not found: {appointmentId}");
                return Json(new { success = false, message = "Appointment not found" });
            }

            Console.WriteLine($"Appointment found: {appointment.PatientName}, Status: {appointment.Status}");

            // Update the status to Notified (patient sent to doctor)
            _appointmentService.UpdateStatus(appointmentId, "Notified");
            Console.WriteLine($"Status updated to Notified");

            // Get the updated list of all notified patients for today (patients in queue)
            var notifiedAppointments = _appointmentService.GetAll()
                .Where(a => a.Status == "Notified" && a.Date.Date == DateTime.Today)
                .OrderBy(a => a.StartTime)
                .Select(a => new
                {
                    id = a.Id,
                    patientId = a.PatientId,
                    patientName = a.PatientName,
                    patientPhone = a.PatientPhone,
                    startTime = a.StartTime,
                    endTime = a.EndTime,
                    status = a.Status,
                    date = a.Date
                })
                .ToList();

            Console.WriteLine($"Queue count: {notifiedAppointments.Count}");

            // Notify all clients (receptionists and doctors) via SignalR
            await _hubContext.Clients.All.SendAsync("PatientSentToDoctor", notifiedAppointments);
            Console.WriteLine($"SignalR notification sent");

            // Return success response
            return Json(new
            {
                success = true,
                message = $"Patient {appointment.PatientName} sent to doctor successfully",
                appointmentId = appointmentId,
                patientName = appointment.PatientName,
                queueData = notifiedAppointments
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR in AddToQueue: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return Json(new { success = false, message = $"Error: {ex.Message}" });
        }
    }
}