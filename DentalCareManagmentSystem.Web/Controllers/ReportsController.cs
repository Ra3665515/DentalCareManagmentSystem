using DentalCareManagmentSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DentalCareManagmentSystem.Web.Controllers;

[Authorize(Roles = "Doctor,SystemAdmin")]
public class ReportsController : Controller
{
    private readonly IPatientService _patientService;
    private readonly IAppointmentService _appointmentService;
    private readonly ITreatmentPlanService _treatmentPlanService;
    private readonly IPriceListService _priceListService;

    public ReportsController(
        IPatientService patientService,
        IAppointmentService appointmentService,
        ITreatmentPlanService treatmentPlanService,
        IPriceListService priceListService)
    {
        _patientService = patientService;
        _appointmentService = appointmentService;
        _treatmentPlanService = treatmentPlanService;
        _priceListService = priceListService;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult PatientReport()
    {
        var viewModel = new PatientReportViewModel
        {
            TotalPatients = _patientService.GetAll().Count(),
            ActivePatients = _patientService.GetActivePatients().Count(),
            NewPatientsThisMonth = _patientService.GetNewPatientsThisMonth().Count(),
            PatientsByGender = _patientService.GetPatientCountByGender(),
            PatientsByAgeGroup = _patientService.GetPatientCountByAgeGroup()
        };

        return View(viewModel);
    }

    public IActionResult AppointmentReport()
    {
        var viewModel = new AppointmentReportViewModel
        {
            TotalAppointments = _appointmentService.GetAll().Count(),
            CompletedAppointments = _appointmentService.GetCompletedAppointments().Count(),
            CancelledAppointments = _appointmentService.GetCancelledAppointments().Count(),
            AppointmentsThisMonth = _appointmentService.GetAppointmentsThisMonth().Count(),
            AppointmentsByStatus = _appointmentService.GetAppointmentCountByStatus(),
            AppointmentsByMonth = _appointmentService.GetAppointmentsByMonth()
        };

        return View(viewModel);
    }

    public IActionResult FinancialReport()
    {
        var viewModel = new FinancialReportViewModel
        {
            TotalRevenue = _treatmentPlanService.GetTotalRevenue(),
            RevenueThisMonth = _treatmentPlanService.GetRevenueThisMonth(),
            OutstandingPayments = _treatmentPlanService.GetOutstandingPayments(),
            RevenueByTreatment = _treatmentPlanService.GetRevenueByTreatmentType(),
            MonthlyRevenue = _treatmentPlanService.GetMonthlyRevenue()
        };

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult ExportPatients()
    {
        var patients = _patientService.GetAll();
        // Implementation for CSV/Excel export would go here
        return Json(patients);
    }

    [HttpGet]
    public IActionResult ExportAppointments(DateTime? startDate, DateTime? endDate)
    {
        var appointments = _appointmentService.GetAppointmentsByDateRange(startDate ?? DateTime.MinValue, endDate ?? DateTime.MaxValue);
        // Implementation for CSV/Excel export would go here
        return Json(appointments);
    }
}

// View Models for Reports
public class PatientReportViewModel
{
    public int TotalPatients { get; set; }
    public int ActivePatients { get; set; }
    public int NewPatientsThisMonth { get; set; }
    public Dictionary<string, int> PatientsByGender { get; set; } = new();
    public Dictionary<string, int> PatientsByAgeGroup { get; set; } = new();
}

public class AppointmentReportViewModel
{
    public int TotalAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public int CancelledAppointments { get; set; }
    public int AppointmentsThisMonth { get; set; }
    public Dictionary<string, int> AppointmentsByStatus { get; set; } = new();
    public Dictionary<string, int> AppointmentsByMonth { get; set; } = new();
}

public class FinancialReportViewModel
{
    public decimal TotalRevenue { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public decimal OutstandingPayments { get; set; }
    public Dictionary<string, decimal> RevenueByTreatment { get; set; } = new();
    public Dictionary<string, decimal> MonthlyRevenue { get; set; } = new();
}