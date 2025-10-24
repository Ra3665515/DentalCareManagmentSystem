
namespace DentalCareManagmentSystem.Application.DTOs;

public class PatientDto
{
    public Guid Id { get; set; }
    public string? FullName { get; set; }
    public int Age { get; set; }
    public string? Phone { get; set; }
    public string? Gender { get; set; }
    public string? Notes { get; set; }
    public decimal TotalDue { get; set; }
}
