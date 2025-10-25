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
ClinicDbContext clinicDbContext
        )
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
        if (treatmentPlan == null) return NotFound();

        ViewBag.PriceListItems = _priceListService.GetAll()?.ToList() ?? new List<PriceListItemDto>();
        return View(treatmentPlan);
    }

    public IActionResult Create(Guid? patientId)
    {
        ViewBag.Patients = new SelectList(_patientService.GetAll(), "Id", "FullName", patientId);
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(TreatmentPlanDto treatmentPlanDto)
    {
        if (ModelState.IsValid)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
            var planId = _treatmentPlanService.CreatePlan(treatmentPlanDto.PatientId, userId);
            return RedirectToAction(nameof(Details), new { id = planId });
        }
        
        ViewBag.Patients = new SelectList(_patientService.GetAll(), "Id", "FullName", treatmentPlanDto.PatientId);
        return View(treatmentPlanDto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreatePlan(Guid patientId)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
            _treatmentPlanService.CreatePlan(patientId, userId);
        }

        return RedirectToAction("Details", "Patients", new { id = patientId });
    }




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
        return RedirectToAction("Details", new { id = planId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateItemQuantity(Guid itemId, int quantity)
    {
        var planId = _context.TreatmentItems.Find(itemId)?.TreatmentPlanId;
        _treatmentPlanService.UpdateItemQuantity(itemId, quantity);
        return RedirectToAction("Details", new { id = planId });
    }

    [HttpGet]
    public IActionResult GetPriceListItems()
    {
        var items = _priceListService.GetAll().ToList();
        return Json(items);
    }

    [HttpPost]
    public IActionResult CompletePlan(Guid planId)
    {
        _treatmentPlanService.CompletePlan(planId);
        return Ok();
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

    //[HttpGet]
    //public IActionResult GetPlansByPatient(Guid patientId)
    //{
    //    var plans = _treatmentPlanService.GetPlansByPatientId(patientId);
    //    return Json(plans);
    //}
    [HttpGet]
    public IActionResult GetPlansByPatient(Guid patientId)
    {
        var plans = _treatmentPlanService.GetPlansByPatientId(patientId);
        return PartialView("~/Views/Patients/_TreatmentPlans.cshtml", plans);
    }
}
