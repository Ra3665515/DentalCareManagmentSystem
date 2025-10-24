
namespace DentalCareManagmentSystem.Domain.Entities;

public class PatientImage
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string? FileName { get; set; }
    public string? FilePath { get; set; }
    public string? ThumbnailPath { get; set; }
    public DateTime UploadedAt { get; set; }

    public virtual Patient? Patient { get; set; }
}
