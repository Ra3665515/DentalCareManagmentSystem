using DentalCareManagmentSystem.Application.DTOs;
using DentalCareManagmentSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DentalCareManagmentSystem.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "SystemAdmin")]
public class PriceListController : Controller
{
    private readonly IPriceListService _priceListService;

    public PriceListController(IPriceListService priceListService)
    {
        _priceListService = priceListService;
    }

    public IActionResult Index()
    {
        var items = _priceListService.GetAll().ToList();
        return View(items);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(PriceListItemDto itemDto)
    {
        if (ModelState.IsValid)
        {
            _priceListService.Create(itemDto);
            return RedirectToAction(nameof(Index));
        }
        return View(itemDto);
    }

    public IActionResult Edit(Guid id)
    {
        var item = _priceListService.GetById(id);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(PriceListItemDto itemDto)
    {
        if (ModelState.IsValid)
        {
            _priceListService.Update(itemDto);
            return RedirectToAction(nameof(Index));
        }
        return View(itemDto);
    }

    public IActionResult Delete(Guid id)
    {
        var item = _priceListService.GetById(id);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(Guid id)
    {
        _priceListService.Delete(id);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult GetPriceListGrid()
    {
        var items = _priceListService.GetAll().ToList();
        return PartialView("_PriceListGrid", items);
    }
}
