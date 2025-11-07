
using DentalCareManagmentSystem.Application.DTOs;

namespace DentalCareManagmentSystem.Application.Interfaces;

public interface IAppointmentService
{
    AppointmentDto? GetById(Guid id);
    IQueryable<AppointmentDto> GetAll();
    List<AppointmentDto> GetTodaysAppointments();
    List<AppointmentDto> GetPendingAppointments();
    void Create(AppointmentDto appointment);
    void Update(AppointmentDto appointment);
    void Delete(Guid id);
    void UpdateStatus(Guid id, string status);
    List<AppointmentDto> GetAppointmentsByDate(DateTime date);
    List<AppointmentDto> GetCompletedAppointments();
    List<AppointmentDto> GetCancelledAppointments();
    List<AppointmentDto> GetAppointmentsThisMonth();
    Dictionary<string, int> GetAppointmentCountByStatus();
    Dictionary<string, int> GetAppointmentsByMonth();
    List<AppointmentDto> GetAppointmentsByDateRange(DateTime startDate, DateTime endDate);
    void MarkAsNotified(Guid id, string userId);
}
