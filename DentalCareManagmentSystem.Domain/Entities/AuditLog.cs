
namespace DentalCareManagmentSystem.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; set; }
    public string? EntityName { get; set; }
    public string? EntityId { get; set; }
    public string? Action { get; set; }
    public string? UserId { get; set; }
    public DateTime Timestamp { get; set; }
    public string? ChangesJson { get; set; }

    public virtual User? User { get; set; }
}
