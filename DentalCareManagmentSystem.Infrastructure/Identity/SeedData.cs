
using DentalCareManagmentSystem.Domain.Entities;
using DentalCareManagmentSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace DentalCareManagmentSystem.Infrastructure.Identity;

public static class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ClinicDbContext>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

        string[] roleNames = { "SystemAdmin", "Doctor", "Receptionist" };
        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        await CreateUser(userManager, "admin@clinic.local", "Admin@123", "SystemAdmin", "Admin User");
        await CreateUser(userManager, "doctor@clinic.local", "Doctor@123", "Doctor", "Doctor User");
        await CreateUser(userManager, "reception@clinic.local", "Reception@123", "Receptionist", "Receptionist User");
    }
    public static void SeedTreatmentPlans(ClinicDbContext context)
    {
        if (!context.TreatmentPlans.Any())
        {
            var plan1 = new TreatmentPlan
            {
                PatientId = Guid.NewGuid(),
                CreatedById = "some-user-id",
                CreatedAt = DateTime.UtcNow,
                IsCompleted = true,
                Items = new List<TreatmentItem>
            {
                new TreatmentItem { NameSnapshot = "Cleaning", PriceSnapshot = 50, Quantity = 1},
                new TreatmentItem { NameSnapshot = "Filling", PriceSnapshot = 100, Quantity =5  }
            }
            };

            var plan2 = new TreatmentPlan
            {
                PatientId = Guid.NewGuid(),
                CreatedById = "some-user-id",
                CreatedAt = DateTime.UtcNow.AddMonths(-1),
                IsCompleted = false,
                Items = new List<TreatmentItem>
            {
                new TreatmentItem { NameSnapshot = "Extraction", PriceSnapshot = 150, Quantity = 1 }
            }
            };

            context.TreatmentPlans.AddRange(plan1, plan2);
            context.SaveChanges();
        }
    }

    private static async Task CreateUser(UserManager<User> userManager, string email, string password, string role, string fullName)
    {
        if (await userManager.FindByEmailAsync(email) == null)
        {
            var user = new User
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }
    }
}
