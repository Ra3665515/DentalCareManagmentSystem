
using DentalCareManagmentSystem.Application.DTOs;
using DentalCareManagmentSystem.Domain.Entities;

namespace DentalCareManagmentSystem.Application.Interfaces;

public interface IPatientService
{
    PatientDto GetById(Guid id);
    IQueryable<Patient> GetAll();
    List<PatientDto> GetRecentPatients(); // Added
    List<PatientDto> GetActivePatients(); // Added
    List<PatientDto> GetNewPatientsThisMonth(); // Added
    Dictionary<string, int> GetPatientCountByGender(); // Added
    Dictionary<string, int> GetPatientCountByAgeGroup(); // Added
    List<PatientDto> GetPatientsWithTotalDue();

    void Create(PatientDto patient);
    void Update(PatientDto patient);
    void Delete(Guid id);
}
