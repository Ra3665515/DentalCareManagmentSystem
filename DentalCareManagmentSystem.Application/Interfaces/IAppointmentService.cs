
using DentalCareManagmentSystem.Application.DTOs;

namespace DentalCareManagmentSystem.Application.Interfaces;

public interface IAppointmentService
{
    AppointmentDto GetById(Guid id);
    IQueryable<AppointmentDto> GetAll();
    List<AppointmentDto> GetTodaysAppointments();
    List<AppointmentDto> GetPendingAppointments(); // Added
    void Create(AppointmentDto appointment);
    void Update(AppointmentDto appointment);
    void Delete(Guid id); // Added
    void UpdateStatus(Guid id, string status); // Added
    List<AppointmentDto> GetAppointmentsByDate(DateTime date); // Added
    List<AppointmentDto> GetCompletedAppointments(); // Added
    List<AppointmentDto> GetCancelledAppointments(); // Added
    List<AppointmentDto> GetAppointmentsThisMonth(); // Added
    Dictionary<string, int> GetAppointmentCountByStatus(); // Added
    Dictionary<string, int> GetAppointmentsByMonth(); // Added
    List<AppointmentDto> GetAppointmentsByDateRange(DateTime startDate, DateTime endDate); // Added
    void MarkAsNotified(Guid id, string userId);
}
