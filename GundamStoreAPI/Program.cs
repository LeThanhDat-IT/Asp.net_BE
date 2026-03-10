using Microsoft.EntityFrameworkCore;
using GundamStoreAPI.Models; // Thay bằng tên project thật của bạn, ví dụ GundamStoreAPI.Models
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
// Lấy chuỗi kết nối từ appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Đăng ký GundamStoreContext
builder.Services.AddDbContext<GundamStoreContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowTeamFE", policy =>
    {
        policy.AllowAnyOrigin()   // Cho phép mọi đường link FE (React, Vue, Flutter...) gọi vào
              .AllowAnyMethod()   // Cho phép mọi lệnh (GET, POST, PUT, DELETE...)
              .AllowAnyHeader();  // Cho phép mọi loại dữ liệu gửi lên (JSON, Form-data...)
    });
});

// ĐỌC THÔNG TIN TỪ APPSETTINGS
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);

// ĐĂNG KÝ XÁC THỰC JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Đang chạy localhost nên để false
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true // Kiểm tra token hết hạn chưa
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
// Luôn bật Swagger bất kể môi trường nào để bạn có thể test trên Azure
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowTeamFE");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
