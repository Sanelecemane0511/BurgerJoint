using BurgerJoint.Data;
using BurgerJoint.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace BurgerJoint.Controllers
{
    public class BurgerController : Controller
    {
        private readonly AppDbContext _context;
        public BurgerController(AppDbContext context) => _context = context;

        // SINGLE Index – search + sort + pagination ready
        public async Task<IActionResult> Index(string search, string sort, int pg = 1)
        {
            const int pageSize = 12;                       // 12 burgers per page
            var query = _context.Burgers.AsNoTracking();

            // search
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(b => b.Name.Contains(search) ||
                                         b.Ingredients.Contains(search));

            // sort
            query = sort switch
            {
                "name_desc" => query.OrderByDescending(b => b.Name),
                "price_asc" => query.OrderBy(b => b.Price),
                "price_desc" => query.OrderByDescending(b => b.Price),
                _ => query.OrderBy(b => b.Name),
            };

            // pagination
            int count = query.Count();
            var data = await query.Skip((pg - 1) * pageSize)
                                  .Take(pageSize)
                                  .ToListAsync();

            ViewBag.Pager = new
            {
                Search = search,
                Sort = sort,
                PageSize = pageSize,
                Total = count,
                CurrentPage = pg,
                TotalPages = (int)Math.Ceiling(count / (double)pageSize)
            };

            return View(data);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var burger = await _context.Burgers.FirstOrDefaultAsync(m => m.Id == id);
            return burger == null ? NotFound() : View(burger);
        }

        [Authorize]
        public IActionResult Create() => View();

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Price,Ingredients,ImageFileName")] Burger burger)
        {
            if (ModelState.IsValid)
            {
                _context.Add(burger);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(burger);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var burger = await _context.Burgers.FindAsync(id);
            return burger == null ? NotFound() : View(burger);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,Ingredients,ImageFileName")] Burger burger)
        {
            if (id != burger.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(burger);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Burgers.Any(e => e.Id == burger.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(burger);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var burger = await _context.Burgers.FirstOrDefaultAsync(m => m.Id == id);
            return burger == null ? NotFound() : View(burger);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var burger = await _context.Burgers.FindAsync(id);
            if (burger != null) { _context.Burgers.Remove(burger); await _context.SaveChangesAsync(); }
            return RedirectToAction(nameof(Index));
        }
    }
}