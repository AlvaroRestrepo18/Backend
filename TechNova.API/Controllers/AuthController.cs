using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TechNova.API.Data;
using TechNova.API.Models;
using System.Net.Mail;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly TechNovaContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(TechNovaContext context, IConfiguration configuration, ILogger<AuthController> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    // 🔐 LOGIN
    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            _logger.LogInformation($"🔐 Intento de login: {request.Email}");

            var usuario = await _context.Usuarios
                .Include(u => u.FkRolNavigation)
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (usuario == null)
            {
                _logger.LogWarning($"❌ Usuario no encontrado: {request.Email}");
                return Unauthorized(new { success = false, message = "Credenciales inválidas" });
            }

            if (!usuario.Estado)
            {
                _logger.LogWarning($"❌ Usuario inactivo: {request.Email}");
                return Unauthorized(new { success = false, message = "Usuario inactivo" });
            }

            if (usuario.Contrasena != request.Password)
            {
                _logger.LogWarning($"❌ Contraseña incorrecta para: {request.Email}");
                return Unauthorized(new { success = false, message = "Credenciales inválidas" });
            }

            var token = GenerateJwtToken(usuario);

            _logger.LogInformation($"✅ Login exitoso: {usuario.Nombre}");

            return Ok(new
            {
                success = true,
                message = "Login exitoso",
                token = token,
                user = new
                {
                    id = usuario.IdUsuario,
                    nombre = usuario.Nombre,
                    email = usuario.Email,
                    rol = usuario.FkRolNavigation?.NombreRol ?? "Sin rol"
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error en login");
            return StatusCode(500, new { success = false, message = "Error interno del servidor" });
        }
    }

    // 🔄 RECUPERAR CONTRASEÑA
    [HttpPost("recuperar-contrasena")]
    public async Task<ActionResult> RecuperarContrasena([FromBody] RecuperarContrasenaRequest request)
    {
        try
        {
            _logger.LogInformation($"📧 Recuperación de contraseña: {request.Email}");

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (usuario == null)
            {
                _logger.LogInformation($"📧 Email no encontrado (pero no lo revelamos): {request.Email}");
                return Ok(new { success = true, message = "Si el email existe, se enviarán instrucciones" });
            }

            // Generar código de recuperación de 6 dígitos seguro
            var random = new Random();
            var codigo = random.Next(100000, 999999).ToString();
            usuario.CodigoRecuperacion = codigo.Substring(0, Math.Min(6, codigo.Length)); // asegura máximo 6 caracteres
            usuario.CodigoExpira = DateTime.UtcNow.AddHours(24); // usar UTC

            await _context.SaveChangesAsync();

            // Enviar correo real
            await EnviarEmailRecuperacion(usuario.Email, usuario.CodigoRecuperacion);

            return Ok(new { success = true, message = "Si el email existe, se enviarán instrucciones" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error en recuperación de contraseña");
            return StatusCode(500, new { success = false, message = "Error interno del servidor" });
        }
    }

    // 🔑 RESETEAR CONTRASEÑA
    [HttpPost("resetear-contrasena")]
    public async Task<ActionResult> ResetearContrasena([FromBody] ResetearContrasenaRequest request)
    {
        try
        {
            _logger.LogInformation($"🔄 Reseteo de contraseña para código: {request.Codigo}");

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.CodigoRecuperacion == request.Codigo);

            if (usuario == null)
            {
                _logger.LogWarning($"❌ Código inválido: {request.Codigo}");
                return BadRequest(new { success = false, message = "Código de recuperación inválido" });
            }

            if (usuario.CodigoExpira == null || usuario.CodigoExpira < DateTime.UtcNow)
            {
                _logger.LogWarning($"❌ Código expirado: {request.Codigo}");
                return BadRequest(new { success = false, message = "Código de recuperación expirado" });
            }

            usuario.Contrasena = request.NuevaContrasena;
            usuario.CodigoRecuperacion = null;
            usuario.CodigoExpira = null;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"✅ Contraseña actualizada para: {usuario.Email}");

            return Ok(new { success = true, message = "Contraseña actualizada exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al resetear contraseña");
            return StatusCode(500, new { success = false, message = "Error interno del servidor" });
        }
    }

    // 🔧 GENERAR TOKEN JWT
    private string GenerateJwtToken(Usuario usuario)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? "fallback-key-minimo-32-caracteres-123456";
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nombre),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.FkRolNavigation?.NombreRol ?? "Usuario")
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "TechNova",
            audience: _configuration["Jwt:Audience"] ?? "TechNovaUsers",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // 📧 MÉTODO PARA ENVIAR EMAIL REAL
    private async Task EnviarEmailRecuperacion(string emailDestino, string codigo)
    {
        try
        {
            var smtpHost = _configuration["SMTP:Host"];
            var smtpPort = int.Parse(_configuration["SMTP:Port"] ?? "587");
            var smtpUser = _configuration["SMTP:User"];
            var smtpPass = _configuration["SMTP:Pass"];

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new System.Net.NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            var mail = new MailMessage
            {
                From = new MailAddress(smtpUser, "TechNova"),
                Subject = "Recuperación de contraseña",
                Body = $"Tu código de recuperación es: {codigo}\nEste código expira en 24 horas.",
                IsBodyHtml = false
            };

            mail.To.Add(emailDestino);

            await client.SendMailAsync(mail);

            _logger.LogInformation($"✅ Email real enviado a {emailDestino}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"❌ Error enviando email a {emailDestino}");
            // No tiramos error al usuario, solo log
        }
    }
}

// 📦 MODELOS DE REQUEST
public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public class RecuperarContrasenaRequest
{
    public string Email { get; set; }
}

public class ResetearContrasenaRequest
{
    public string Codigo { get; set; }
    public string NuevaContrasena { get; set; }
}
