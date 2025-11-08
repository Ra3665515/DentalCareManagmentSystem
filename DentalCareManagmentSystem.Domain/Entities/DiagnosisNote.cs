
namespace DentalCareManagmentSystem.Domain.Entities;

public class DiagnosisNote
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string? DoctorId { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual Patient? Patient { get; set; }
    public virtual User? Doctor { get; set; }
}
