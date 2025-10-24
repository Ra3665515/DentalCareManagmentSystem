using DentalCareManagmentSystem.Application.DTOs;
using DentalCareManagmentSystem.Application.Interfaces;
using DentalCareManagmentSystem.Domain.Entities;
using DentalCareManagmentSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DentalCareManagmentSystem.Infrastructure.Services;

public class PatientService : IPatientService
{
    private readonly ClinicDbContext _context;

    public PatientService(ClinicDbContext context)
    {
        _context = context;
    }

    public void Create(PatientDto patientDto)
    {
        var patient = new Patient
        {
            FullName = patientDto.FullName,
            Age = patientDto.Age,
            Phone = patientDto.Phone,
            Gender = Enum.Parse<Domain.Enums.Gender>(patientDto.Gender),
            Notes = patientDto.Notes,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        _context.Patients.Add(patient);
        _context.SaveChanges();
    }

    public void Delete(Guid id)
    {
        var patient = _context.Patients.Find(id);
        if (patient != null)
        {
            patient.IsActive = false; // Soft delete
            _context.SaveChanges();
        }
    }

    public IQueryable<Patient> GetAll()
    {
        return _context.Patients.Where(p => p.IsActive);
    }

    public PatientDto GetById(Guid id)
    {
        var patient = _context.Patients.Find(id);
        if (patient == null) return null;

        return new PatientDto
        {
            Id = patient.Id,
            FullName = patient.FullName,
            Age = patient.Age,
            Phone = patient.Phone,
            Gender = patient.Gender.ToString(),
            Notes = patient.Notes,
            TotalDue = _context.TreatmentPlans
                .Where(tp => tp.PatientId == id)
                .SelectMany(tp => tp.Items)
        .AsEnumerable()
                .Sum(i => i.LineTotal)
        };
    }


    public List<PatientDto> GetRecentPatients()
    {
        return _context.Patients
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .Select(p => new PatientDto
            {
                Id = p.Id,
                FullName = p.FullName,
                Age = p.Age,
                Phone = p.Phone,
                Gender = p.Gender.ToString(),
                Notes = p.Notes
            }).ToList();
    }

    public List<PatientDto> GetActivePatients()
    {
        return _context.Patients.Where(p => p.IsActive)
            .Select(p => new PatientDto
            {
                Id = p.Id,
                FullName = p.FullName,
                Age = p.Age,
                Phone = p.Phone,
                Gender = p.Gender.ToString(),
                Notes = p.Notes
            }).ToList();
    }

    public List<PatientDto> GetNewPatientsThisMonth()
    {
        return _context.Patients
            .Where(p => p.CreatedAt.Month == DateTime.UtcNow.Month && p.CreatedAt.Year == DateTime.UtcNow.Year)
            .Select(p => new PatientDto
            {
                Id = p.Id,
                FullName = p.FullName,
                Age = p.Age,
                Phone = p.Phone,
                Gender = p.Gender.ToString(),
                
                Notes = p.Notes
            }).ToList();
    }

    public Dictionary<string, int> GetPatientCountByGender()
    {
        return _context.Patients
            .GroupBy(p => p.Gender.ToString())
            .Select(g => new { Gender = g.Key, Count = g.Count() })
            .ToDictionary(x => x.Gender, x => x.Count);
    }

    public Dictionary<string, int> GetPatientCountByAgeGroup()
    {
        return _context.Patients
            .AsEnumerable() 
            .GroupBy(p => $"{(p.Age / 10) * 10}-{(p.Age / 10) * 10 + 9}")
            .Select(g => new { AgeGroup = g.Key, Count = g.Count() })
            .ToDictionary(x => x.AgeGroup, x => x.Count);
    }


    /// <summary>




    public void Update(PatientDto patientDto)
    {
        var patient = _context.Patients.Find(patientDto.Id);
        if (patient != null)
        {
            patient.FullName = patientDto.FullName;
            patient.Age = patientDto.Age;
            patient.Phone = patientDto.Phone;
            patient.Gender = Enum.Parse<Domain.Enums.Gender>(patientDto.Gender);

            patient.Notes = patientDto.Notes;
            _context.SaveChanges();
        }
    }
    public List<PatientDto> GetPatientsWithTotalDue()
    {
        var patients = _context.Patients
            .Where(p => p.IsActive)
            .Include(p => p.TreatmentPlans)
                .ThenInclude(tp => tp.Items) // Include كل الـ TreatmentItems
            .ToList(); 

        return patients.Select(p => new PatientDto
        {
            Id = p.Id,
            FullName = p.FullName,
            Age = p.Age,
            Phone = p.Phone,
            Gender = p.Gender.ToString(),
            Notes = p.Notes,
            TotalDue = p.TreatmentPlans
                .SelectMany(tp => tp.Items)
                .Sum(i => i.LineTotal) // دلوقتي الحساب هيشتغل
        }).ToList();
    }


}