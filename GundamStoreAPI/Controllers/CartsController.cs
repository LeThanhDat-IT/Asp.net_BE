using GundamStoreAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GundamStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CartsController : ControllerBase
    {
        private readonly GundamStoreContext _context;

        public CartsController(GundamStoreContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy toàn bộ sản phẩm trong giỏ hàng của một User
        /// </summary>
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetCart(int userId)
        {
            // Dùng Include để lấy luôn thông tin bảng Products (tên, giá) thay vì chỉ lấy ID
            var cartItems = await _context.Carts
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .Select(c => new
                {
                    CartId = c.Id,
                    ProductId = c.ProductId,
                    ProductName = c.Product.Name,
                    Price = c.Product.Price,
                    Quantity = c.Quantity,
                    TotalPrice = c.Quantity * c.Product.Price // Tự tính tổng tiền món đó
                })
                .ToListAsync();

            return Ok(cartItems);
        }

        /// <summary>
        /// Thêm sản phẩm vào giỏ hàng
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] CartRequest request)
        {
            // BÍ KÍP BẢO MẬT: Móc UserId từ trong chính cái Token ra, không tin FE gửi lên
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized("Token không hợp lệ!");
            int currentUserId = int.Parse(userIdClaim);

            var product = await _context.Products.FindAsync(request.ProductId);
            if (product == null) return NotFound(new { message = "Sản phẩm không tồn tại!" });

            var existingCartItem = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == currentUserId && c.ProductId == request.ProductId);

            if (existingCartItem != null)
            {
                existingCartItem.Quantity += request.Quantity;
            }
            else
            {
                var newCartItem = new Cart
                {
                    UserId = currentUserId, // Dùng ID lấy từ Token
                    ProductId = request.ProductId,
                    Quantity = request.Quantity
                };
                _context.Carts.Add(newCartItem);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã cập nhật giỏ hàng thành công!" });
        }
    }

    // Class phụ trợ để nhận dữ liệu JSON từ Frontend gửi lên
    public class CartRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1; // Mặc định là 1 nếu FE không gửi số lượng
    }
}