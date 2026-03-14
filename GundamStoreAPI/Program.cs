using Microsoft.EntityFrameworkCore;
using GundamStoreAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// 1. Lấy chuỗi kết nối từ appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Đăng ký GundamStoreContext (Sửa để tự động nhận diện phiên bản MySQL)
builder.Services.AddDbContext<GundamStoreContext>(options =>
{
    if (!string.IsNullOrEmpty(connectionString))
    {
        // Khai báo cứng phiên bản MySQL 8.0.21 (hoặc phiên bản Clever Cloud đang dùng)
        options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 21)));
    }
});

// 3. Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 4. Cấu hình CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowTeamFE", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 5. Cấu hình JWT
var jwtSettings = builder.Configuration.GetSection("Jwt");
var keyString = jwtSettings["Key"] ?? "MotChuoiMacDinhSieuDaiDeKhongBiLoiHeThong2026!!!";
var key = Encoding.ASCII.GetBytes(keyString);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero // Loại bỏ thời gian trễ mặc định của token
    };
});

var app = builder.Build();

// 6. Cấu hình hỗ trợ Proxy (Azure/Render thường dùng Reverse Proxy)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// 7. Cấu hình Swagger
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "GundamStoreAPI v1");
    options.RoutePrefix = string.Empty; // Swagger tại root
});

// 8. Middleware Pipeline
app.UseCors("AllowTeamFE");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();