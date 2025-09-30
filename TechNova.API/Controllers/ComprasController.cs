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
    public class ComprasController : ControllerBase
    {
        private readonly TechNovaContext _context;

        public ComprasController(TechNovaContext context)
        {
            _context = context;
        }

        // ✅ GET: api/Compras
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Compra>>> GetCompras()
        {
            var compras = await _context.Compras
                .Include(c => c.Proveedor)
                .Include(c => c.DetallesCompra)
                    .ThenInclude(d => d.Producto)
                .ToListAsync();

            return Ok(compras);
        }

        // ✅ GET: api/Compras/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Compra>> GetCompra(int id)
        {
            var compra = await _context.Compras
                .Include(c => c.Proveedor)
                .Include(c => c.DetallesCompra)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (compra == null)
                return NotFound(new { message = $"Compra {id} no encontrada." });

            return Ok(compra);
        }

        // ✅ PUT: api/Compras/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCompra(int id, Compra compra)
        {
            if (id != compra.Id)
                return BadRequest(new { message = "El ID de la compra no coincide." });

            _context.Entry(compra).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Compra actualizada con éxito." });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CompraExists(id))
                    return NotFound(new { message = $"Compra {id} no existe." });

                throw;
            }
        }

        // ✅ POST: api/Compras
        [HttpPost]
        public async Task<ActionResult<Compra>> PostCompra(Compra compra)
        {
            if (compra == null)
                return BadRequest(new { message = "Datos inválidos." });

            // Agregar compra con sus detalles en una transacción
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Compras.Add(compra);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Retorna con las relaciones cargadas
                var compraCreada = await _context.Compras
                    .Include(c => c.Proveedor)
                    .Include(c => c.DetallesCompra)
                        .ThenInclude(d => d.Producto)
                    .FirstOrDefaultAsync(c => c.Id == compra.Id);

                return CreatedAtAction(nameof(GetCompra), new { id = compra.Id }, compraCreada);
            }
            catch (System.Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Error al crear la compra.", error = ex.Message });
            }
        }

        // ✅ DELETE: api/Compras/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCompra(int id)
        {
            var compra = await _context.Compras
            .Include(c => c.DetallesCompra)
            .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(c => c.Id == id);



            if (compra == null)
                return NotFound(new { message = $"Compra {id} no encontrada." });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Elimina primero los detalles
                _context.DetalleCompras.RemoveRange(compra.DetallesCompra);

                _context.Compras.Remove(compra);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return Ok(new { message = "Compra eliminada con éxito." });
            }
            catch (System.Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Error al eliminar la compra.", error = ex.Message });
            }
        }

        private bool CompraExists(int id)
        {
            return _context.Compras.Any(e => e.Id == id);
        }
    }
}
