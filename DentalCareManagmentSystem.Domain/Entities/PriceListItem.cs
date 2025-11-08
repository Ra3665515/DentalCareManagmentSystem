
namespace DentalCareManagmentSystem.Domain.Entities;

public class PriceListItem
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Category { get; set; }
    public decimal DefaultPrice { get; set; }
    public bool IsActive { get; set; }
}
