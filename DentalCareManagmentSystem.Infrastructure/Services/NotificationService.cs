
using DentalCareManagmentSystem.Application.DTOs;
using DentalCareManagmentSystem.Application.Interfaces;
using DentalCareManagmentSystem.Domain.Enums;
using DentalCareManagmentSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DentalCareManagmentSystem.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly ClinicDbContext _context;
    private readonly IAppointmentService _appointmentService;

    public NotificationService(ClinicDbContext context, IAppointmentService appointmentService)
    {
        _context = context;
        _appointmentService = appointmentService;
    }


    public List<AppointmentDto> GetTodayNotifications()
    {
        return _appointmentService.GetTodaysAppointments()
            .Where(a => a.Status == AppointmentStatus.Scheduled.ToString())
            .ToList();
    }

    public void MarkAsDone(Guid appointmentId, string userId)
    {
        _appointmentService.MarkAsNotified(appointmentId, userId);
    }


    public async Task<List<NotificationDto>> GetTodayNotificationsAsync()
    {
        var today = DateTime.Today;
        var appointments = await _context.Appointments
            .Include(a => a.Patient)
            .Where(a => a.Date.Date == today.Date &&
                       a.Status == AppointmentStatus.Scheduled || a.Status == AppointmentStatus.Notified)
            .OrderBy(a => a.StartTime)
            .ToListAsync();

        return appointments.Select(a => new NotificationDto
        {
            Id = a.Id,
            PatientName = a.Patient?.FullName ?? "Unknown",
            AppointmentDate = a.Date,
            Message = $"Appointment scheduled for {a.StartTime.ToString(@"hh\:mm")}",
            NotifiedAt = DateTime.Now,
            IsDone = false
        }).ToList();
    }

    public async Task<List<AppointmentDto>> GetTodayAppointmentsAsync()
    {
        var today = DateTime.Today;
        var appointments = await _context.Appointments
            .Include(a => a.Patient)
            .Where(a => a.Date.Date == today.Date &&
                       a.Status != AppointmentStatus.Cancelled)
            .OrderBy(a => a.StartTime)
            .ToListAsync();

        return appointments.Select(a => new AppointmentDto
        {
            Id = a.Id,
            PatientId = a.PatientId,
            PatientName = a.Patient?.FullName ?? "Unknown",
            PatientPhone = a.Patient?.Phone ?? "N/A",
            Date = a.Date,
            StartTime = a.StartTime,
            EndTime = a.EndTime,
            Status = a.Status.ToString(),
            Notes = a.Notes
        }).ToList();
    }

    public async Task<List<AppointmentDto>> GetAppointmentsByDateAsync(string date)
    {
        if (DateTime.TryParse(date, out DateTime targetDate))
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.Date.Date == targetDate.Date &&
                           a.Status != AppointmentStatus.Cancelled)
                .OrderBy(a => a.StartTime)
                .ToListAsync();

            return appointments.Select(a => new AppointmentDto
            {
                Id = a.Id,
                PatientId = a.PatientId,
                PatientName = a.Patient?.FullName ?? "Unknown",
                PatientPhone = a.Patient?.Phone ?? "N/A",
                Date = a.Date,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Status = a.Status.ToString(),
                Notes = a.Notes
            }).ToList();
        }

        return new List<AppointmentDto>();
    }

    public async Task<AppointmentDto> GetAppointmentByIdAsync(Guid id)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Patient)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment == null) return null;

        return new AppointmentDto
        {
            Id = appointment.Id,
            PatientId = appointment.PatientId,
            PatientName = appointment.Patient?.FullName ?? "Unknown",
            PatientPhone = appointment.Patient?.Phone ?? "N/A",
            Date = appointment.Date,
            StartTime = appointment.StartTime,
            EndTime = appointment.EndTime,
            Status = appointment.Status.ToString(),
            Notes = appointment.Notes
        };
    }

    public async Task<bool> MarkAsDoneAsync(Guid appointmentId, string userId)
    {
        try
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null) return false;

            appointment.Status = AppointmentStatus.Completed;

            if (_context.Model.FindEntityType(typeof(DentalCareManagmentSystem.Domain.Entities.Appointment))
                ?.FindProperty("ModifiedBy") != null)
            {
                appointment.GetType().GetProperty("ModifiedBy")?.SetValue(appointment, userId);
            }

            if (_context.Model.FindEntityType(typeof(DentalCareManagmentSystem.Domain.Entities.Appointment))
                ?.FindProperty("ModifiedAt") != null)
            {
                appointment.GetType().GetProperty("ModifiedAt")?.SetValue(appointment, DateTime.Now);
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error marking appointment as done: {ex.Message}");
            return false;
        }
    }
}