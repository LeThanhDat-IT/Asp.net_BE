using GundamStoreAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GundamStoreAPI.Controllers
{
    [Route("/carts")]
    [ApiController]
    [Authorize]
    public class CartsController : ControllerBase
    {
        private readonly GundamStoreContext _context;

        public CartsController(GundamStoreContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            return string.IsNullOrEmpty(userIdClaim) ? 0 : int.Parse(userIdClaim);
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var cartItems = await _context.Carts
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .Select(c => new
                {
                    CartId = c.Id,
                    ProductId = c.ProductId,
                    ProductName = c.Product.Name,
                    // Đã bỏ dòng ProductImage bị lỗi ở đây
                    Price = c.Product.Price,
                    Quantity = c.Quantity,
                    TotalPrice = c.Quantity * c.Product.Price
                })
                .ToListAsync();

            return Ok(cartItems);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] CartRequest request)
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var product = await _context.Products.FindAsync(request.ProductId);
            if (product == null) return NotFound(new { message = "Sản phẩm không tồn tại!" });

            var existingCartItem = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == request.ProductId);

            if (existingCartItem != null)
            {
                existingCartItem.Quantity += request.Quantity;
            }
            else
            {
                var newCartItem = new Cart
                {
                    UserId = userId,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity
                };
                _context.Carts.Add(newCartItem);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã thêm vào giỏ hàng!" });
        }

        [HttpPut("update-quantity")]
        public async Task<IActionResult> UpdateQuantity([FromBody] CartRequest request)
        {
            int userId = GetCurrentUserId();
            var cartItem = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == request.ProductId);

            if (cartItem == null) return NotFound();

            if (request.Quantity <= 0)
            {
                _context.Carts.Remove(cartItem);
            }
            else
            {
                cartItem.Quantity = request.Quantity;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã cập nhật số lượng!" });
        }

        [HttpDelete("{productId}")]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            int userId = GetCurrentUserId();
            var cartItem = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);

            if (cartItem == null) return NotFound();

            _context.Carts.Remove(cartItem);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã xóa sản phẩm!" });
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            int userId = GetCurrentUserId();
            var userItems = _context.Carts.Where(c => c.UserId == userId);
            _context.Carts.RemoveRange(userItems);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Giỏ hàng đã trống!" });
        }
    }

    public class CartRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;
    }
}