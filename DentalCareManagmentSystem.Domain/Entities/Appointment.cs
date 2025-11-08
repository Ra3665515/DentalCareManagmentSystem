
using DentalCareManagmentSystem.Domain.Enums;

namespace DentalCareManagmentSystem.Domain.Entities;

public class Appointment
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public AppointmentStatus Status { get; set; }
    public string? Notes { get; set; } // Added

    public virtual Patient? Patient { get; set; }
}
