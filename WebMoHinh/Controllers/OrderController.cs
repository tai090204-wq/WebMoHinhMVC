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

        // ==========================
        // DASHBOARD
        // ==========================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Dashboard()
        {
            DashboardViewModel model = new();

            model.TotalCustomers =
                await _userManager.Users.CountAsync();

            model.TotalOrders =
                await _context.Orders.CountAsync();

            model.TotalRevenue =
                await _context.Orders
                    .SumAsync(x => (decimal?)x.TotalAmount) ?? 0;

            var chartData = await _context.Orders
                .GroupBy(x => x.OrderDate.Date)
                .Select(x => new
                {
                    Date = x.Key,
                    Orders = x.Count(),
                    Revenue = x.Sum(y => y.TotalAmount)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            model.Labels = chartData
                .Select(x => x.Date.ToString("dd/MM"))
                .ToList();

            model.OrderCounts = chartData
                .Select(x => x.Orders)
                .ToList();

            model.Revenues = chartData
                .Select(x => x.Revenue)
                .ToList();

            return View(model);
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Report(
    DateTime? fromDate,
    DateTime? toDate)
        {
            var startDate = fromDate ??
                new DateTime(
                    DateTime.Now.Year,
                    DateTime.Now.Month,
                    1);

            var endDate = toDate ??
                DateTime.Now;

            ReportViewModel model = new()
            {
                FromDate = startDate,
                ToDate = endDate
            };

            var orders = _context.Orders
                .Where(x =>
                    x.OrderDate >= startDate &&
                    x.OrderDate <= endDate);

            model.TotalRevenue =
                await orders.SumAsync(
                    x => (decimal?)x.TotalAmount) ?? 0;

            model.TotalOrders =
                await orders.CountAsync();

            model.TotalCustomers =
                await orders
                    .Select(x => x.UserId)
                    .Distinct()
                    .CountAsync();

            model.TopBestProducts =
                await _context.OrderDetails
                .Include(x => x.Order)
                .Include(x => x.Product)
                .Where(x =>
                    x.Order.OrderDate >= startDate &&
                    x.Order.OrderDate <= endDate)
                .GroupBy(x => x.Product.Name)
                .Select(x => new ProductStatisticVM
                {
                    ProductName = x.Key,
                    QuantitySold = x.Sum(y => y.Quantity)
                })
                .OrderByDescending(x => x.QuantitySold)
                .Take(5)
                .ToListAsync();

            model.TopWorstProducts =
                await _context.OrderDetails
                .Include(x => x.Order)
                .Include(x => x.Product)
                .Where(x =>
                    x.Order.OrderDate >= startDate &&
                    x.Order.OrderDate <= endDate)
                .GroupBy(x => x.Product.Name)
                .Select(x => new ProductStatisticVM
                {
                    ProductName = x.Key,
                    QuantitySold = x.Sum(y => y.Quantity)
                })
                .OrderBy(x => x.QuantitySold)
                .Take(5)
                .ToListAsync();

            model.TopBestCategories =
                await _context.OrderDetails
                .Include(x => x.Order)
                .Include(x => x.Product)
                .ThenInclude(x => x.Category)
                .Where(x =>
                    x.Order.OrderDate >= startDate &&
                    x.Order.OrderDate <= endDate)
                .GroupBy(x => x.Product.Category.Name)
                .Select(x => new CategoryStatisticVM
                {
                    CategoryName = x.Key,
                    QuantitySold = x.Sum(y => y.Quantity)
                })
                .OrderByDescending(x => x.QuantitySold)
                .Take(5)
                .ToListAsync();

            model.TopWorstCategories =
                await _context.OrderDetails
                .Include(x => x.Order)
                .Include(x => x.Product)
                .ThenInclude(x => x.Category)
                .Where(x =>
                    x.Order.OrderDate >= startDate &&
                    x.Order.OrderDate <= endDate)
                .GroupBy(x => x.Product.Category.Name)
                .Select(x => new CategoryStatisticVM
                {
                    CategoryName = x.Key,
                    QuantitySold = x.Sum(y => y.Quantity)
                })
                .OrderBy(x => x.QuantitySold)
                .Take(5)
                .ToListAsync();

            return View(model);
        }

        // ==========================
        // CUSTOMER: ĐƠN HÀNG CỦA TÔI
        // ==========================
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

        // ==========================
        // ADMIN: DANH SÁCH ĐƠN HÀNG
        // ==========================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .OrderByDescending(x => x.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // ==========================
        // CHI TIẾT ĐƠN HÀNG
        // ==========================
        [Authorize(Roles = "Customer,Admin")]
        public async Task<IActionResult> Details(int id)
        {
            var details = await _context.OrderDetails
                .Include(x => x.Product)
                .Where(x => x.OrderId == id)
                .ToListAsync();

            return View(details);
        }

        // ==========================
        // THANH TOÁN
        // ==========================
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

            decimal total = cartItems.Sum(x =>
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

        // ==========================
        // ĐẶT HÀNG THÀNH CÔNG
        // ==========================
        public async Task<IActionResult> OrderSuccess(int id)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(x => x.Id == id);

            return View(order);
        }

        // ==========================
        // ADMIN: ĐỔI TRẠNG THÁI
        // ==========================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeStatus(
            int id,
            string status)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
                return NotFound();

            order.Status = status;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ==========================
        // ADMIN: XÓA ĐƠN HÀNG
        // ==========================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
                return NotFound();

            _context.Orders.Remove(order);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}