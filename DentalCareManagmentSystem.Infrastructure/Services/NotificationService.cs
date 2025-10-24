
using DentalCareManagmentSystem.Application.DTOs;
using DentalCareManagmentSystem.Application.Interfaces;
using DentalCareManagmentSystem.Domain.Enums;
using DentalCareManagmentSystem.Infrastructure.Data;

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
}
