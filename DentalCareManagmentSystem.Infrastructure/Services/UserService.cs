
using DentalCareManagmentSystem.Application.DTOs;
using DentalCareManagmentSystem.Application.Interfaces;
using DentalCareManagmentSystem.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DentalCareManagmentSystem.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserService(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task CreateAsync(UserDto userDto, string password, string role)
    {
        var user = new User
        {
            FullName = userDto.FullName,
            Email = userDto.Email,
            UserName = userDto.Email,
            EmailConfirmed = true
        };
        var result = await _userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, role);
        }
    }

    public async Task DeleteAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user != null)
        {
            await _userManager.DeleteAsync(user);
        }
    }

    public IQueryable<UserDto> GetAll()
    {
        // This is inefficient for getting roles. A better implementation would involve a join.
        // For simplicity, we'll do it this way, but it's not recommended for production with many users.
        var users = _userManager.Users.ToList();
        var userDtos = new List<UserDto>();
        foreach (var user in users)
        {
            userDtos.Add(new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = _userManager.GetRolesAsync(user).Result.FirstOrDefault()
            });
        }
        return userDtos.AsQueryable();
    }

    public async Task<UserDto> GetByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return null;

        return new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault()
        };
    }

    public async Task UpdateAsync(UserDto userDto)
    {
        var user = await _userManager.FindByIdAsync(userDto.Id);
        if (user != null)
        {
            user.FullName = userDto.FullName;
            user.Email = userDto.Email;
            user.UserName = userDto.Email;
            await _userManager.UpdateAsync(user);

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, userDto.Role);
        }
    }
}
