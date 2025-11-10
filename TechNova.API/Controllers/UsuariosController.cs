using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechNova.API.Data;

namespace TechNova.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly TechNovaContext _context;

        public UsuariosController(TechNovaContext context)
        {
            _context = context;
        }

        // ✅ 1️⃣ Obtener todos los usuarios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            return await _context.Usuarios
                .Include(u => u.FkRolNavigation)
                    .ThenInclude(r => r.Permisoxrols)
                        .ThenInclude(pr => pr.FkPermisoNavigation)
                .ToListAsync();
        }

        // ✅ 2️⃣ Obtener un usuario por ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.FkRolNavigation)
                    .ThenInclude(r => r.Permisoxrols)
                        .ThenInclude(pr => pr.FkPermisoNavigation)
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (usuario == null)
                return NotFound();

            return usuario;
        }

        // ✅ 3️⃣ Verificar si un correo ya existe (para el frontend)
        [HttpGet("ExisteCorreo")]
        public async Task<ActionResult<bool>> ExisteCorreo([FromQuery] string correo)
        {
            bool existe = await _context.Usuarios.AnyAsync(u => u.Email == correo);
            return Ok(existe);
        }

        // ✅ 4️⃣ Actualizar un usuario
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(int id, Usuario inputUsuario)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
                return NotFound();

            // 🔹 Validar correo duplicado (si lo cambia)
            if (usuario.Email != inputUsuario.Email)
            {
                bool correoEnUso = await _context.Usuarios.AnyAsync(u => u.Email == inputUsuario.Email && u.IdUsuario != id);
                if (correoEnUso)
                    return BadRequest("⚠️ El correo ya está registrado por otro usuario.");
            }

            // 🔹 Actualizar campos
            usuario.Nombre = inputUsuario.Nombre;
            usuario.Email = inputUsuario.Email;
            usuario.Celular = inputUsuario.Celular;
            usuario.Direccion = inputUsuario.Direccion;
            usuario.FkRol = inputUsuario.FkRol;
            usuario.TipoDoc = inputUsuario.TipoDoc;
            usuario.Documento = inputUsuario.Documento;
            usuario.Contrasena = string.IsNullOrEmpty(inputUsuario.Contrasena)
                ? usuario.Contrasena
                : inputUsuario.Contrasena;
            usuario.Estado = inputUsuario.Estado;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsuarioExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // ✅ 5️⃣ Crear un nuevo usuario
        [HttpPost]
        public async Task<ActionResult<Usuario>> PostUsuario(Usuario usuario)
        {
            // 🔹 Validar que el correo no esté repetido
            bool existeCorreo = await _context.Usuarios.AnyAsync(u => u.Email == usuario.Email);
            if (existeCorreo)
                return BadRequest("⚠️ El correo ya está registrado. Usa otro diferente.");

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "✅ Usuario creado exitosamente",
                usuario.IdUsuario,
                usuario.Nombre,
                usuario.Email,
                usuario.FkRol,
                usuario.Estado
            });
        }

        // ✅ 6️⃣ Eliminar un usuario
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
                return NotFound();

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ✅ 7️⃣ Cambiar estado de usuario (activo/inactivo)
        [HttpPatch("{id}/estado")]
        public async Task<IActionResult> PatchUsuarioEstado(int id, [FromBody] EstadoRequest request)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
                return NotFound();

            usuario.Estado = request.Estado;
            _context.Entry(usuario).Property(u => u.Estado).IsModified = true;
            await _context.SaveChangesAsync();

            return Ok(usuario);
        }

        // ✅ 8️⃣ Método auxiliar
        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.IdUsuario == id);
        }
    }

    // ✅ DTO mínimo para PATCH
    public class EstadoRequest
    {
        public bool Estado { get; set; }
    }
}
