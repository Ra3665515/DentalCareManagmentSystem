
using DentalCareManagmentSystem.Domain.Enums;

namespace DentalCareManagmentSystem.Domain.Entities;

public class Patient
{
    public Guid Id { get; set; }
    public string? FullName { get; set; }
    public int Age { get; set; }
    public string? Phone { get; set; }
    public Gender Gender { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public virtual ICollection<DiagnosisNote> DiagnosisNotes { get; set; } = new List<DiagnosisNote>();
    public virtual ICollection<PatientImage> PatientImages { get; set; } = new List<PatientImage>();
    public virtual ICollection<TreatmentPlan> TreatmentPlans { get; set; } = new List<TreatmentPlan>();
}
