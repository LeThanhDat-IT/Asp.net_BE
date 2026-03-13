using Microsoft.EntityFrameworkCore;
using GundamStoreAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Lấy chuỗi kết nối từ appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Đăng ký GundamStoreContext
builder.Services.AddDbContext<GundamStoreContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 21))));

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowTeamFE", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
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
        ValidateLifetime = true
    };
});

var app = builder.Build();

// CONFIGURE SWAGGER AT ROOT
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "GundamStoreAPI v1");
    // Sửa dòng này để Swagger chạy ngay tại địa chỉ gốc (/)
    options.RoutePrefix = string.Empty;
});

app.UseCors("AllowTeamFE");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();