using GundamStoreAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GundamStoreAPI.Controllers // Đã sửa namespace cho khớp với dự án của bạn
{
    [Route("/auth/login")] // Nên để api/auth cho đồng nhất với các controller khác
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly GundamStoreContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(GundamStoreContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // 1. Kiểm tra đầu vào
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { message = "Email và mật khẩu không được để trống!" });
            }

            // 2. Tìm user trong Database bằng EMAIL
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.Password == request.Password);

            if (user == null)
            {
                return Unauthorized(new { message = "Sai email hoặc mật khẩu!" });
            }

            try
            {
                // 3. Lấy Key bảo mật (Fix lỗi 500 nếu thiếu Key trong appsettings)
                var jwtKey = _configuration["Jwt:Key"];
                if (string.IsNullOrEmpty(jwtKey))
                {
                    return StatusCode(500, new { message = "Chưa cấu hình Jwt:Key trong server!" });
                }

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                // 4. Tạo các thẻ thông tin (Claims)
                // Dùng ?? "" để tránh lỗi 500 nếu trường Role hoặc Username trong DB bị null
                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email ?? ""),
                    new Claim("UserId", user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role ?? "user"),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                // 5. Định hình Token
                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: creds
                );

                // 6. Trả Token về cho Frontend
                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    message = "Đăng nhập thành công!",
                    user = new
                    {
                        id = user.Id,
                        email = user.Email,
                        username = user.Username,
                        role = user.Role
                    }
                });
            }
            catch (Exception ex)
            {
                // Trả về lỗi chi tiết nếu có crash trong quá trình tạo Token
                return StatusCode(500, new { message = "Lỗi hệ thống khi tạo Token", error = ex.Message });
            }
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } // Đã đổi từ Username sang Email
        public string Password { get; set; }
    }
}