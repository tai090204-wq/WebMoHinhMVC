using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebMoHinh.Data;
using WebMoHinh.Models;

namespace WebMoHinh.Controllers
{
    [Authorize]
    public class FavoriteController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;


    public FavoriteController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Add(int id)
        {
            var userId = _userManager.GetUserId(User);

            bool exists = await _context.Favorites
                .AnyAsync(x =>
                    x.UserId == userId &&
                    x.ProductId == id);

            if (!exists)
            {
                Favorite favorite = new()
                {
                    UserId = userId,
                    ProductId = id
                };

                _context.Favorites.Add(favorite);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(
                "Details",
                "Product",
                new { id });
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var favorites = await _context.Favorites
                .Include(x => x.Product)
                .Where(x => x.UserId == userId)
                .ToListAsync();

            return View(favorites);
        }

        public async Task<IActionResult> Remove(int id)
        {
            var favorite =
                await _context.Favorites.FindAsync(id);

            if (favorite != null)
            {
                _context.Favorites.Remove(favorite);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }


}
