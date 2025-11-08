
namespace DentalCareManagmentSystem.Application.DTOs;

public class PriceListItemDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Category { get; set; }
    public decimal DefaultPrice { get; set; }
    public bool IsActive { get; set; }
}
