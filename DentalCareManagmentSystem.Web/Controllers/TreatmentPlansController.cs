using DentalCareManagmentSystem.Application.DTOs;
using DentalCareManagmentSystem.Application.Interfaces;
using DentalCareManagmentSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DentalCareManagmentSystem.Web.Controllers;

[Authorize(Roles = "Doctor,SystemAdmin")]
public class TreatmentPlansController : Controller
{
    private readonly ITreatmentPlanService _treatmentPlanService;
    private readonly IPriceListService _priceListService;
    private readonly IPatientService _patientService;
    private readonly ClinicDbContext _context;

    public TreatmentPlansController(
        ITreatmentPlanService treatmentPlanService,
        IPriceListService priceListService,
        IPatientService patientService,
        ClinicDbContext clinicDbContext)
    {
        _treatmentPlanService = treatmentPlanService;
        _priceListService = priceListService;
        _patientService = patientService;
        _context = clinicDbContext;
    }

    public IActionResult Index()
    {
        var treatmentPlans = _treatmentPlanService.GetAll().ToList();
        return View(treatmentPlans);
    }

    public IActionResult Details(Guid id)
    {
        var treatmentPlan = _treatmentPlanService.GetById(id);
        if (treatmentPlan == null)
        {
            return NotFound();
        }

        ViewBag.PriceListItems = _priceListService.GetAll()?.ToList() ?? new List<PriceListItemDto>();
        return View(treatmentPlan);
    }
    [HttpGet]
    public IActionResult Create(Guid patientId)
    {
        var patient = _patientService.GetById(patientId);
        if (patient == null)
        {
            return NotFound();
        }

        var model = new TreatmentPlanDto
        {
            PatientId = patientId,
            CreatedAt = DateTime.Now,
            TotalCost = 0,
            IsCompleted = false
        };

        ViewBag.PatientName = patient.FullName;
        ViewBag.PatientId = patientId;

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(TreatmentPlanDto treatmentPlanDto)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "User not authenticated!";
                    return RedirectToAction("Details", "Patients", new { id = treatmentPlanDto.PatientId });
                }

                var planId = _treatmentPlanService.CreatePlan(treatmentPlanDto.PatientId, userId);

                if (planId != Guid.Empty)
                {
                    TempData["SuccessMessage"] = "Treatment plan created successfully!";
                    return RedirectToAction("Details", "Patients", new { id = treatmentPlanDto.PatientId });
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to create treatment plan!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error creating treatment plan: {ex.Message}";
            }
        }

        if (treatmentPlanDto.PatientId != Guid.Empty)
        {
            var patient = _patientService.GetById(treatmentPlanDto.PatientId);
            ViewBag.PatientName = patient?.FullName;
            ViewBag.PatientId = treatmentPlanDto.PatientId;
        }

        return View(treatmentPlanDto);
    }
    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public IActionResult CreatePlan(Guid patientId)
    //{
    //    if (User.Identity?.IsAuthenticated == true)
    //    {
    //        return RedirectToAction("Create", "TreatmentPlans", new { patientId = patientId });
    //    }

    //    return Unauthorized();
    //}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddItem(Guid planId, Guid priceListItemId, int quantity)
    {
        _treatmentPlanService.AddItemToPlan(planId, priceListItemId, quantity);
        return RedirectToAction("Details", new { id = planId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteItem(Guid itemId)
    {
        var planId = _context.TreatmentItems.Find(itemId)?.TreatmentPlanId;
        _treatmentPlanService.RemoveItemFromPlan(itemId);

        if (planId.HasValue)
        {
            return RedirectToAction("Details", new { id = planId.Value });
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateItemQuantity(Guid itemId, int quantity)
    {
        var planId = _context.TreatmentItems.Find(itemId)?.TreatmentPlanId;
        _treatmentPlanService.UpdateItemQuantity(itemId, quantity);

        if (planId.HasValue)
        {
            return RedirectToAction("Details", new { id = planId.Value });
        }

        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult GetPriceListItems()
    {
        var items = _priceListService.GetAll().ToList();
        return Json(items);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CompletePlan(Guid planId)
    {
        try
        {
            var plan = _context.TreatmentPlans.Find(planId);
            if (plan != null)
            {
                plan.IsCompleted = true;
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Treatment plan completed successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = " Treatment plan not found!";
            }

            var patientId = plan?.PatientId ?? GetPatientIdFromPlan(planId);
            return RedirectToAction("Details", "Patients", new { id = patientId });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $" Error: {ex.Message}";
            var patientId = GetPatientIdFromPlan(planId);
            return RedirectToAction("Details", "Patients", new { id = patientId });
        }
    }
    public IActionResult Delete(Guid id)
    {
        var treatmentPlan = _treatmentPlanService.GetById(id);
        if (treatmentPlan == null) return NotFound();

        return View(treatmentPlan);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeletePlanConfirmed(Guid id)
    {
        var plan = _treatmentPlanService.GetById(id);
        if (plan != null)
        {
            var patientId = plan.PatientId;
            _treatmentPlanService.DeletePlan(id);
            return RedirectToAction("Details", "Patients", new { id = patientId });
        }
        return NotFound();
    }

    [HttpGet]
    public IActionResult GetPlansByPatient(Guid patientId)
    {
        var plans = _treatmentPlanService.GetPlansByPatientId(patientId);
        return PartialView("~/Views/Patients/_TreatmentPlans.cshtml", plans);
    }

    private Guid? GetPatientIdFromPlan(Guid planId)
    {
        var plan = _context.TreatmentPlans
            .AsNoTracking()
            .FirstOrDefault(p => p.Id == planId);

        return plan?.PatientId;
    }
}