
using DentalCareManagmentSystem.Application.DTOs;

namespace DentalCareManagmentSystem.Application.Interfaces;

public interface ITreatmentPlanService
{
    IQueryable<TreatmentPlanDto> GetAll(); // Added
    TreatmentPlanDto GetById(Guid id);
    List<TreatmentPlanDto> GetPlansByPatientId(Guid patientId);
    Guid CreatePlan(Guid patientId, string createdById);
    void AddItemToPlan(Guid planId, Guid priceListItemId, int quantity);
    void RemoveItemFromPlan(Guid itemId);
    void CompletePlan(Guid planId); // Added
    void UpdateItemQuantity(Guid itemId, int quantity); // Added
    void DeletePlan(Guid id); // Added
    decimal GetTotalRevenue(); // Added
    decimal GetRevenueThisMonth(); // Added
    decimal GetOutstandingPayments(); // Added
    Dictionary<string, decimal> GetRevenueByTreatmentType(); // Added
    Dictionary<string, decimal> GetMonthlyRevenue(); // Added
}
