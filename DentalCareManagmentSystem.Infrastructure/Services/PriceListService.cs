
using DentalCareManagmentSystem.Application.DTOs;
using DentalCareManagmentSystem.Application.Interfaces;
using DentalCareManagmentSystem.Domain.Entities;
using DentalCareManagmentSystem.Infrastructure.Data;

namespace DentalCareManagmentSystem.Infrastructure.Services;

public class PriceListService : IPriceListService
{
    private readonly ClinicDbContext _context;

    public PriceListService(ClinicDbContext context)
    {
        _context = context;
    }

    public void Create(PriceListItemDto itemDto)
    {
        var item = new PriceListItem
        {
            Name = itemDto.Name,
            Category = itemDto.Category,
            DefaultPrice = itemDto.DefaultPrice,
            IsActive = true
        };
        _context.PriceListItems.Add(item);
        _context.SaveChanges();
    }

    public void Delete(Guid id)
    {
        var item = _context.PriceListItems.Find(id);
        if (item != null)
        {
            item.IsActive = false; // Soft delete
            _context.SaveChanges();
        }
    }

    public IQueryable<PriceListItemDto> GetAll()
    {
        return _context.PriceListItems.Where(i => i.IsActive).Select(i => new PriceListItemDto
        {
            Id = i.Id,
            Name = i.Name,
            Category = i.Category,
            DefaultPrice = i.DefaultPrice,
            IsActive = i.IsActive
        });
    }

    public PriceListItemDto GetById(Guid id)
    {
        return GetAll().FirstOrDefault(i => i.Id == id);
    }

    public void Update(PriceListItemDto itemDto)
    {
        var item = _context.PriceListItems.Find(itemDto.Id);
        if (item != null)
        {
            item.Name = itemDto.Name;
            item.Category = itemDto.Category;
            item.DefaultPrice = itemDto.DefaultPrice;
            item.IsActive = itemDto.IsActive;
            _context.SaveChanges();
        }
    }
}
