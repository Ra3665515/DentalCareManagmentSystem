
using DentalCareManagmentSystem.Application.DTOs;

namespace DentalCareManagmentSystem.Application.Interfaces;

public interface IPriceListService
{
    PriceListItemDto GetById(Guid id);
    IQueryable<PriceListItemDto> GetAll();
    void Create(PriceListItemDto item);
    void Update(PriceListItemDto item);
    void Delete(Guid id);
}
