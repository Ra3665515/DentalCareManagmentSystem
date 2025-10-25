using DentalCareManagmentSystem.Application.DTOs;
using DentalCareManagmentSystem.Application.Interfaces;
using DentalCareManagmentSystem.Domain.Entities;
using DentalCareManagmentSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DentalCareManagmentSystem.Infrastructure.Services;

public class TreatmentPlanService : ITreatmentPlanService
{
    private readonly ClinicDbContext _context;

    public TreatmentPlanService(ClinicDbContext context)
    {
        _context = context;
    }

    public IQueryable<TreatmentPlanDto> GetAll()
    {
        return GetPlansQuery();
    }

    public Guid CreatePlan(Guid patientId, string createdById)
    {
        var plan = new TreatmentPlan
        {
            PatientId = patientId,
            CreatedById = createdById,
            CreatedAt = DateTime.UtcNow
        };
        _context.TreatmentPlans.Add(plan);
        _context.SaveChanges();
        return plan.Id;
    }

    public void AddItemToPlan(Guid planId, Guid priceListItemId, int quantity)
    {
        var priceListItem = _context.PriceListItems.Find(priceListItemId);
        if (priceListItem == null) return;

        var item = new TreatmentItem
        {
            TreatmentPlanId = planId,
            PriceListItemId = priceListItemId,
            NameSnapshot = priceListItem.Name,
            PriceSnapshot = priceListItem.DefaultPrice,
            Quantity = quantity
        };
        _context.TreatmentItems.Add(item);
        _context.SaveChanges();
    }

    public void RemoveItemFromPlan(Guid itemId)
    {
        var item = _context.TreatmentItems.Find(itemId);
        if (item != null)
        {
            _context.TreatmentItems.Remove(item);
            _context.SaveChanges();
        }
    }

    public void CompletePlan(Guid planId)
    {
        var plan = _context.TreatmentPlans.Find(planId);
        if (plan != null)
        {
            plan.IsCompleted = true;
            _context.SaveChanges();
        }
    }

    public void UpdateItemQuantity(Guid itemId, int quantity)
    {
        var item = _context.TreatmentItems.Find(itemId);
        if (item != null)
        {
            item.Quantity = quantity;
            _context.SaveChanges();
        }
    }

    public void DeletePlan(Guid id)
    {
        var plan = _context.TreatmentPlans.Find(id);
        if (plan != null)
        {
            _context.TreatmentPlans.Remove(plan);
            _context.SaveChanges();
        }
    }

    public decimal GetTotalRevenue()
    {
        return _context.TreatmentPlans
            .Where(p => p.IsCompleted)
            .Include(p => p.Items)
            .AsEnumerable()
            .Sum(p => p.Items.Sum(i => i.LineTotal));
    }

    public decimal GetRevenueThisMonth()
    {
        return _context.TreatmentPlans
            .Where(p => p.IsCompleted &&
                        p.CreatedAt.Month == DateTime.UtcNow.Month &&
                        p.CreatedAt.Year == DateTime.UtcNow.Year)
            .Include(p => p.Items)
            .AsEnumerable()
            .Sum(p => p.Items.Sum(i => i.LineTotal));
    }

    public decimal GetOutstandingPayments()
    {
        return _context.TreatmentPlans
            .Where(p => !p.IsCompleted)
            .Include(p => p.Items)
            .AsEnumerable()
            .Sum(p => p.Items.Sum(i => i.LineTotal));
    }

    public Dictionary<string, decimal> GetRevenueByTreatmentType()
    {
        return _context.TreatmentItems
            .AsEnumerable()
            .GroupBy(item => item.NameSnapshot ?? "Unknown")
            .Select(g => new
            {
                TreatmentType = g.Key,
                Revenue = g.Sum(item => item.LineTotal)
            })
            .ToDictionary(x => x.TreatmentType, x => x.Revenue);
    }

    public Dictionary<string, decimal> GetMonthlyRevenue()
    {
        return _context.TreatmentPlans
            .Where(p => p.IsCompleted)
            .Include(p => p.Items)
            .AsEnumerable()
            .GroupBy(p => new { p.CreatedAt.Year, p.CreatedAt.Month })
            .Select(g => new
            {
                Month = $"{g.Key.Year}-{g.Key.Month:00}",
                Revenue = g.Sum(p => p.Items.Sum(i => i.LineTotal))
            })
            .OrderBy(x => x.Month)
            .ToDictionary(x => x.Month, x => x.Revenue);
    }

    public TreatmentPlanDto? GetById(Guid id)
    {
        return GetPlansQuery().FirstOrDefault(p => p.Id == id);
    }

    public List<TreatmentPlanDto> GetPlansByPatientId(Guid patientId)
    {
        return _context.TreatmentPlans
            .Include(p => p.Items)
            .Include(p => p.CreatedBy)
            .Where(p => p.PatientId == patientId)
            .AsEnumerable() 
            .Select(p => new TreatmentPlanDto
            {
                Id = p.Id,
                PatientId = p.PatientId,
                CreatedAt = p.CreatedAt,
                CreatedBy = p.CreatedBy?.FullName ?? "Unknown",
                Items = p.Items
                    .Select(i => new TreatmentItemDto
                    {
                        Id = i.Id,
                        Name = i.NameSnapshot,
                        Price = i.PriceSnapshot,
                        Quantity = i.Quantity,
                        LineTotal = i.Quantity * i.PriceSnapshot
                    })
                    .ToList(),
                TotalCost = p.Items.Sum(i => i.Quantity * i.PriceSnapshot)
            })
            .ToList();
    }



    private IQueryable<TreatmentPlanDto> GetPlansQuery()
    {
        return _context.TreatmentPlans
            .Include(tp => tp.Items)
            .Include(tp => tp.CreatedBy)
            .Select(tp => new TreatmentPlanDto
            {
                Id = tp.Id,
                PatientId = tp.PatientId,
                CreatedAt = tp.CreatedAt,
                CreatedBy = tp.CreatedBy != null ? tp.CreatedBy.FullName : "Unknown",
                TotalCost = tp.Items
                    .AsEnumerable()
                    .Sum(i => i.LineTotal),
                Items = tp.Items.Select(i => new TreatmentItemDto
                {
                    Id = i.Id,
                    Name = i.NameSnapshot,
                    Price = i.PriceSnapshot,
                    Quantity = i.Quantity,
                    LineTotal = i.LineTotal
                }).ToList()
            });
    }
}
