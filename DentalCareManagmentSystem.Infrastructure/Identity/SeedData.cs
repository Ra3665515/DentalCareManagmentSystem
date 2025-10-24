
using DentalCareManagmentSystem.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace DentalCareManagmentSystem.Infrastructure.Identity;

public static class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
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
