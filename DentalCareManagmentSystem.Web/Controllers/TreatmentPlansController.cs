using DentalCareManagmentSystem.Application.DTOs;
using DentalCareManagmentSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DentalCareManagmentSystem.Web.Controllers;

[Authorize(Roles = "Doctor,SystemAdmin")]
public class TreatmentPlansController : Controller
{
    private readonly ITreatmentPlanService _treatmentPlanService;
    private readonly IPriceListService _priceListService;
    private readonly IPatientService _patientService;

    public TreatmentPlansController(
        ITreatmentPlanService treatmentPlanService, 
        IPriceListService priceListService,
        IPatientService patientService)
    {
        _treatmentPlanService = treatmentPlanService;
        _priceListService = priceListService;
        _patientService = patientService;
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
        
        ViewBag.PriceListItems = _priceListService.GetAll().ToList();
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
    public IActionResult CreatePlan(Guid patientId)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var planId = _treatmentPlanService.CreatePlan(patientId, User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            return Ok(new { planId = planId });
        }
        return Unauthorized();
    }

    [HttpPost]
    public IActionResult AddItem(Guid planId, Guid priceListItemId, int quantity)
    {
        _treatmentPlanService.AddItemToPlan(planId, priceListItemId, quantity);
        return Ok();
    }

    [HttpPost]
    public IActionResult DeleteItem(Guid itemId)
    {
        _treatmentPlanService.RemoveItemFromPlan(itemId);
        return Ok();
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

    [HttpPost]
    public IActionResult UpdateItemQuantity(Guid itemId, int quantity)
    {
        _treatmentPlanService.UpdateItemQuantity(itemId, quantity);
        return Ok();
    }

    public IActionResult Delete(Guid id)
    {
        var treatmentPlan = _treatmentPlanService.GetById(id);
        if (treatmentPlan == null) return NotFound();
        
        return View(treatmentPlan);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(Guid id)
    {
        var treatmentPlan = _treatmentPlanService.GetById(id);
        if (treatmentPlan != null)
        {
            _treatmentPlanService.DeletePlan(id);
            return RedirectToAction("Details", "Patients", new { id = treatmentPlan.PatientId });
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
