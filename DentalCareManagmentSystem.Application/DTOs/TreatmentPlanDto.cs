
namespace DentalCareManagmentSystem.Application.DTOs;

public class TreatmentPlanDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public decimal TotalCost { get; set; }
    public bool IsCompleted { get; set; }
    public List<TreatmentItemDto> Items { get; set; } = new();
    public string? PatientName { get; set; }

}

public class TreatmentItemDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
    public string? PriceListItemName { get; set; }

}
