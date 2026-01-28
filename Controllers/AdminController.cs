using BurgerJoint.Data;
using BurgerJoint.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace BurgerJoint.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AdminController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // -----------  BURGER CRUD + PHOTO UPLOAD  -----------
        public async Task<IActionResult> Index() => View(await _context.Burgers.ToListAsync());

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Burger burger, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                burger.ImageFileName = await SaveImage(imageFile);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Burger burger, IFormFile? imageFile)
        {
            if (id != burger.Id) return NotFound();
            if (ModelState.IsValid)
            {
                if (imageFile != null)
                {
                    DeleteImage(burger.ImageFileName);
                    burger.ImageFileName = await SaveImage(imageFile);
                }
                _context.Update(burger);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(burger);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var burger = await _context.Burgers.FindAsync(id);
            if (burger != null)
            {
                DeleteImage(burger.ImageFileName);
                _context.Burgers.Remove(burger);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // -----------  HERO CAROUSEL UPLOAD  -----------
        private readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private readonly long MaxSize = 2 * 1024 * 1024; // 2 MB

        public IActionResult Hero() => View(GetHeroImages());

        [HttpPost]
        public async Task<IActionResult> UploadHero(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["msg"] = "No file selected.";
                return RedirectToAction(nameof(Hero));
            }

            var ext = Path.GetExtension(file.FileName).ToLower();
            if (!AllowedExtensions.Contains(ext))
            {
                TempData["msg"] = "Only .jpg .png .webp allowed.";
                return RedirectToAction(nameof(Hero));
            }
            if (file.Length > MaxSize)
            {
                TempData["msg"] = "Max 2 MB.";
                return RedirectToAction(nameof(Hero));
            }

            var fileName = Guid.NewGuid() + ext;
            var path = Path.Combine(_env.WebRootPath, "img", "hero", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);

            TempData["msg"] = "Hero image uploaded.";
            return RedirectToAction(nameof(Hero));
        }

        [HttpPost]
        public IActionResult DeleteHero(string fileName)
        {
            var path = Path.Combine(_env.WebRootPath, "img", "hero", fileName);
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
            TempData["msg"] = "Hero deleted.";
            return RedirectToAction(nameof(Hero));
        }

        // -----------  BEER LIST (simple text)  -----------
        public IActionResult Beer() => View(GetBeerList());

        [HttpPost]
        public IActionResult Beer(List<string> beers)
        {
            System.IO.File.WriteAllLines(BeerFilePath(), beers.Where(b => !string.IsNullOrWhiteSpace(b)));
            TempData["msg"] = "Beer list saved.";
            return RedirectToAction(nameof(Beer));
        }

        // -----------  PRIVATE HELPERS  -----------
        private async Task<string> SaveImage(IFormFile? file)
        {
            if (file == null || file.Length == 0) return string.Empty;
            var ext = Path.GetExtension(file.FileName).ToLower();
            if (!AllowedExtensions.Contains(ext)) return string.Empty;
            var fileName = Guid.NewGuid() + ext;
            var path = Path.Combine(_env.WebRootPath, "img", "burgers", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);
            return fileName;
        }

        private void DeleteImage(string? fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return;
            var path = Path.Combine(_env.WebRootPath, "img", "burgers", fileName);
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
        }

        private List<string> GetHeroImages()
        {
            var dir = Path.Combine(_env.WebRootPath, "img", "hero");
            Directory.CreateDirectory(dir);
            return Directory.GetFiles(dir).Select(Path.GetFileName).ToList()!;
        }

        private List<string> GetBeerList()
        {
            var path = BeerFilePath();
            return System.IO.File.Exists(path) ? System.IO.File.ReadAllLines(path).ToList() : new List<string>();
        }

        private static string BeerFilePath() => Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "beers.txt");
    }
}