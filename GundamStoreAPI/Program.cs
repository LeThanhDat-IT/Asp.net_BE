using Microsoft.EntityFrameworkCore;
using GundamStoreAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// 1. Lấy chuỗi kết nối từ appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. ĐĂNG KÝ DATABASE (ĐÂY LÀ ĐOẠN BẠN BỊ THIẾU KHIẾN CODE BÁO LỖI)
builder.Services.AddDbContext<GundamStoreContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 21))));

// 3. Đăng ký Controllers và Fix lỗi vòng lặp JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// 4. Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 5. Cấu hình CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowTeamFE", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 6. Cấu hình JWT
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

// 7. Cấu hình hỗ trợ Proxy (Azure/Render thường dùng Reverse Proxy)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// 8. Cấu hình Swagger
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "GundamStoreAPI v1");
    options.RoutePrefix = string.Empty; // Swagger tại root
});

// 9. Middleware Pipeline
app.UseCors("AllowTeamFE");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();