using DentalCareManagmentSystem.Application.DTOs;
using DentalCareManagmentSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DentalCareManagmentSystem.Web.Controllers;

[Authorize(Roles = "Receptionist,Doctor,SystemAdmin")]
public class PatientsController : Controller
{
    private readonly IPatientService _patientService;
    private readonly IDiagnosisService _diagnosisService;
    private readonly IImageService _imageService;
    private readonly ITreatmentPlanService _treatmentPlanService;

    public PatientsController(IPatientService patientService, IDiagnosisService diagnosisService, IImageService imageService, ITreatmentPlanService treatmentPlanService)
    {
        _patientService = patientService;
        _diagnosisService = diagnosisService;
        _imageService = imageService;
        _treatmentPlanService = treatmentPlanService;
    }

   
    public IActionResult Index()
    {
        var patients = _patientService.GetPatientsWithTotalDue();
        return View(patients);
    }

    [HttpGet]
    public IActionResult GetPatientsGrid()
    {
        var patients = _patientService.GetAll().Select(p => new PatientDto
        {
            Id = p.Id,
            FullName = p.FullName,
            Phone = p.Phone,
            Age = p.Age,
            Gender = p.Gender.ToString(),
            Notes = p.Notes,
            TotalDue = _patientService.GetById(p.Id).TotalDue
        }).ToList();
        return PartialView("_PatientsGrid", patients);
    }

    public IActionResult Create()
    {
        ViewBag.Genders = new SelectList(Enum.GetNames(typeof(Domain.Enums.Gender)));
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(PatientDto patientDto)
    {
        if (ModelState.IsValid)
        {
            _patientService.Create(patientDto);
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Genders = new SelectList(Enum.GetNames(typeof(Domain.Enums.Gender)));
        return View(patientDto);
    }

    public IActionResult Edit(Guid id)
    {
        var patient = _patientService.GetById(id);
        if (patient == null) return NotFound();
        ViewBag.Genders = new SelectList(Enum.GetNames(typeof(Domain.Enums.Gender)), patient.Gender);
        return View(patient);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(PatientDto patientDto)
    {
        if (ModelState.IsValid)
        {
            _patientService.Update(patientDto);
            return RedirectToAction(nameof(Index));
        }
        ViewBag.Genders = new SelectList(Enum.GetNames(typeof(Domain.Enums.Gender)), patientDto.Gender);
        return View(patientDto);
    }

    public IActionResult Delete(Guid id)
    {
        var patient = _patientService.GetById(id);
        if (patient == null) return NotFound();
        return View(patient);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(Guid id)
    {
        _patientService.Delete(id);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Details(Guid id)
    {
        var patient = _patientService.GetById(id);
        if (patient == null) return NotFound();

        ViewBag.DiagnosisNotes = _diagnosisService.GetNotesByPatientId(id);
        ViewBag.TreatmentPlans = _treatmentPlanService.GetPlansByPatientId(id);
        ViewBag.PatientImages = _imageService.GetImagesByPatientId(id);


        return View(patient);
    }


    // AJAX partials for patient details tabs
    public IActionResult GetDiagnosisNotes(Guid patientId)
    {
        var notes = _diagnosisService.GetNotesByPatientId(patientId);
        return PartialView("_DiagnosisNotes", notes);
    }

    public IActionResult GetPatientImages(Guid patientId)
    {
        var images = _imageService.GetImagesByPatientId(patientId);
        return PartialView("_PatientImages", images);
    }

    public IActionResult GetTreatmentPlans(Guid patientId)
    {
        var plans = _treatmentPlanService.GetPlansByPatientId(patientId);
        return PartialView("_TreatmentPlans", plans);
    }

    [HttpPost]
    [Authorize(Roles = "Doctor")]
    public IActionResult AddDiagnosisNote(Guid patientId, string note)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            _diagnosisService.AddNote(patientId, User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value, note);
            return Ok();
        }
        return Unauthorized();
    }

    [HttpPost]
    [Authorize(Roles = "Doctor")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> UploadPatientImage(Guid patientId, IFormFile imageFile)
    {
        if (imageFile != null && imageFile.Length > 0)
        {
            await _imageService.UploadImageAsync(patientId, imageFile.OpenReadStream(), imageFile.FileName);
        }

        return RedirectToAction("Details", new { id = patientId });
    }
    [HttpPost]
    [Authorize(Roles = "Doctor")]
    [ValidateAntiForgeryToken]
    public IActionResult CreateTreatmentPlan(Guid patientId)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        _treatmentPlanService.CreatePlan(patientId, userId);

        return RedirectToAction("Details", new { id = patientId });
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Doctor")]
    public IActionResult DeletePatientImage(Guid patientId, Guid imageId)
    {
        _imageService.DeleteImage(imageId);
        return RedirectToAction("Details", new { id = patientId });
    }

}
