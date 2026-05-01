using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PerryHomesTracker.Data;
using PerryHomesTracker.Models;

namespace PerryHomesTracker.Controllers;

public class HomesController : Controller
{
    private readonly PerryHomesDbContext _context;

    public HomesController(PerryHomesDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var homes = _context.Homes
            .Include(h => h.Stage)
            .Include(h => h.PrimaryContact)
            .OrderBy(h => h.City).ThenBy(h => h.AddressLine1);
        return View(await homes.ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var home = await _context.Homes
            .Include(h => h.Stage)
            .Include(h => h.PrimaryContact)
            .Include(h => h.PurchaseInfo)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (home == null)
            return NotFound();

        return View(home);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateLookupsAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Id,AddressLine1,AddressLine2,City,State,Zip,CommunityName,PlanName,StageId,PrimaryContactId")]
        Home home)
    {
        if (ModelState.IsValid)
        {
            _context.Add(home);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        await PopulateLookupsAsync(home.StageId, home.PrimaryContactId);
        return View(home);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var home = await _context.Homes.FindAsync(id);
        if (home == null)
            return NotFound();

        await PopulateLookupsAsync(home.StageId, home.PrimaryContactId);
        return View(home);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,AddressLine1,AddressLine2,City,State,Zip,CommunityName,PlanName,StageId,PrimaryContactId")]
        Home home)
    {
        if (id != home.Id)
            return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(home);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await HomeExists(home.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        await PopulateLookupsAsync(home.StageId, home.PrimaryContactId);
        return View(home);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var home = await _context.Homes
            .Include(h => h.Stage)
            .Include(h => h.PrimaryContact)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (home == null)
            return NotFound();

        return View(home);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var home = await _context.Homes.FindAsync(id);
        if (home != null)
        {
            _context.Homes.Remove(home);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> HomeExists(int id) =>
        await _context.Homes.AnyAsync(e => e.Id == id);

    private async Task PopulateLookupsAsync(int? selectedStageId = null, int? selectedPersonId = null)
    {
        var stages = await _context.Stages.OrderBy(s => s.SortOrder).ThenBy(s => s.Name).ToListAsync();
        ViewBag.StageId = new SelectList(stages, "Id", "Name", selectedStageId);

        var people = await _context.People.OrderBy(p => p.LastName).ThenBy(p => p.FirstName).ToListAsync();
        var peopleList = people.Select(p => new { p.Id, Name = $"{p.LastName}, {p.FirstName}" }).ToList();
        ViewBag.PrimaryContactId = new SelectList(peopleList, "Id", "Name", selectedPersonId);
    }
}
