
using Microsoft.AspNetCore.Identity;

namespace DentalCareManagmentSystem.Domain.Entities;

public class User : IdentityUser
{
    public string? FullName { get; set; }
    // The 'Role' is implicitly handled by Identity's RoleManager and is not a direct property on the User entity.
}
