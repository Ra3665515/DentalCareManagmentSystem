using DentalCareManagmentSystem.Application.DTOs;
using DentalCareManagmentSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DentalCareManagmentSystem.Web.Controllers;

[Authorize(Roles = "Receptionist,Doctor,SystemAdmin")]
public class AppointmentsController : Controller
{
    private readonly IAppointmentService _appointmentService;
    private readonly IPatientService _patientService;

    public AppointmentsController(IAppointmentService appointmentService, IPatientService patientService)
    {
        _appointmentService = appointmentService;
        _patientService = patientService;
    }

    public IActionResult Index()
    {
        var appointments = _appointmentService.GetAll().ToList();


        return View(appointments);
    }

    [HttpGet]
    public IActionResult Create(Guid? patientId)
    {
        ViewBag.Patients = new SelectList(_patientService.GetAll(), "Id", "FullName", patientId);

    
        ViewBag.StatusOptions = new SelectList(Enum.GetNames(typeof(Domain.Enums.AppointmentStatus)));

        var model = new AppointmentDto();

        if (patientId.HasValue)
        {
            model.PatientId = patientId.Value;
        }

        return View(model);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(AppointmentDto appointmentDto)
    {
        if (ModelState.IsValid)
        {
            _appointmentService.Create(appointmentDto);
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Patients = new SelectList(_patientService.GetAll(), "Id", "FullName", appointmentDto.PatientId);
        ViewBag.StatusOptions = new SelectList(Enum.GetNames(typeof(Domain.Enums.AppointmentStatus)), appointmentDto.Status);
        return View(appointmentDto);
    }

    public IActionResult Edit(Guid id)
    {
        var appointment = _appointmentService.GetById(id);
        if (appointment == null) return NotFound();
        ViewBag.Patients = new SelectList(_patientService.GetAll(), "Id", "FullName", appointment.PatientId);
        ViewBag.StatusOptions = new SelectList(Enum.GetNames(typeof(Domain.Enums.AppointmentStatus)), appointment.Status);
        return View(appointment);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(AppointmentDto appointmentDto)
    {
        if (ModelState.IsValid)
        {
            _appointmentService.Update(appointmentDto);
            return RedirectToAction(nameof(Index));
        }
        ViewBag.Patients = new SelectList(_patientService.GetAll(), "Id", "FullName", appointmentDto.PatientId);
        ViewBag.StatusOptions = new SelectList(Enum.GetNames(typeof(Domain.Enums.AppointmentStatus)), appointmentDto.Status);
        return View(appointmentDto);
    }

    public IActionResult Details(Guid id)
    {
        var appointment = _appointmentService.GetById(id);
        if (appointment == null) return NotFound();
        return View(appointment);
    }

    public IActionResult Delete(Guid id)
    {
        var appointment = _appointmentService.GetById(id);
        if (appointment == null) return NotFound();
        return View(appointment);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(Guid id)
    {
        _appointmentService.Delete(id);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult UpdateStatus(Guid id, string status)
    {
        if (Enum.TryParse<Domain.Enums.AppointmentStatus>(status, out var appointmentStatus))
        {
            _appointmentService.UpdateStatus(id, appointmentStatus.ToString());
            return Ok();
        }
        return BadRequest("Invalid status");
    }

    public IActionResult Calendar()
    {
        var appointments = _appointmentService.GetAll().ToList();
        return View(appointments);
    }

    [HttpGet]
    public IActionResult GetAppointmentsByDate(DateTime date)
    {
        var appointments = _appointmentService.GetAppointmentsByDate(date);
        return Json(appointments);
    }

    [HttpGet]
    public IActionResult GetAppointmentsGrid()
    {
        var appointments = _appointmentService.GetAll().ToList();
        return PartialView("_AppointmentsGrid", appointments);
    }
    public IActionResult TodaysAppointments()
    {
        var todaysAppointments = _appointmentService.GetTodaysAppointments()
            .Where(a => a.Status == "Scheduled").ToList();
        return View(todaysAppointments);
    }
}
