using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebMoHinh.Data;
using WebMoHinh.Models;

namespace WebMoHinh.Controllers
{
    [Authorize(Roles = "Customer,Admin")]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Danh sách giỏ hàng
        public async Task<IActionResult> Index()
        {
            var user =
                await _userManager.GetUserAsync(User);

            var cartItems =
                await _context.CartItems
                .Include(x => x.Product)
                .Where(x => x.UserId == user.Id)
                .ToListAsync();

            return View(cartItems);
        }

        // Thêm vào giỏ hàng
        public async Task<IActionResult> AddToCart(int id)
        {
            var user =
                await _userManager.GetUserAsync(User);

            var cartItem =
                await _context.CartItems
                .FirstOrDefaultAsync(
                    x => x.ProductId == id &&
                    x.UserId == user.Id);

            if (cartItem == null)
            {
                cartItem = new CartItem
                {
                    ProductId = id,
                    UserId = user.Id,
                    Quantity = 1
                };

                _context.CartItems.Add(cartItem);
            }
            else
            {
                cartItem.Quantity++;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Tăng số lượng
        public async Task<IActionResult> Increase(int id)
        {
            var item =
                await _context.CartItems.FindAsync(id);

            if (item != null)
            {
                item.Quantity++;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // Giảm số lượng
        public async Task<IActionResult> Decrease(int id)
        {
            var item =
                await _context.CartItems.FindAsync(id);

            if (item != null)
            {
                item.Quantity--;

                if (item.Quantity <= 0)
                {
                    _context.CartItems.Remove(item);
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // Xóa khỏi giỏ
        public async Task<IActionResult> Remove(int id)
        {
            var item =
                await _context.CartItems.FindAsync(id);

            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}