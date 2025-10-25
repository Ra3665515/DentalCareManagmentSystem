
using DentalCareManagmentSystem.Application.DTOs;
using Microsoft.AspNetCore.Identity;

namespace DentalCareManagmentSystem.Application.Interfaces;

public interface IUserService
{
    IQueryable<UserDto> GetAll();
    Task<UserDto> GetByIdAsync(string id);
    //Task CreateAsync(UserDto user, string password, string role);
    Task<IdentityResult> CreateAsync(UserDto userDto, string password, string role);

    Task UpdateAsync(UserDto user);
    Task DeleteAsync(string id);
}
