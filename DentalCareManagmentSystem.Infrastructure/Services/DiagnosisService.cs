using DentalCareManagmentSystem.Application.Interfaces;
using DentalCareManagmentSystem.Domain.Entities;
using DentalCareManagmentSystem.Infrastructure.Data;

namespace DentalCareManagmentSystem.Infrastructure.Services;

public class DiagnosisService : IDiagnosisService
{
    private readonly ClinicDbContext _context;

    public DiagnosisService(ClinicDbContext context)
    {
        _context = context;
    }

    public IQueryable<DiagnosisNote> GetAll()
    {
        return _context.DiagnosisNotes;
    }

    public DiagnosisNote GetById(Guid id)
    {
        return _context.DiagnosisNotes.Find(id);
    }

    public void AddNote(Guid patientId, string doctorId, string note)
    {
        if (string.IsNullOrWhiteSpace(doctorId))
            throw new ArgumentException("DoctorId cannot be null.");

        // تحقق من وجود المريض
        var patient = _context.Patients.Find(patientId);
        if (patient == null)
            throw new InvalidOperationException($"Cannot add note: Patient with Id {patientId} does not exist.");

        // إنشاء ملاحظة جديدة
        var diagnosisNote = new DiagnosisNote
        {
            PatientId = patientId,
            DoctorId = doctorId,
            Note = note,
            CreatedAt = DateTime.UtcNow
        };

        _context.DiagnosisNotes.Add(diagnosisNote);
        _context.SaveChanges();
    }


    public void UpdateNote(Guid id, string note)
    {
        var existingNote = _context.DiagnosisNotes.Find(id);
        if (existingNote != null)
        {
            existingNote.Note = note;
            _context.SaveChanges();
        }
    }

    public void DeleteNote(Guid id)
    {
        var noteToDelete = _context.DiagnosisNotes.Find(id);
        if (noteToDelete != null)
        {
            _context.DiagnosisNotes.Remove(noteToDelete);
            _context.SaveChanges();
        }
    }

    public List<DiagnosisNote> GetNotesByPatientId(Guid patientId)
    {
        return _context.DiagnosisNotes
            .Where(d => d.PatientId == patientId)
            .OrderByDescending(d => d.CreatedAt)
            .ToList();
    }
}
