using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GundamStoreAPI.Models;

namespace GundamStoreAPI.Controllers
{
    [Route("/products")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        // Khai báo biến chứa Database
        private readonly GundamStoreContext _context;

        // Bơm Database vào Controller (gọi là Dependency Injection)
        public ProductsController(GundamStoreContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy danh sách tất cả mô hình Gundam hiện có
        /// </summary>
        /// <returns>Danh sách sản phẩm</returns>
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            // Dùng EF Core để lấy toàn bộ dữ liệu từ bảng Products
            var products = await _context.Products.ToListAsync();

            // Trả về dữ liệu dưới dạng JSON với mã thành công 200 (Ok)
            return Ok(products);
        }
    }
}