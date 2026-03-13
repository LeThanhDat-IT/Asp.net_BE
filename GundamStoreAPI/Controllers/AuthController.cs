using GundamStoreAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GundamStoreAPI.Models;

namespace Tên_Project_Của_Bạn.Controllers
{
    [Route("/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly GundamStoreContext _context;
        private readonly IConfiguration _configuration;

        // Bơm cả Database và Cấu hình (để lấy Jwt:Key) vào
        public AuthController(GundamStoreContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // 1. Tìm user trong Database
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.Password == request.Password);

            if (user == null)
            {
                return Unauthorized(new { message = "Sai tài khoản hoặc mật khẩu!" });
            }

            // 2. Tạo các thẻ thông tin (Claims) giấu vào trong Token
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim("UserId", user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role), // admin hay user
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // 3. Lấy Key từ appsettings.json để ký
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 4. Định hình cái Token (Thời hạn 1 ngày)
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            // 5. Trả Token về cho Frontend
            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                message = "Đăng nhập thành công!",
                user = new { id = user.Id, username = user.Username, role = user.Role }
            });
        }
    }

    // Class phụ để hứng dữ liệu FE gửi lên
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}