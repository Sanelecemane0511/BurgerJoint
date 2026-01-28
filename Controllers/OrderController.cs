using BurgerJoint.Data;
using BurgerJoint.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BurgerJoint.Controllers
{
    public class OrderController : Controller
    {
        private readonly AppDbContext _ctx;
        public OrderController(AppDbContext ctx) => _ctx = ctx;

        [HttpGet]
        public IActionResult Order()
        {
            // if we arrived via GET (cart click) – build list from query
            if (Request.Query["itemsJson"].Count > 0 &&
                JsonSerializer.Deserialize<List<OrderItem>>(Request.Query["itemsJson"]!) is { Count: > 0 } list)
                return View(list);

            return View(new List<OrderItem>());
        }

        [HttpPost]
        public async Task<IActionResult> Order(string customerEmail, string deliveryOption, string deliveryAddress, string itemsJson)
        {
            if (string.IsNullOrWhiteSpace(customerEmail) || string.IsNullOrWhiteSpace(itemsJson))
                return BadRequest("Email and items required.");

            var items = JsonSerializer.Deserialize<List<OrderItem>>(itemsJson) ?? new();
            decimal total = items.Sum(i => (i.UnitPrice * i.Qty) + (i.AddBeer ? 35 : 0));

            var order = new Order
            {
                CustomerEmail = customerEmail,
                DeliveryOption = deliveryOption,
                DeliveryAddress = deliveryAddress,
                Total = total,
                ItemsJson = itemsJson
            };

            _ctx.Orders.Add(order);
            await _ctx.SaveChangesAsync();
            return RedirectToAction("Success", new { id = order.Id });
        }
    }
}