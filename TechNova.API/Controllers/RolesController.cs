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

        // ✅ GET: api/Roles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Role>>> GetRoles()
        {
            try
            {
                var roles = await _context.Roles
                    .Include(r => r.Usuarios)
                    .Include(r => r.Permisoxrols)
                        .ThenInclude(px => px.FkPermisoNavigation)
                    .ToListAsync();

                return Ok(roles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno en GetRoles: {ex.Message}");
            }
        }

        // ✅ GET: api/Roles/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Role>> GetRol(int id)
        {
            var rol = await _context.Roles
                .Include(r => r.Usuarios)
                .Include(r => r.Permisoxrols)
                    .ThenInclude(px => px.FkPermisoNavigation)
                .FirstOrDefaultAsync(r => r.IdRol == id);

            if (rol == null)
                return NotFound($"No se encontró el rol con id {id}.");

            return Ok(rol);
        }

        // ✅ GET: api/Roles/{id}/Permisos
        [HttpGet("{id}/Permisos")]
        public async Task<ActionResult<IEnumerable<object>>> GetPermisosPorRol(int id)
        {
            var rolExiste = await _context.Roles.AnyAsync(r => r.IdRol == id);
            if (!rolExiste)
                return NotFound($"No existe un rol con id {id}.");

            var permisos = await _context.Permisoxrols
                .Where(pr => pr.FkRol == id)
                .Include(pr => pr.FkPermisoNavigation)
                .Select(pr => new {
                    pr.FkPermisoNavigation.IdPermiso,
                    pr.FkPermisoNavigation.Nombre
                })
                .ToListAsync();

            if (!permisos.Any())
                return NotFound($"El rol con id {id} no tiene permisos asignados.");

            return Ok(permisos);
        }

        // ✅ PUT: api/Roles/5
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
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno en PutRol: {ex.Message}");
            }

            return NoContent();
        }

        // ✅ POST: api/Roles
        [HttpPost]
        public async Task<ActionResult<Role>> PostRol(Role rol)
        {
            try
            {
                _context.Roles.Add(rol);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetRol), new { id = rol.IdRol }, rol);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno en PostRol: {ex.Message}");
            }
        }

        // ✅ DELETE: api/Roles/5
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
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno en DeleteRol: {ex.Message}");
            }
        }

        private bool RolesExists(int id)
        {
            return _context.Roles.Any(e => e.IdRol == id);
        }
    }
}
