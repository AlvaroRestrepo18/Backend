using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TechNova.API.Data;
using TechNova.API.Models;

namespace TechNova.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermisoxrolsController : ControllerBase
    {
        private readonly TechNovaContext _context;
        private readonly ILogger<PermisoxrolsController> _logger;

        public PermisoxrolsController(TechNovaContext context, ILogger<PermisoxrolsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ✅ 1. TRAER TODOS LOS PERMISOS DE TODOS LOS ROLES
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Permisoxrol>>> GetPermisoxrols()
        {
            _logger.LogInformation("Obteniendo todos los permisos por rol...");

            return await _context.Permisoxrols
                .Include(p => p.FkPermisoNavigation)
                .Include(p => p.FkRolNavigation)
                .ToListAsync();
        }

        // ✅ 2. TRAER PERMISO POR ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Permisoxrol>> GetPermisoxrol(int id)
        {
            _logger.LogInformation($"Buscando PermisoxRol con ID: {id}");

            var permisoxrol = await _context.Permisoxrols
                .Include(p => p.FkPermisoNavigation)
                .Include(p => p.FkRolNavigation)
                .FirstOrDefaultAsync(p => p.IdPermisoRol == id);

            if (permisoxrol == null)
            {
                _logger.LogWarning($"No existe el permiso con ID: {id}");
                return NotFound();
            }

            return permisoxrol;
        }

        // 🔥 NUEVO ENDPOINT IMPORTANTE PARA EL FRONT
        // Retorna SOLO los nombres de permisos de un rol
        // GET: api/Permisoxrols/rol-simple/1
        [HttpGet("rol-simple/{idRol}")]
        public async Task<ActionResult<IEnumerable<string>>> GetPermisosSoloNombres(int idRol)
        {
            _logger.LogInformation($"Obteniendo permisos simples del rol {idRol}");

            var permisos = await _context.Permisoxrols
                .Include(p => p.FkPermisoNavigation)
                .Where(p => p.FkRol == idRol)
                .Select(p => p.FkPermisoNavigation.Nombre)
                .ToListAsync();

            if (permisos.Count == 0)
                return NotFound("Este rol no tiene permisos asignados");

            return permisos;
        }

        // 🔥🔥🔥 EL QUE TU FRONT YA USA
        // GET: api/Permisoxrols/rol/1
        [HttpGet("rol/{idRol}")]
        public async Task<ActionResult<IEnumerable<object>>> GetPermisosSimplePorRol(int idRol)
        {
            _logger.LogInformation($"Obteniendo permisos del rol {idRol}");

            var permisos = await _context.Permisoxrols
                .Include(p => p.FkPermisoNavigation)
                .Where(p => p.FkRol == idRol)
                .Select(p => new
                {
                    p.FkPermisoNavigation.Nombre
                })
                .ToListAsync();

            if (permisos.Count == 0)
                return NotFound("Este rol no tiene permisos asignados");

            return permisos;
        }

        // ✅ 3. TRAER PERMISOS POR ID DE ROL
        [HttpGet("rol/id/{idRol}")]
        public async Task<ActionResult<IEnumerable<Permisoxrol>>> GetPermisosPorIdRol(int idRol)
        {
            _logger.LogInformation($"Trayendo permisos por IdRol {idRol}");

            var permisos = await _context.Permisoxrols
                .Include(p => p.FkPermisoNavigation)
                .Include(p => p.FkRolNavigation)
                .Where(p => p.FkRolNavigation.IdRol == idRol)
                .ToListAsync();

            if (permisos.Count == 0)
                return NotFound("Este rol no tiene permisos asignados");

            return permisos;
        }

        // ✅ 4. TRAER PERMISOS POR NOMBRE DE ROL
        [HttpGet("rol/nombre/{nombreRol}")]
        public async Task<ActionResult<IEnumerable<Permisoxrol>>> GetPermisosPorNombreRol(string nombreRol)
        {
            _logger.LogInformation($"Buscando permisos para el rol {nombreRol}");

            var permisos = await _context.Permisoxrols
                .Include(p => p.FkPermisoNavigation)
                .Include(p => p.FkRolNavigation)
                .Where(p => p.FkRolNavigation.NombreRol == nombreRol)
                .ToListAsync();

            if (permisos.Count == 0)
                return NotFound("Este rol no tiene permisos asignados");

            return permisos;
        }

        // ✅ 5. CREAR
        [HttpPost]
        public async Task<ActionResult<Permisoxrol>> PostPermisoxrol(Permisoxrol permisoxrol)
        {
            _logger.LogInformation("Creando nuevo permisoxrol...");

            _context.Permisoxrols.Add(permisoxrol);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPermisoxrol", new { id = permisoxrol.IdPermisoRol }, permisoxrol);
        }

        // ✅ 6. ACTUALIZAR
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPermisoxrol(int id, Permisoxrol permisoxrol)
        {
            _logger.LogInformation($"Actualizando PermisoxRol ID {id}");

            if (id != permisoxrol.IdPermisoRol)
                return BadRequest("El ID no coincide");

            _context.Entry(permisoxrol).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PermisoxrolExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // ✅ 7. ELIMINAR
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePermisoxrol(int id)
        {
            _logger.LogWarning($"Eliminando PermisoxRol ID {id}");

            var permisoxrol = await _context.Permisoxrols.FindAsync(id);

            if (permisoxrol == null)
                return NotFound();

            _context.Permisoxrols.Remove(permisoxrol);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PermisoxrolExists(int id)
        {
            return _context.Permisoxrols.Any(e => e.IdPermisoRol == id);
        }
    }
}
