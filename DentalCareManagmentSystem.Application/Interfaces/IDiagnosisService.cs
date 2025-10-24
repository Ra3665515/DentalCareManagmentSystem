
using DentalCareManagmentSystem.Domain.Entities;

namespace DentalCareManagmentSystem.Application.Interfaces;

public interface IDiagnosisService
{
    IQueryable<DiagnosisNote> GetAll(); // Added
    DiagnosisNote GetById(Guid id); // Added
    void AddNote(Guid patientId, string doctorId, string note);
    void UpdateNote(Guid id, string note); // Added
    void DeleteNote(Guid id); // Added
    List<DiagnosisNote> GetNotesByPatientId(Guid patientId);
}
