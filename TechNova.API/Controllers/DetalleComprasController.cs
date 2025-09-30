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
    public class DetalleComprasController : ControllerBase
    {
        private readonly TechNovaContext _context;

        public DetalleComprasController(TechNovaContext context)
        {
            _context = context;
        }

        // ✅ GET: api/DetalleCompras
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetDetalleCompras()
        {
            var detalles = await _context.DetalleCompras
                .Include(d => d.Producto)
                .Include(d => d.Compra)
                .Select(d => new {
                    d.Id,
                    d.CompraId,
                    d.ProductoId,
                    Producto = new
                    {
                        d.Producto.Id,
                        d.Producto.Nombre,
                        d.Producto.Precio
                    },
                    d.Cantidad,
                    d.PrecioUnitario,
                    d.SubtotalItems
                })
                .ToListAsync();

            return Ok(detalles);
        }

        // ✅ GET: api/DetalleCompras/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetDetalleCompra(int id)
        {
            var detalle = await _context.DetalleCompras
                .Include(d => d.Producto)
                .Include(d => d.Compra)
                .Where(d => d.Id == id)
                .Select(d => new {
                    d.Id,
                    d.CompraId,
                    d.ProductoId,
                    Producto = new
                    {
                        d.Producto.Id,
                        d.Producto.Nombre,
                        d.Producto.Precio
                    },
                    d.Cantidad,
                    d.PrecioUnitario,
                    d.SubtotalItems
                })
                .FirstOrDefaultAsync();

            if (detalle == null)
                return NotFound(new { message = $"DetalleCompra {id} no encontrado." });

            return Ok(detalle);
        }

        // ✅ PUT: api/DetalleCompras/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDetalleCompra(int id, DetalleCompra detalleCompra)
        {
            if (id != detalleCompra.Id)
                return BadRequest(new { message = "El ID no coincide con el detalle enviado." });

            var detalleExistente = await _context.DetalleCompras.FindAsync(id);
            if (detalleExistente == null)
                return NotFound(new { message = $"DetalleCompra {id} no existe." });

            // 🔄 Actualizar campos
            detalleExistente.ProductoId = detalleCompra.ProductoId;
            detalleExistente.CompraId = detalleCompra.CompraId;
            detalleExistente.Cantidad = detalleCompra.Cantidad;
            detalleExistente.PrecioUnitario = detalleCompra.PrecioUnitario;
            detalleExistente.SubtotalItems = detalleCompra.SubtotalItems;

            try
            {
                await _context.SaveChangesAsync();

                var detalleActualizado = await _context.DetalleCompras
                    .Include(d => d.Producto)
                    .Include(d => d.Compra)
                    .Where(d => d.Id == id)
                    .Select(d => new {
                        d.Id,
                        d.CompraId,
                        d.ProductoId,
                        Producto = new
                        {
                            d.Producto.Id,
                            d.Producto.Nombre,
                            d.Producto.Precio
                        },
                        d.Cantidad,
                        d.PrecioUnitario,
                        d.SubtotalItems
                    })
                    .FirstOrDefaultAsync();

                return Ok(detalleActualizado);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DetalleCompraExists(id))
                    return NotFound(new { message = $"DetalleCompra {id} no existe." });

                throw;
            }
        }

        // ✅ POST: api/DetalleCompras
        [HttpPost]
        public async Task<ActionResult<object>> PostDetalleCompra(DetalleCompra detalleCompra)
        {
            if (detalleCompra == null)
                return BadRequest(new { message = "Datos inválidos." });

            _context.DetalleCompras.Add(detalleCompra);
            await _context.SaveChangesAsync();

            var detalleCreado = await _context.DetalleCompras
                .Include(d => d.Producto)
                .Include(d => d.Compra)
                .Where(d => d.Id == detalleCompra.Id)
                .Select(d => new {
                    d.Id,
                    d.CompraId,
                    d.ProductoId,
                    Producto = new
                    {
                        d.Producto.Id,
                        d.Producto.Nombre,
                        d.Producto.Precio
                    },
                    d.Cantidad,
                    d.PrecioUnitario,
                    d.SubtotalItems
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetDetalleCompra), new { id = detalleCompra.Id }, detalleCreado);
        }

        // ✅ DELETE: api/DetalleCompras/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDetalleCompra(int id)
        {
            var detalleCompra = await _context.DetalleCompras.FindAsync(id);
            if (detalleCompra == null)
                return NotFound(new { message = $"DetalleCompra {id} no encontrado." });

            _context.DetalleCompras.Remove(detalleCompra);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Detalle de compra eliminado con éxito." });
        }

        private bool DetalleCompraExists(int id)
        {
            return _context.DetalleCompras.Any(e => e.Id == id);
        }
    }
}
