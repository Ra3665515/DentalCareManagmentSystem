
namespace DentalCareManagmentSystem.Domain.Entities;

public class NotificationLog
{
    public Guid Id { get; set; }
    public Guid AppointmentId { get; set; }
    public string? NotifiedById { get; set; }
    public DateTime NotifiedAt { get; set; }

    public virtual Appointment? Appointment { get; set; }
    public virtual User? NotifiedBy { get; set; }
}
