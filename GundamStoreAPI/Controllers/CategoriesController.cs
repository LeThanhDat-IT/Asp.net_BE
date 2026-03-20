using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GundamStoreAPI.Models;

namespace GundamStoreAPI.Controllers
{
    [Route("/categories")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly GundamStoreContext _context;

        public CategoriesController(GundamStoreContext context)
        {
            _context = context;
        }

        // 1. LẤY TẤT CẢ DANH MỤC (Kèm theo danh mục con)
        // GET: api/categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            // Sử dụng .AsNoTracking() để tăng tốc độ truy vấn khi chỉ đọc dữ liệu
            return await _context.Categories
                .Include(c => c.Subcategories)
                .AsNoTracking()
                .ToListAsync();
        }

        // 2. TÌM KIẾM DANH MỤC THEO TÊN
        // GET: api/categories/search?name=HG
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Category>>> SearchCategories([FromQuery] string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return await GetCategories();
            }

            var results = await _context.Categories
                .Where(c => c.Name.Contains(name))
                .Include(c => c.Subcategories)
                .ToListAsync();

            return Ok(results);
        }

        // 3. LẤY CHI TIẾT 1 DANH MỤC (Kèm danh mục con và sản phẩm)
        // GET: api/categories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Subcategories)
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound(new { message = "Không tìm thấy danh mục này!" });
            }

            return category;
        }

        // 4. THÊM DANH MỤC MỚI (Admin)
        // POST: api/categories
        [HttpPost]
        public async Task<ActionResult<Category>> PostCategory(Category category)
        {
            try
            {
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi thêm danh mục", error = ex.Message });
            }
        }

        // 5. CẬP NHẬT DANH MỤC (Admin)
        // PUT: api/categories/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(int id, Category category)
        {
            if (id != category.Id)
            {
                return BadRequest(new { message = "ID không khớp!" });
            }

            _context.Entry(category).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(id))
                {
                    return NotFound(new { message = "Danh mục không tồn tại để cập nhật!" });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new { message = "Cập nhật thành công!", data = category });
        }

        // 6. XÓA DANH MỤC (Admin)
        // DELETE: api/categories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound(new { message = "Không tìm thấy danh mục để xóa!" });
            }

            try
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Đã xóa danh mục thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Không thể xóa vì danh mục đang chứa sản phẩm hoặc danh mục con!", error = ex.Message });
            }
        }

        // Hàm kiểm tra tồn tại
        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}