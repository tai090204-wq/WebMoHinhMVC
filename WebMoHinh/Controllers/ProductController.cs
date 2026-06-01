using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebMoHinh.Data;
using WebMoHinh.Models;

namespace WebMoHinh.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ProductController(
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // LIST
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            return View(
                await _context.Products
                .Include(x => x.Category)
                .ToListAsync());
        }

        // DETAIL
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(x => x.Category)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        // CREATE
        [Authorize(Roles = "Admin,Employee")]
        public IActionResult Create()
        {
            ViewBag.CategoryId =
                new SelectList(
                    _context.Categories,
                    "Id",
                    "Name");

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Create(Product product)
        {
            if (ModelState.IsValid)
            {
                if (product.ImageFile != null)
                {
                    string fileName =
                        Guid.NewGuid().ToString()
                        + Path.GetExtension(
                            product.ImageFile.FileName);

                    string uploadPath =
                        Path.Combine(
                            _environment.WebRootPath,
                            "images");

                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    string filePath =
                        Path.Combine(uploadPath, fileName);

                    using (var stream =
                        new FileStream(
                            filePath,
                            FileMode.Create))
                    {
                        await product.ImageFile
                            .CopyToAsync(stream);
                    }

                    product.ImageUrl =
                        "/images/" + fileName;
                }

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewBag.CategoryId =
                new SelectList(
                    _context.Categories,
                    "Id",
                    "Name");

            return View(product);
        }

        // EDIT
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Edit(int id)
        {
            var product =
                await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound();

            ViewBag.CategoryId =
                new SelectList(
                    _context.Categories,
                    "Id",
                    "Name",
                    product.CategoryId);

            return View(product);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            var oldProduct =
                await _context.Products.FindAsync(id);

            if (oldProduct == null)
                return NotFound();

            oldProduct.Name = product.Name;
            oldProduct.Description = product.Description;
            oldProduct.Price = product.Price;
            oldProduct.Quantity = product.Quantity;
            oldProduct.CategoryId = product.CategoryId;

            if (product.ImageFile != null)
            {
                string fileName =
                    Guid.NewGuid().ToString()
                    + Path.GetExtension(
                        product.ImageFile.FileName);

                string filePath =
                    Path.Combine(
                        _environment.WebRootPath,
                        "images",
                        fileName);

                using (var stream =
                    new FileStream(
                        filePath,
                        FileMode.Create))
                {
                    await product.ImageFile
                        .CopyToAsync(stream);
                }

                oldProduct.ImageUrl =
                    "/images/" + fileName;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // DELETE
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var product =
                await _context.Products
                .Include(x => x.Category)
                .FirstOrDefaultAsync(x => x.Id == id);

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product =
                await _context.Products.FindAsync(id);

            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}