using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PerryHomesTracker.Data;
using PerryHomesTracker.Models;

namespace PerryHomesTracker.Controllers;

public class PurchaseInfosController : Controller
{
    private readonly PerryHomesDbContext _context;

    public PurchaseInfosController(PerryHomesDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var list = _context.PurchaseInfos
            .Include(p => p.Home)
            .OrderBy(p => p.Home!.City).ThenBy(p => p.Home!.AddressLine1);
        return View(await list.ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var purchaseInfo = await _context.PurchaseInfos
            .Include(p => p.Home)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (purchaseInfo == null)
            return NotFound();

        return View(purchaseInfo);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateHomesDropdownAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Id,HomeId,ContractDate,ClosingDate,PurchasePrice,Notes")] PurchaseInfo purchaseInfo)
    {
        if (ModelState.IsValid)
        {
            var exists = await _context.PurchaseInfos.AnyAsync(p => p.HomeId == purchaseInfo.HomeId);
            if (exists)
            {
                ModelState.AddModelError(nameof(purchaseInfo.HomeId), "This home already has purchase information.");
                await PopulateHomesDropdownAsync(purchaseInfo.HomeId);
                return View(purchaseInfo);
            }
            _context.Add(purchaseInfo);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        await PopulateHomesDropdownAsync(purchaseInfo.HomeId);
        return View(purchaseInfo);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var purchaseInfo = await _context.PurchaseInfos.FindAsync(id);
        if (purchaseInfo == null)
            return NotFound();

        await PopulateHomesDropdownAsync(purchaseInfo.HomeId, lockedHomeId: purchaseInfo.HomeId);
        return View(purchaseInfo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,HomeId,ContractDate,ClosingDate,PurchasePrice,Notes")] PurchaseInfo purchaseInfo)
    {
        if (id != purchaseInfo.Id)
            return NotFound();

        if (ModelState.IsValid)
        {
            var duplicate = await _context.PurchaseInfos
                .AnyAsync(p => p.HomeId == purchaseInfo.HomeId && p.Id != purchaseInfo.Id);
            if (duplicate)
            {
                ModelState.AddModelError(nameof(purchaseInfo.HomeId), "Another record already uses this home.");
                await PopulateHomesDropdownAsync(purchaseInfo.HomeId, lockedHomeId: purchaseInfo.HomeId);
                return View(purchaseInfo);
            }
            try
            {
                _context.Update(purchaseInfo);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await PurchaseInfoExists(purchaseInfo.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        await PopulateHomesDropdownAsync(purchaseInfo.HomeId, lockedHomeId: purchaseInfo.HomeId);
        return View(purchaseInfo);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var purchaseInfo = await _context.PurchaseInfos
            .Include(p => p.Home)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (purchaseInfo == null)
            return NotFound();

        return View(purchaseInfo);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var purchaseInfo = await _context.PurchaseInfos.FindAsync(id);
        if (purchaseInfo != null)
        {
            _context.PurchaseInfos.Remove(purchaseInfo);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> PurchaseInfoExists(int id) =>
        await _context.PurchaseInfos.AnyAsync(e => e.Id == id);

    /// <param name="lockedHomeId">When editing, only this home is selectable to preserve the 1:1 relationship.</param>
    private async Task PopulateHomesDropdownAsync(int? selectedHomeId = null, int? lockedHomeId = null)
    {
        IQueryable<Home> homesQuery = _context.Homes
            .OrderBy(h => h.City).ThenBy(h => h.AddressLine1);

        if (lockedHomeId == null)
        {
            var used = await _context.PurchaseInfos.Select(p => p.HomeId).ToListAsync();
            homesQuery = homesQuery.Where(h => !used.Contains(h.Id));
        }
        else
        {
            homesQuery = homesQuery.Where(h => h.Id == lockedHomeId);
        }

        var homes = await homesQuery.ToListAsync();
        var items = homes.Select(h => new { h.Id, Label = $"{h.AddressLine1}, {h.City}, {h.State}" }).ToList();
        ViewBag.HomeId = new SelectList(items, "Id", "Label", selectedHomeId);
    }
}
