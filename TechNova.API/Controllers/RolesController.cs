using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechNova.API.Data;
using TechNova.API.Models;

namespace TechNova.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly TechNovaContext _context;

        public RolesController(TechNovaContext context)
        {
            _context = context;
        }

        // ✅ GET: api/Roles - Roles con permisos y usuarios proyectados para frontend
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetRolesParaFront()
        {
            try
            {
                var roles = await _context.Roles
                    .Include(r => r.Usuarios)
                    .Include(r => r.Permisoxrols)
                        .ThenInclude(px => px.FkPermisoNavigation)
                    .Select(r => new
                    {
                        r.IdRol,
                        r.NombreRol,
                        r.Descripcion,
                        r.Activo,
                        Permisos = r.Permisoxrols.Select(px => new
                        {
                            px.FkPermisoNavigation.IdPermiso,
                            px.FkPermisoNavigation.Nombre
                        }),
                        Usuarios = r.Usuarios.Select(u => new
                        {
                            u.IdUsuario,
                            u.Nombre,
                            u.Email,
                            u.Estado,
                            u.TipoDoc,
                            u.Documento,
                            u.Celular,
                            u.Direccion
                        })
                    })
                    .ToListAsync();

                return Ok(roles);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Error interno en GetRolesParaFront: {ex.Message}");
            }
        }

        // ✅ GET: api/Roles/5 - Detalle de un rol con permisos y usuarios
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetRol(int id)
        {
            var rol = await _context.Roles
                .Include(r => r.Usuarios)
                .Include(r => r.Permisoxrols)
                    .ThenInclude(px => px.FkPermisoNavigation)
                .Where(r => r.IdRol == id)
                .Select(r => new
                {
                    r.IdRol,
                    r.NombreRol,
                    r.Descripcion,
                    r.Activo,
                    Permisos = r.Permisoxrols.Select(px => new
                    {
                        px.FkPermisoNavigation.IdPermiso,
                        px.FkPermisoNavigation.Nombre
                    }),
                    Usuarios = r.Usuarios.Select(u => new
                    {
                        u.IdUsuario,
                        u.Nombre,
                        u.Email,
                        u.Estado,
                        u.TipoDoc,
                        u.Documento,
                        u.Celular,
                        u.Direccion
                    })
                })
                .FirstOrDefaultAsync();

            if (rol == null)
                return NotFound($"No se encontró el rol con id {id}.");

            return Ok(rol);
        }

        // ✅ GET: api/Roles/5/Permisos - Obtener solo los permisos de un rol
        [HttpGet("{id}/Permisos")]
        public async Task<ActionResult<IEnumerable<object>>> GetPermisosPorRol(int id)
        {
            var rol = await _context.Roles
                .Include(r => r.Permisoxrols)
                    .ThenInclude(px => px.FkPermisoNavigation)
                .FirstOrDefaultAsync(r => r.IdRol == id);

            if (rol == null)
                return NotFound($"No se encontró el rol con id {id}.");

            var permisos = rol.Permisoxrols.Select(px => new
            {
                px.FkPermisoNavigation.IdPermiso,
                px.FkPermisoNavigation.Nombre
            });

            return Ok(permisos);
        }

        // ✅ POST: api/Roles - Crear un nuevo rol
        [HttpPost]
        public async Task<ActionResult<Role>> PostRol(Role rol)
        {
            try
            {
                _context.Roles.Add(rol);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetRol), new { id = rol.IdRol }, rol);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Error interno en PostRol: {ex.Message}");
            }
        }

        // ✅ PUT: api/Roles/5 - Actualizar un rol
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRol(int id, Role rol)
        {
            if (id != rol.IdRol)
                return BadRequest("El id del rol no coincide con el objeto enviado.");

            try
            {
                _context.Entry(rol).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RolesExists(id))
                    return NotFound($"No existe un rol con id {id}.");
                else
                    throw;
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Error interno en PutRol: {ex.Message}");
            }

            return NoContent();
        }

        // ✅ DELETE: api/Roles/5 - Eliminar un rol solo si no tiene usuarios o permisos
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRol(int id)
        {
            try
            {
                var rol = await _context.Roles
                    .Include(r => r.Usuarios)
                    .Include(r => r.Permisoxrols)
                    .FirstOrDefaultAsync(r => r.IdRol == id);

                if (rol == null)
                    return NotFound($"No se encontró el rol con id {id}.");

                if (rol.Usuarios.Any() || rol.Permisoxrols.Any())
                    return BadRequest("No se puede eliminar el rol porque tiene usuarios o permisos asociados.");

                _context.Roles.Remove(rol);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Error interno en DeleteRol: {ex.Message}");
            }
        }

        // ✅ POST: api/Roles/{id}/AsignarPermisos - Asignar lista de permisos a un rol
        [HttpPost("{id}/AsignarPermisos")]
        public async Task<IActionResult> AsignarPermisos(int id, [FromBody] List<int> permisosIds)
        {
            var rol = await _context.Roles
                .Include(r => r.Permisoxrols)
                .FirstOrDefaultAsync(r => r.IdRol == id);

            if (rol == null)
                return NotFound($"No existe un rol con id {id}.");

            // Limpiar todos los permisos actuales
            _context.Permisoxrols.RemoveRange(rol.Permisoxrols);

            // Agregar los permisos que envía el front
            foreach (var permisoId in permisosIds)
            {
                rol.Permisoxrols.Add(new Permisoxrol { FkRol = id, FkPermiso = permisoId });
            }

            await _context.SaveChangesAsync();
            return Ok($"Permisos actualizados para el rol {id}.");
        }

        private bool RolesExists(int id)
        {
            return _context.Roles.Any(e => e.IdRol == id);
        }
    }
}
