using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TechNova.API.Data;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// 🧩 Conexión a PostgreSQL
builder.Services.AddDbContext<TechNovaContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 🔐 JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "fallback-key-minimo-32-caracteres-aqui-1234567890"))
        };
    });

builder.Services.AddAuthorization();

// 🚦 CORS (permite acceso desde tu frontend React)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// ✅ Controladores y configuración JSON
builder.Services.AddControllers()
    .AddJsonOptions(x =>
    {
        x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        x.JsonSerializerOptions.WriteIndented = true;
    });

// ✅ Swagger para pruebas
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ⚙️ Inyección de configuración
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

var app = builder.Build();

// ✅ Swagger visible en desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TechNova API v1");
        c.RoutePrefix = string.Empty; // 👈 Abre Swagger en raíz "/"
    });
}

// ✅ Middleware principal (IMPORTANTE: UseAuthentication antes de UseAuthorization)
app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthentication(); // 🔐 ESTA LÍNEA ES NUEVA
app.UseAuthorization();
app.MapControllers();
app.Run();