using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebMoHinh.Data;
using WebMoHinh.Models;

namespace WebMoHinh.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // CUSTOMER: Lịch sử đơn hàng
        [Authorize(Roles = "Customer,Admin")]
        public async Task<IActionResult> MyOrders()
        {
            var user = await _userManager.GetUserAsync(User);

            var orders = await _context.Orders
                .Where(x => x.UserId == user.Id)
                .OrderByDescending(x => x.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // ADMIN: Danh sách tất cả đơn hàng
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .OrderByDescending(x => x.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // Chi tiết đơn hàng
        [Authorize(Roles = "Customer,Admin")]
        public async Task<IActionResult> Details(int id)
        {
            var details = await _context.OrderDetails
                .Include(x => x.Product)
                .Where(x => x.OrderId == id)
                .ToListAsync();

            return View(details);
        }

        // Checkout
        [Authorize(Roles = "Customer,Admin")]
        public async Task<IActionResult> Checkout()
        {
            var user = await _userManager.GetUserAsync(User);

            var cartItems = await _context.CartItems
                .Include(x => x.Product)
                .Where(x => x.UserId == user.Id)
                .ToListAsync();

            if (!cartItems.Any())
                return RedirectToAction("Index", "Cart");

            decimal total =
                cartItems.Sum(x =>
                    x.Product.Price * x.Quantity);

            Order order = new()
            {
                UserId = user.Id,
                OrderDate = DateTime.Now,
                TotalAmount = total,
                Status = "Pending"
            };

            _context.Orders.Add(order);

            await _context.SaveChangesAsync();

            foreach (var item in cartItems)
            {
                OrderDetail detail = new()
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Product.Price
                };

                _context.OrderDetails.Add(detail);

                item.Product.Quantity -= item.Quantity;
            }

            _context.CartItems.RemoveRange(cartItems);

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(OrderSuccess),
                new { id = order.Id });
        }

        // Trang thành công
        public async Task<IActionResult> OrderSuccess(int id)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(x => x.Id == id);

            return View(order);
        }

        // ADMIN đổi trạng thái đơn
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeStatus(
            int id,
            string status)
        {
            var order =
                await _context.Orders.FindAsync(id);

            if (order == null)
                return NotFound();

            order.Status = status;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ADMIN xóa đơn hàng
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var order =
                await _context.Orders.FindAsync(id);

            if (order == null)
                return NotFound();

            _context.Orders.Remove(order);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Dashboard doanh thu
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Dashboard()
        {
            ViewBag.TotalRevenue =
                await _context.Orders
                .SumAsync(x => x.TotalAmount);

            ViewBag.TotalOrders =
                await _context.Orders.CountAsync();

            ViewBag.TotalProducts =
                await _context.Products.CountAsync();

            ViewBag.TotalUsers =
                await _userManager.Users.CountAsync();

            return View();
        }
    }
}