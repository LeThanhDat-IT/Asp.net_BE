using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GundamStoreAPI.Models;

namespace GundamStoreAPI.Controllers
{
    [Route("api/users")] // Đường dẫn sẽ là api/users
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly GundamStoreContext _context;

        public UsersController(GundamStoreContext context)
        {
            _context = context;
        }

        // 1. GET: api/users -> Lấy tất cả users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            // Trả về danh sách tất cả người dùng từ database Azure
            return await _context.Users.ToListAsync();
        }

        // 2. GET: api/users/1 -> Lấy user có id=1
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng này!" });
            }

            return user;
        }
    }
}