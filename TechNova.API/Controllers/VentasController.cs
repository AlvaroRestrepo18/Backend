using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechNova.API.Data;
using TechNova.API.Models;

namespace TechNova.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VentasController : ControllerBase
    {
        private readonly TechNovaContext _context;

        public VentasController(TechNovaContext context)
        {
            _context = context;
        }

        // ✅ GET: api/Ventas (con cliente, productos y servicios + relaciones profundas)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Venta>>> GetVentas()
        {
            return await _context.Ventas
                .Include(v => v.FkClienteNavigation)
                .Include(v => v.Productoxventa)
                    .ThenInclude(pv => pv.Producto)
                        .ThenInclude(p => p.Categoria)
                .Include(v => v.Servicioxventa)
                    .ThenInclude(sv => sv.FkServicioNavigation)
                        .ThenInclude(s => s.Categoria)
                .ToListAsync();
        }

        // ✅ GET: api/Ventas/5 (con todas las relaciones)
        [HttpGet("{id}")]
        public async Task<ActionResult<Venta>> GetVenta(int id)
        {
            var venta = await _context.Ventas
                .Include(v => v.FkClienteNavigation)
                .Include(v => v.Productoxventa)
                    .ThenInclude(pv => pv.Producto)
                        .ThenInclude(p => p.Categoria)
                .Include(v => v.Servicioxventa)
                    .ThenInclude(sv => sv.FkServicioNavigation)
                        .ThenInclude(s => s.Categoria)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venta == null)
            {
                return NotFound();
            }

            return venta;
        }

        // ✅ PUT: api/Ventas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVenta(int id, Venta venta)
        {
            if (id != venta.Id)
            {
                return BadRequest();
            }

            _context.Entry(venta).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VentaExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // ✅ POST: api/Ventas (devuelve la venta completa con productos y servicios)
        [HttpPost]
        public async Task<ActionResult<Venta>> PostVenta(Venta venta)
        {
            if (venta == null)
            {
                return BadRequest("La venta no puede ser nula.");
            }

            // ⚡ Forzamos la fecha con DateOnly
            venta.fecha = DateOnly.FromDateTime(DateTime.Now);

            _context.Ventas.Add(venta);
            await _context.SaveChangesAsync();

            // 🔹 Recargamos con todas las relaciones
            var ventaConDetalles = await _context.Ventas
                .Include(v => v.FkClienteNavigation)
                .Include(v => v.Productoxventa)
                    .ThenInclude(pv => pv.Producto)
                        .ThenInclude(p => p.Categoria)
                .Include(v => v.Servicioxventa)
                    .ThenInclude(sv => sv.FkServicioNavigation)
                        .ThenInclude(s => s.Categoria)
                .FirstOrDefaultAsync(v => v.Id == venta.Id);

            return CreatedAtAction("GetVenta", new { id = venta.Id }, ventaConDetalles);
        }

        // ✅ DELETE: api/Ventas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVenta(int id)
        {
            var venta = await _context.Ventas.FindAsync(id);
            if (venta == null)
            {
                return NotFound();
            }

            _context.Ventas.Remove(venta);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool VentaExists(int id)
        {
            return _context.Ventas.Any(e => e.Id == id);
        }
    }
}
