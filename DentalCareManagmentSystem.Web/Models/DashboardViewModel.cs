using DentalCareManagmentSystem.Application.DTOs;

namespace DentalCareManagmentSystem.Web.Models
{
    public class DashboardViewModel
    {
        public int TotalPatients { get; set; }
        public int TodayAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public IEnumerable<PatientDto> RecentPatients { get; set; } = new List<PatientDto>();
        public IEnumerable<AppointmentDto> TodayAppointmentsList { get; set; } = new List<AppointmentDto>();
    }
}