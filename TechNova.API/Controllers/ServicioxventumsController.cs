using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechNova.API.Data;
using TechNova.API.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechNova.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicioxventumsController : ControllerBase
    {
        private readonly TechNovaContext _context;

        public ServicioxventumsController(TechNovaContext context)
        {
            _context = context;
        }

        // ✅ GET: api/Servicioxventums
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Servicioxventum>>> GetServicioxventa()
        {
            return await _context.Servicioxventa.AsNoTracking().ToListAsync();
        }

        // ✅ GET: api/Servicioxventums/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Servicioxventum>> GetServicioxventum(int id)
        {
            var servicioxventum = await _context.Servicioxventa.AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (servicioxventum == null)
                return NotFound(new { mensaje = "Servicio por venta no encontrado." });

            return servicioxventum;
        }

        // ✅ GET: api/Servicioxventums/ByVenta/5
        [HttpGet("ByVenta/{ventaId}")]
        public async Task<ActionResult<IEnumerable<Servicioxventum>>> GetServiciosByVenta(int ventaId)
        {
            var servicios = await _context.Servicioxventa
                .Where(s => s.FkVenta == ventaId)
                .AsNoTracking()
                .ToListAsync();

            // Devuelve siempre un array, incluso si está vacío
            return servicios;
        }

        // ✅ POST: api/Servicioxventums
        [HttpPost]
        public async Task<ActionResult<Servicioxventum>> PostServicioxventum(Servicioxventum servicioxventum)
        {
            // 🔹 Validaciones básicas
            if (servicioxventum.FkServicio <= 0)
                return BadRequest(new { mensaje = "El campo FkServicio es obligatorio y debe ser mayor que 0." });

            if (servicioxventum.FkVenta <= 0)
                return BadRequest(new { mensaje = "El campo FkVenta es obligatorio y debe ser mayor que 0." });

            if (servicioxventum.Precio <= 0)
                return BadRequest(new { mensaje = "El campo Precio debe ser mayor que 0." });

            // 🔹 Validar existencia de FK
            var servicioExist = await _context.Servicios.FindAsync(servicioxventum.FkServicio);
            if (servicioExist == null)
                return BadRequest(new { mensaje = $"No existe un servicio con Id {servicioxventum.FkServicio}." });

            var ventaExist = await _context.Ventas.FindAsync(servicioxventum.FkVenta);
            if (ventaExist == null)
                return BadRequest(new { mensaje = $"No existe una venta con Id {servicioxventum.FkVenta}." });

            // 🔹 Evitar duplicados (mismo servicio en la misma venta)
            var duplicado = await _context.Servicioxventa
                .AnyAsync(s => s.FkServicio == servicioxventum.FkServicio && s.FkVenta == servicioxventum.FkVenta);

            if (duplicado)
                return BadRequest(new { mensaje = "Este servicio ya está registrado para la venta indicada." });

            try
            {
                // 🔹 Calcular ValorTotal si no viene del frontend
                if (servicioxventum.ValorTotal == null || servicioxventum.ValorTotal == 0)
                    servicioxventum.ValorTotal = servicioxventum.Precio;

                _context.Servicioxventa.Add(servicioxventum);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetServicioxventum", new { id = servicioxventum.Id }, servicioxventum);
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { mensaje = "Error de base de datos al guardar el servicio.", detalle = dbEx.Message });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error inesperado al registrar el servicio.", detalle = ex.Message });
            }
        }

        // ✅ PUT: api/Servicioxventums/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutServicioxventum(int id, Servicioxventum servicioxventum)
        {
            if (id != servicioxventum.Id)
                return BadRequest(new { mensaje = "El ID del servicio no coincide con el ID del registro." });

            // 🔹 Validar existencia de FK
            var servicioExist = await _context.Servicios.FindAsync(servicioxventum.FkServicio);
            if (servicioExist == null)
                return BadRequest(new { mensaje = $"No existe un servicio con Id {servicioxventum.FkServicio}." });

            var ventaExist = await _context.Ventas.FindAsync(servicioxventum.FkVenta);
            if (ventaExist == null)
                return BadRequest(new { mensaje = $"No existe una venta con Id {servicioxventum.FkVenta}." });

            _context.Entry(servicioxventum).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { mensaje = "Servicio por venta actualizado correctamente." });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServicioxventumExists(id))
                    return NotFound(new { mensaje = "Registro no encontrado para actualizar." });
                else
                    throw;
            }
        }

        // ✅ DELETE: api/Servicioxventums/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteServicioxventum(int id)
        {
            var servicioxventum = await _context.Servicioxventa.FindAsync(id);
            if (servicioxventum == null)
                return NotFound(new { mensaje = "El registro no existe o ya fue eliminado." });

            _context.Servicioxventa.Remove(servicioxventum);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Servicio eliminado correctamente." });
        }

        private bool ServicioxventumExists(int id)
        {
            return _context.Servicioxventa.Any(e => e.Id == id);
        }
    }
}
