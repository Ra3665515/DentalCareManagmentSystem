using DentalCareManagmentSystem.Application.DTOs;
using DentalCareManagmentSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DentalCareManagmentSystem.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "SystemAdmin")]
public class UsersController : Controller
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    public IActionResult Index()
    {
        var users = _userService.GetAll().ToList();
        return View(users);
    }

    [HttpGet]
    public IActionResult GetUsersGrid()
    {
        var users = _userService.GetAll().ToList();
        return PartialView("_UsersGrid", users);
    }

    public IActionResult Create()
    {
        ViewBag.Roles = new SelectList(new List<string> { "SystemAdmin", "Doctor", "Receptionist" });
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserDto userDto, string password)
    {
        if (ModelState.IsValid)
        {
            await _userService.CreateAsync(userDto, password, userDto.Role!);
            return RedirectToAction(nameof(Index));
        }
        ViewBag.Roles = new SelectList(new List<string> { "SystemAdmin", "Doctor", "Receptionist" }, userDto.Role);
        return View(userDto);
    }

    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null) return NotFound();
        ViewBag.Roles = new SelectList(new List<string> { "SystemAdmin", "Doctor", "Receptionist" }, user.Role);
        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UserDto userDto)
    {
        if (ModelState.IsValid)
        {
            await _userService.UpdateAsync(userDto);
            return RedirectToAction(nameof(Index));
        }
        ViewBag.Roles = new SelectList(new List<string> { "SystemAdmin", "Doctor", "Receptionist" }, userDto.Role);
        return View(userDto);
    }

    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null) return NotFound();
        return View(user);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        await _userService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
