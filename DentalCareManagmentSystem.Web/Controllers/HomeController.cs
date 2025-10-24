using DentalCareManagmentSystem.Application.Interfaces;
using DentalCareManagmentSystem.Web.Models; // Added
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DentalCareManagmentSystem.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IPatientService _patientService;
    private readonly IAppointmentService _appointmentService;
    private readonly INotificationService _notificationService;

    public HomeController(IPatientService patientService, IAppointmentService appointmentService, INotificationService notificationService)
    {
        _patientService = patientService;
        _appointmentService = appointmentService;
        _notificationService = notificationService;
    }

    public IActionResult Index()
    {
        var viewModel = new DashboardViewModel
        {
            TotalPatients = _patientService.GetAll().Count(),
            TodayAppointments = _appointmentService.GetTodaysAppointments().Count(),
            PendingAppointments = _appointmentService.GetPendingAppointments().Count(),
            RecentPatients = _patientService.GetRecentPatients(),
            TodayAppointmentsList = _appointmentService.GetTodaysAppointments()
        };

        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }
    //[HttpPost]
    //public IActionResult SetLanguage(string culture, string returnUrl = null)
    //{
    //    Response.Cookies.Append(
    //        CookieRequestCultureProvider.DefaultCookieName,
    //        CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
    //        new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
    //    );

    //    return LocalRedirect(returnUrl ?? "/");
    //}

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}

