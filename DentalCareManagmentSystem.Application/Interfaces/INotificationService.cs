
using DentalCareManagmentSystem.Application.DTOs;

namespace DentalCareManagmentSystem.Application.Interfaces;

public interface INotificationService
{
    List<AppointmentDto> GetTodayNotifications();
    void MarkAsDone(Guid appointmentId, string userId);
}
