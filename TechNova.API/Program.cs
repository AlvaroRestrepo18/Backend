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
                Encoding.UTF8.GetBytes(
                    builder.Configuration["Jwt:Key"]
                    ?? "fallback-key-minimo-32-caracteres-aqui-1234567890"
                )
            )
        };
    });

builder.Services.AddAuthorization();

// 🚦 CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Controladores + JSON
builder.Services.AddControllers()
    .AddJsonOptions(x =>
    {
        x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        x.JsonSerializerOptions.WriteIndented = true;
    });

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Inyección de configuración
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

var app = builder.Build();

// Swagger en desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TechNova API v1");
        c.RoutePrefix = string.Empty;
    });
}

// ============================
// 🔥 ORDEN CORRECTO DEL PIPELINE
// ============================

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthentication();

// 🔥 EL MIDDLEWARE DE PERMISOS DEBE IR ANTES DE UseAuthorization
app.UseMiddleware<TechNova.API.Middleware.PermisoMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
