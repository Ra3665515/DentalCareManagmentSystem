using DentalCareManagmentSystem.Application.Interfaces;
using DentalCareManagmentSystem.Domain.Entities;
using DentalCareManagmentSystem.Infrastructure.Data;
using DentalCareManagmentSystem.Infrastructure.Identity;
using DentalCareManagmentSystem.Infrastructure.Services;
using DentalCareManagmentSystem.Web.Hubs;
using DentalCareManagmentSystem.Web.Middleware;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// 1️⃣ Configure Services
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ClinicDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddIdentity<User, IdentityRole>(options =>
    options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ClinicDbContext>()
    .AddDefaultTokenProviders();

// Localization
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddControllersWithViews()
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix);
//.AddDataAnnotationsLocalization(options =>
//{
//    options.DataAnnotationLocalizerProvider = (type, factory) =>
//        factory.Create(typeof(SharedResource));
//});
builder.Services.AddRazorPages()
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization();

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("en"),
        new CultureInfo("ar")
    };
    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders.Insert(0, new RouteDataRequestCultureProvider());
});

// Register Application Services
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IDiagnosisService, DiagnosisService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IPriceListService, PriceListService>();
builder.Services.AddScoped<ITreatmentPlanService, TreatmentPlanService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Configure Identity Cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// SignalR services
builder.Services.AddSignalR();

var app = builder.Build();

// 2️⃣ Configure Middleware
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Localization Middleware - Must be before Authorization
var localizationOption = app.Services.GetService<IOptions<RequestLocalizationOptions>>();
app.UseRequestLocalization(localizationOption.Value);

app.UseAuthentication();
app.UseAuthorization();

// 3️⃣ SignalR Hub Mapping
app.MapHub<NotificationHub>("/notificationHub");

// Routes with Culture Support
app.MapControllerRoute(
    name: "areas_localized",
    pattern: "{culture=en}/{area:exists}/{controller=Home}/{action=Index}/{id?}",
    constraints: new { culture = @"^(en|ar)$" });

// New route for culture-only URLs
app.MapControllerRoute(
    name: "culture_only",
    pattern: "{culture=en}",
    defaults: new { controller = "Home", action = "Index" },
    constraints: new { culture = @"^(en|ar)$" });

app.MapControllerRoute(
    name: "default_localized",
    pattern: "{culture=en}/{controller}/{action=Index}/{id?}",
    constraints: new { culture = @"^(en|ar)$" });

// Fallback routes without culture (redirect to default culture)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Fallback route for non-localized URLs, redirects to default culture
app.MapControllerRoute(
    name: "root_redirect",
    pattern: "/",
    defaults: new { controller = "Home", action = "RedirectToLocalized" });

app.MapRazorPages();

// 4️⃣ Seed Data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await SeedData.Initialize(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the DB.");
    }
}

app.Run();
