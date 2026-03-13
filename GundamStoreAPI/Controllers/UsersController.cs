using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GundamStoreAPI.Models;


namespace GundamStoreAPI.Controllers
{
    [Route("/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly GundamStoreContext _context;

        public UsersController(GundamStoreContext context)
        {
            _context = context;
        }

        // --- READ (R) ---
        // GET: api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // GET: api/users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return user;
        }

        // --- CREATE (C) ---
        // POST: api/users
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Trả về kết quả và link đến user vừa tạo
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        // --- UPDATE (U) ---
        // PUT: api/users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User updatedData)
        {
            // 1. Kiểm tra ID có khớp không (Tránh gửi ID 5 nhưng sửa ID 10)
            if (id != updatedData.Id)
            {
                return BadRequest(new { message = "ID không khớp!" });
            }

            // 2. Tìm User trong DB
            var userInDb = await _context.Users.FindAsync(id);
            if (userInDb == null)
            {
                return NotFound(new { message = "Không tìm thấy User!" });
            }

            // 3. Cập nhật dữ liệu (Chỉ cập nhật những gì cần thiết)
            // Lưu ý: Không nên cho phép sửa Password ở đây, Password nên có API riêng
            userInDb.Username = updatedData.Username;
            userInDb.Password = updatedData.Password;
            userInDb.Email = updatedData.Email;
            userInDb.Phone = updatedData.Phone;
            userInDb.Address = updatedData.Address;
            userInDb.Avatar = updatedData.Avatar;
            userInDb.Role = updatedData.Role;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id)) return NotFound();
                else throw;
            }

            return Ok(userInDb); // Trả về User đã sửa để FE cập nhật giao diện
        }

        // --- DELETE (D) ---
        // DELETE: api/users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng để xóa!" });
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            // Trả về 204 No Content (Thành công nhưng không trả về dữ liệu)
            return NoContent();
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}