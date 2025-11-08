
namespace DentalCareManagmentSystem.Domain.Entities;

public class TreatmentPlan
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedById { get; set; }
    public bool IsCompleted { get; set; } // Added

    public virtual Patient? Patient { get; set; }
    public virtual User? CreatedBy { get; set; }
    public virtual ICollection<TreatmentItem> Items { get; set; } = new List<TreatmentItem>();
}
