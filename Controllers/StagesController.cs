using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PerryHomesTracker.Data;
using PerryHomesTracker.Models;

namespace PerryHomesTracker.Controllers;

public class StagesController : Controller
{
    private readonly IPerryHomesDbContext _context;

    public StagesController(IPerryHomesDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.Stages.OrderBy(s => s.SortOrder).ThenBy(s => s.Name).ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var stage = await _context.Stages.FirstOrDefaultAsync(m => m.Id == id);
        if (stage == null)
            return NotFound();

        return View(stage);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Name,Description,SortOrder")] Stage stage)
    {
        if (ModelState.IsValid)
        {
            _context.Add(stage);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(stage);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var stage = await _context.Stages.FindAsync(id);
        if (stage == null)
            return NotFound();

        return View(stage);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,SortOrder")] Stage stage)
    {
        if (id != stage.Id)
            return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(stage);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await StageExists(stage.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(stage);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var stage = await _context.Stages.FirstOrDefaultAsync(m => m.Id == id);
        if (stage == null)
            return NotFound();

        return View(stage);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var stage = await _context.Stages.Include(s => s.Homes).FirstOrDefaultAsync(s => s.Id == id);
        if (stage != null)
        {
            if (stage.Homes.Count > 0)
            {
                TempData["Error"] = "Cannot delete a stage that is assigned to one or more homes.";
                return RedirectToAction(nameof(Delete), new { id });
            }
            _context.Stages.Remove(stage);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> StageExists(int id) =>
        await _context.Stages.AnyAsync(e => e.Id == id);
}
