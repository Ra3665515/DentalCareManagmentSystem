using DentalCareManagmentSystem.Application.DTOs;
using DentalCareManagmentSystem.Application.Interfaces;
using DentalCareManagmentSystem.Domain.Entities;
using DentalCareManagmentSystem.Domain.Enums;
using DentalCareManagmentSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DentalCareManagmentSystem.Infrastructure.Services;

public class AppointmentService : IAppointmentService
{
    private readonly ClinicDbContext _context;

    public AppointmentService(ClinicDbContext context)
    {
        _context = context;
    }

    public void Create(AppointmentDto appointmentDto)
    {
        var appointment = new Appointment
        {
            PatientId = appointmentDto.PatientId,
            Date = appointmentDto.Date,
            StartTime = appointmentDto.StartTime,
            EndTime = appointmentDto.StartTime.Add(TimeSpan.FromMinutes(30)), // Assuming 30 min slots
            Status = AppointmentStatus.Scheduled,
            Notes = appointmentDto.Notes // Added
        };
        try
        {
            _context.Appointments.Add(appointment);
            _context.SaveChanges();
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine(ex.InnerException?.Message);
            throw;
        }

    }

    public IQueryable<AppointmentDto> GetAll()
    {
        return _context.Appointments
            .Include(a => a.Patient)
            .Select(a => new AppointmentDto
            {
                Id = a.Id,
                PatientId = a.PatientId,
                PatientName = a.Patient.FullName,
                PatientPhone = a.Patient.Phone,
                Date = a.Date,
                StartTime = a.StartTime,
                EndTime = a.EndTime, // Added
                Status = a.Status.ToString(),
                Notes = a.Notes // Added
            });
    }

    public AppointmentDto GetById(Guid id)
    {
        return GetAll().FirstOrDefault(a => a.Id == id);
    }

    public List<AppointmentDto> GetTodaysAppointments()
    {
        return GetAll().Where(a => a.Date.Date == DateTime.Today).ToList();
    }

    public List<AppointmentDto> GetPendingAppointments()
    {
        return GetAll().Where(a => a.Status == AppointmentStatus.Scheduled.ToString()).ToList();
    }

    public void Delete(Guid id)
    {
        var appointment = _context.Appointments.Find(id);
        if (appointment != null)
        {
            _context.Appointments.Remove(appointment);
            _context.SaveChanges();
        }
    }

    public void UpdateStatus(Guid id, string status)
    {
        var appointment = _context.Appointments.Find(id);
        if (appointment != null)
        {
            appointment.Status = Enum.Parse<AppointmentStatus>(status);
            _context.SaveChanges();
        }
    }

    public List<AppointmentDto> GetAppointmentsByDate(DateTime date)
    {
        return GetAll().Where(a => a.Date.Date == date.Date).ToList();
    }

    public List<AppointmentDto> GetCompletedAppointments()
    {
        return GetAll().Where(a => a.Status == AppointmentStatus.Completed.ToString()).ToList();
    }

    public List<AppointmentDto> GetCancelledAppointments()
    {
        return GetAll().Where(a => a.Status == AppointmentStatus.Cancelled.ToString()).ToList();
    }

    public List<AppointmentDto> GetAppointmentsThisMonth()
    {
        return GetAll().Where(a => a.Date.Month == DateTime.UtcNow.Month && a.Date.Year == DateTime.UtcNow.Year).ToList();
    }

    public Dictionary<string, int> GetAppointmentCountByStatus()
    {
        return _context.Appointments
            .GroupBy(a => a.Status.ToString())
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionary(x => x.Status, x => x.Count);
    }

    public Dictionary<string, int> GetAppointmentsByMonth()
    {
        return _context.Appointments
            .GroupBy(a => new { a.Date.Year, a.Date.Month })
            .AsEnumerable()
            .Select(g => new { Month = $"{g.Key.Year}-{g.Key.Month:00}", Count = g.Count() })
            .OrderBy(x => x.Month)
            .ToDictionary(x => x.Month, x => x.Count);
    }

    public List<AppointmentDto> GetAppointmentsByDateRange(DateTime startDate, DateTime endDate)
    {
        return GetAll().Where(a => a.Date.Date >= startDate.Date && a.Date.Date <= endDate.Date).ToList();
    }

    public void MarkAsNotified(Guid id, string userId)
    {
        var appointment = _context.Appointments.Find(id);
        if (appointment != null && appointment.Status == AppointmentStatus.Scheduled)
        {
            appointment.Status = AppointmentStatus.Notified;
            _context.NotificationLogs.Add(new NotificationLog
            {
                AppointmentId = id,
                NotifiedById = userId,
                NotifiedAt = DateTime.UtcNow
            });
            _context.SaveChanges();
        }
    }

    public void Update(AppointmentDto appointmentDto)
    {
        var appointment = _context.Appointments.Find(appointmentDto.Id);
        if (appointment != null)
        {
            appointment.PatientId = appointmentDto.PatientId;
            appointment.Date = appointmentDto.Date;
            appointment.StartTime = appointmentDto.StartTime;
            appointment.EndTime = appointmentDto.EndTime; // Updated
            appointment.Status = Enum.Parse<AppointmentStatus>(appointmentDto.Status);
            appointment.Notes = appointmentDto.Notes; // Added
            _context.SaveChanges();
        }
    }
}