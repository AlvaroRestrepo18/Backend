using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechNova.API.Data;
using TechNova.API.Models;

namespace TechNova.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductoxventumsController : ControllerBase
    {
        private readonly TechNovaContext _context;

        public ProductoxventumsController(TechNovaContext context)
        {
            _context = context;
        }

        // ✅ GET: api/Productoxventums
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Productoxventum>>> GetAll()
        {
            return await _context.Productoxventa
                .Include(p => p.Producto)
                .Include(v => v.Venta)
                .ToListAsync();
        }

        // ✅ GET: api/Productoxventums/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Productoxventum>> GetById(int id)
        {
            var productoVenta = await _context.Productoxventa
                .Include(p => p.Producto)
                .Include(v => v.Venta)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (productoVenta == null)
                return NotFound();

            return productoVenta;
        }

        // ✅ GET: api/Productoxventums/productoxventa/5  (productos de una venta)
        [HttpGet("productoxventa/{ventaId}")]
        public async Task<ActionResult<IEnumerable<Productoxventum>>> GetByVentaId(int ventaId)
        {
            var productos = await _context.Productoxventa
                .Include(p => p.Producto)
                .Where(x => x.VentaId == ventaId)
                .ToListAsync();

            return productos;
        }

        // ✅ GET: api/Productoxventums/byproducto/3 (ventas en las que participa un producto)
        [HttpGet("byproducto/{productoId}")]
        public async Task<ActionResult<IEnumerable<Productoxventum>>> GetByProductoId(int productoId)
        {
            var ventas = await _context.Productoxventa
                .Include(v => v.Venta)
                .Where(x => x.ProductoId == productoId)
                .ToListAsync();

            return ventas;
        }

        // ✅ POST: api/Productoxventums
        [HttpPost]
        public async Task<ActionResult<Productoxventum>> Create(Productoxventum productoVenta)
        {
            // Recalcular valor total si no viene
            if (!productoVenta.ValorTotal.HasValue && productoVenta.Cantidad.HasValue)
            {
                productoVenta.ValorTotal = productoVenta.ValorUnitario * productoVenta.Cantidad.Value;
            }

            _context.Productoxventa.Add(productoVenta);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = productoVenta.Id }, productoVenta);
        }

        // ✅ PUT: api/Productoxventums/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Productoxventum productoVenta)
        {
            if (id != productoVenta.Id)
                return BadRequest();

            if (!productoVenta.ValorTotal.HasValue && productoVenta.Cantidad.HasValue)
            {
                productoVenta.ValorTotal = productoVenta.ValorUnitario * productoVenta.Cantidad.Value;
            }

            _context.Entry(productoVenta).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Productoxventa.Any(e => e.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // ✅ DELETE: api/Productoxventums/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var productoVenta = await _context.Productoxventa.FindAsync(id);
            if (productoVenta == null)
                return NotFound();

            _context.Productoxventa.Remove(productoVenta);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
