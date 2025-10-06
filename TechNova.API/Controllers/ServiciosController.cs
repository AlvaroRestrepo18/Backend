using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechNova.API.Data;
using TechNova.API.Models;

namespace TechNova.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiciosController : ControllerBase
    {
        private readonly TechNovaContext _context;

        public ServiciosController(TechNovaContext context)
        {
            _context = context;
        }

        // ✅ GET: api/Servicios (con categoría y ventas asociadas)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Servicio>>> GetServicios()
        {
            try
            {
                var servicios = await _context.Servicios
                    .Include(s => s.Categoria)
                    .Include(s => s.Servicioxventa)
                        .ThenInclude(sv => sv.FkVentaNavigation) // 👈 cambio aquí (antes: FkVentaNavigation)
                    .ToListAsync();

                return Ok(servicios);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error interno del servidor al obtener servicios",
                    detalle = ex.Message
                });
            }
        }

        // 🔹 NUEVO ENDPOINT: lista simple (id, nombre, precio)
        [HttpGet("lista-simple")]
        public async Task<ActionResult<IEnumerable<object>>> GetServiciosSimple()
        {
            try
            {
                var servicios = await _context.Servicios
                    .Select(s => new
                    {
                        id = s.Id,
                        nombre = s.Nombre,
                        precio = s.Precio
                    })
                    .ToListAsync();

                return Ok(servicios);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error interno al obtener lista simple de servicios",
                    detalle = ex.Message
                });
            }
        }

        // 🔹 NUEVO ENDPOINT: obtener servicios por categoría
        [HttpGet("bycategoria/{categoriaId:int}")]
        public async Task<ActionResult<IEnumerable<Servicio>>> GetServiciosPorCategoria(int categoriaId)
        {
            try
            {
                var servicios = await _context.Servicios
                    .Where(s => s.CategoriaId == categoriaId)
                    .Include(s => s.Categoria)
                    .ToListAsync();

                if (!servicios.Any())
                {
                    return NotFound(new { mensaje = $"No se encontraron servicios para la categoría {categoriaId}" });
                }

                return Ok(servicios);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error al obtener servicios por categoría",
                    detalle = ex.Message
                });
            }
        }

        // 🔹 NUEVO ENDPOINT: obtener ventas asociadas a un servicio
        [HttpGet("{id:int}/ventas")]
        public async Task<ActionResult<IEnumerable<object>>> GetVentasDeServicio(int id)
        {
            try
            {
                var servicio = await _context.Servicios
                    .Include(s => s.Servicioxventa)
                        .ThenInclude(sv => sv.FkVentaNavigation) // 👈 cambio aquí (antes: FkVentaNavigation)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (servicio == null)
                {
                    return NotFound(new { mensaje = $"No se encontró un servicio con ID {id}" });
                }

                var ventas = servicio.Servicioxventa.Select(sv => new
                {
                    ventaId = sv.FkVenta,              // 👈 cambio aquí (antes: FkVenta)
                    fecha = sv.FkVentaNavigation.fecha,            // 👈 cambio aquí (antes: FkVentaNavigation.fecha)
                    clienteId = sv.FkVentaNavigation.ClienteId,    // 👈 cambio aquí (antes: FkVentaNavigation.FkCliente)
                    valor = sv.ValorTotal
                });

                return Ok(ventas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error al obtener ventas de un servicio",
                    detalle = ex.Message
                });
            }
        }

        // ✅ GET: api/Servicios/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Servicio>> GetServicio(int id)
        {
            try
            {
                var servicio = await _context.Servicios
                    .Include(s => s.Categoria)
                    .Include(s => s.Servicioxventa)
                        .ThenInclude(sv => sv.FkVentaNavigation) // 👈 cambio aquí (antes: FkVentaNavigation)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (servicio == null)
                {
                    return NotFound(new { mensaje = $"No se encontró un servicio con ID {id}" });
                }

                return Ok(servicio);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error al obtener servicio",
                    detalle = ex.Message
                });
            }
        }

        // ✅ PUT: api/Servicios/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutServicio(int id, Servicio servicio)
        {
            if (id != servicio.Id)
            {
                return BadRequest(new { mensaje = "El ID del servicio no coincide con el de la URL" });
            }

            if (string.IsNullOrWhiteSpace(servicio.Nombre))
            {
                return BadRequest(new { mensaje = "El nombre del servicio es obligatorio" });
            }

            try
            {
                _context.Entry(servicio).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Servicio actualizado correctamente", servicio });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServicioExists(id))
                {
                    return NotFound(new { mensaje = $"No existe un servicio con ID {id}" });
                }
                throw;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al actualizar servicio", detalle = ex.Message });
            }
        }

        // ✅ POST: api/Servicios
        [HttpPost]
        public async Task<ActionResult<Servicio>> PostServicio(Servicio servicio)
        {
            if (string.IsNullOrWhiteSpace(servicio.Nombre))
            {
                return BadRequest(new { mensaje = "El nombre del servicio es obligatorio" });
            }

            try
            {
                _context.Servicios.Add(servicio);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetServicio), new { id = servicio.Id }, servicio);
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error al guardar en la base de datos",
                    detalle = dbEx.InnerException?.Message ?? dbEx.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error inesperado al guardar servicio", detalle = ex.Message });
            }
        }

        // ✅ DELETE: api/Servicios/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteServicio(int id)
        {
            try
            {
                var servicio = await _context.Servicios.FindAsync(id);
                if (servicio == null)
                {
                    return NotFound(new { mensaje = $"No existe un servicio con ID {id}" });
                }

                _context.Servicios.Remove(servicio);
                await _context.SaveChangesAsync();

                return Ok(new { mensaje = $"Servicio con ID {id} eliminado correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al eliminar servicio", detalle = ex.Message });
            }
        }

        // 🔍 Helper
        private bool ServicioExists(int id) =>
            _context.Servicios.Any(e => e.Id == id);
    }
}
