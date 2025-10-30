using DentalCareManagmentSystem.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DentalCareManagmentSystem.Application.Interfaces
{
    public interface INotificationService
    {
        List<AppointmentDto> GetTodayNotifications();
        void MarkAsDone(Guid appointmentId, string userId);

        Task<List<NotificationDto>> GetTodayNotificationsAsync();
        Task<List<AppointmentDto>> GetTodayAppointmentsAsync();
        Task<List<AppointmentDto>> GetAppointmentsByDateAsync(string date);
        Task<AppointmentDto> GetAppointmentByIdAsync(Guid id);
        Task<bool> MarkAsDoneAsync(Guid appointmentId, string userId);
    }
}