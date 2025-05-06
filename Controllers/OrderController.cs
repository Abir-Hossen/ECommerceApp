using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECommerceApp.Data;
using ECommerceApp.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerceApp.Controllers
{
    [Authorize(Roles = "Customer")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // View available products
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products.Where(p => p.Stock > 0).ToListAsync();
            return View(products);
        }

        // Place order GET
        public async Task<IActionResult> Create(int? productId)
        {
            if (productId == null) return NotFound();
            var product = await _context.Products.FindAsync(productId);
            if (product == null || product.Stock <= 0) return NotFound();
            ViewBag.Product = product;
            return View();
        }

        // Place order POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int productId, int quantity)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null || product.Stock < quantity)
            {
                ModelState.AddModelError("", "Not enough stock available.");
                ViewBag.Product = product;
                return View();
            }
            var userName = User?.Identity?.Name;
            var userId = userName != null ? _context.Users.FirstOrDefault(u => u.UserName == userName)?.Id : null;
            if (userId == null)
            {
                return Forbid();
            }
            var order = new Order
            {
                ProductId = productId,
                Quantity = quantity,
                OrderDate = DateTime.Now,
                UserId = userId
            };
            product.Stock -= quantity;
            _context.Orders.Add(order);
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            return RedirectToAction("MyOrders");
        }

        // View user's orders
        public async Task<IActionResult> MyOrders()
        {
            var userName = User?.Identity?.Name;
            var userId = userName != null ? _context.Users.FirstOrDefault(u => u.UserName == userName)?.Id : null;
            var orders = await _context.Orders.Include(o => o.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
            return View(orders);
        }
    }
}
