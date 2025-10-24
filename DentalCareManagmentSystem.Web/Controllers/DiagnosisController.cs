using DentalCareManagmentSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DentalCareManagmentSystem.Web.Controllers;

[Authorize(Roles = "Doctor,SystemAdmin")]
public class DiagnosisController : Controller
{
    private readonly IDiagnosisService _diagnosisService;
    private readonly IPatientService _patientService;

    public DiagnosisController(IDiagnosisService diagnosisService, IPatientService patientService)
    {
        _diagnosisService = diagnosisService;
        _patientService = patientService;
    }

    public IActionResult Index()
    {
        var diagnosisNotes = _diagnosisService.GetAll().ToList();
        return View(diagnosisNotes);
    }

    public IActionResult Create(Guid? patientId)
    {
        ViewBag.Patients = new SelectList(_patientService.GetAll(), "Id", "FullName", patientId);
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Guid patientId, string note)
    {
        if (ModelState.IsValid && !string.IsNullOrWhiteSpace(note))
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
            _diagnosisService.AddNote(patientId, userId, note);
            return RedirectToAction("Details", "Patients", new { id = patientId });
        }
        
        ViewBag.Patients = new SelectList(_patientService.GetAll(), "Id", "FullName", patientId);
        ViewBag.Note = note;
        return View();
    }

    public IActionResult Edit(Guid id)
    {
        var diagnosisNote = _diagnosisService.GetById(id);
        if (diagnosisNote == null) return NotFound();
        
        return View(diagnosisNote);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Guid id, string note)
    {
        if (ModelState.IsValid && !string.IsNullOrWhiteSpace(note))
        {
            _diagnosisService.UpdateNote(id, note);
            var diagnosisNote = _diagnosisService.GetById(id);
            return RedirectToAction("Details", "Patients", new { id = diagnosisNote.PatientId });
        }
        
        return View(_diagnosisService.GetById(id));
    }

    public IActionResult Delete(Guid id)
    {
        var diagnosisNote = _diagnosisService.GetById(id);
        if (diagnosisNote == null) return NotFound();
        
        return View(diagnosisNote);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(Guid id)
    {
        var diagnosisNote = _diagnosisService.GetById(id);
        if (diagnosisNote != null)
        {
            _diagnosisService.DeleteNote(id);
            return RedirectToAction("Details", "Patients", new { id = diagnosisNote.PatientId });
        }
        
        return NotFound();
    }

    //[HttpGet]
    //public IActionResult GetNotesByPatient(Guid patientId)
    //{
    //    var notes = _diagnosisService.GetNotesByPatientId(patientId);
    //    return Json(notes);
    //}

    [HttpGet]
    public IActionResult GetNotesByPatient(Guid patientId)
    {
        var notes = _diagnosisService.GetNotesByPatientId(patientId);
        ViewBag.PatientId = patientId; 
        return PartialView("~/Views/Patients/_DiagnosisNotes.cshtml", notes);
    }
    [HttpPost]
    public IActionResult AddNoteAjax(Guid patientId, [FromBody] NoteDto data)
    {
        if (string.IsNullOrWhiteSpace(data.Note))
            return BadRequest();

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        _diagnosisService.AddNote(patientId, userId, data.Note);

        var notes = _diagnosisService.GetNotesByPatientId(patientId);
        ViewBag.PatientId = patientId;
        return PartialView("~/Views/Patients/_DiagnosisNotes.cshtml", notes);
    }

    public class NoteDto
    {
        public string Note { get; set; }
    }


}