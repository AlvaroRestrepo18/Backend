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
        private readonly ILogger<VentasController> _logger;

        public VentasController(TechNovaContext context, ILogger<VentasController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ✅ DEBUG: Endpoint para verificar producto
        [HttpGet("debug-producto/{id}")]
        public async Task<ActionResult> DebugProducto(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
                return NotFound($"Producto con ID {id} no encontrado");

            return Ok(new
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                Cantidad = producto.Cantidad,
                CategoriaId = producto.CategoriaId,
                Precio = producto.Precio
            });
        }

        // ✅ GET: api/Ventas (con cliente, productos y servicios)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Venta>>> GetVentas()
        {
            return await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Productoxventa)
                    .ThenInclude(pv => pv.FkproductoNavigation)
                        .ThenInclude(p => p.Categoria)
                .Include(v => v.Servicioxventa)
                    .ThenInclude(sv => sv.FkServicioNavigation)
                        .ThenInclude(s => s.Categoria)
                .ToListAsync();
        }

        // ✅ GET: api/Ventas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Venta>> GetVenta(int id)
        {
            var venta = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Productoxventa)
                    .ThenInclude(pv => pv.FkproductoNavigation)
                        .ThenInclude(p => p.Categoria)
                .Include(v => v.Servicioxventa)
                    .ThenInclude(sv => sv.FkServicioNavigation)
                        .ThenInclude(s => s.Categoria)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venta == null)
                return NotFound();

            return venta;
        }

        // ✅ PUT: api/Ventas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVenta(int id, Venta venta)
        {
            if (id != venta.Id)
                return BadRequest("El ID de la venta no coincide.");

            _context.Entry(venta).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VentaExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // ✅ POST: api/Ventas (VERSIÓN CORREGIDA - SIN ACTUALIZAR STOCK)
        [HttpPost]
        public async Task<ActionResult<Venta>> PostVenta(Venta venta)
        {
            try
            {
                _logger.LogInformation("🔵 INICIANDO CREACIÓN DE VENTA");

                if (venta == null)
                    return BadRequest("La venta no puede ser nula.");

                // ⚡ Validar que el cliente exista
                var clienteExistente = await _context.Clientes.FindAsync(venta.ClienteId);
                if (clienteExistente == null)
                    return BadRequest($"El cliente con ID {venta.ClienteId} no existe.");

                // ⚡ Forzar fecha actual si no viene del frontend
                if (venta.fecha == default)
                    venta.fecha = DateTime.Now;

                // 🔹 Prevenir referencias erróneas
                venta.Cliente = null;

                // 🔥 DEBUG: Log de productos en la venta
                _logger.LogInformation($"🛒 Venta recibida - Productos: {venta.Productoxventa?.Count ?? 0}, Servicios: {venta.Servicioxventa?.Count ?? 0}");

                // 1️⃣ GUARDAR LA VENTA PRINCIPAL
                _context.Ventas.Add(venta);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"✅ Venta creada - ID: {venta.Id}");

                // 2️⃣ GUARDAR PRODUCTOS ASOCIADOS A LA VENTA
                if (venta.Productoxventa != null && venta.Productoxventa.Any())
                {
                    foreach (var pv in venta.Productoxventa)
                    {
                        pv.Id = 0;
                        pv.VentaId = venta.Id;
                        pv.FkproductoNavigation = null;
                        _context.Productoxventa.Add(pv);
                        _logger.LogInformation($"✅ Producto vinculado a venta - ProductoID: {pv.ProductoId}, Cantidad: {pv.Cantidad}");

                        // 🔥 NOTA: La actualización de stock ahora la hace ProductoxventumsController automáticamente
                        // cuando se crea esta relación Productoxventa
                    }
                }

                // 3️⃣ GUARDAR SERVICIOS ASOCIADOS
                if (venta.Servicioxventa != null && venta.Servicioxventa.Any())
                {
                    foreach (var sv in venta.Servicioxventa)
                    {
                        var servicioExistente = await _context.Servicios.FindAsync(sv.FkServicio);
                        if (servicioExistente == null)
                            return BadRequest($"El servicio con ID {sv.FkServicio} no existe.");

                        sv.Id = 0;
                        sv.FkVenta = venta.Id;
                        sv.FkServicioNavigation = null;
                        _context.Servicioxventa.Add(sv);
                        _logger.LogInformation($"✅ Servicio vinculado a venta - ServicioID: {sv.FkServicio}");
                    }
                }

                // 4️⃣ GUARDAR TODOS LOS CAMBIOS FINALES
                await _context.SaveChangesAsync();
                _logger.LogInformation("✅ Todos los cambios guardados (venta + productos + servicios)");

                // 5️⃣ RECARGAR LA VENTA CON RELACIONES PARA RETORNAR
                var ventaConDetalles = await _context.Ventas
                    .Include(v => v.Cliente)
                    .Include(v => v.Productoxventa)
                        .ThenInclude(pv => pv.FkproductoNavigation)
                            .ThenInclude(p => p.Categoria)
                    .Include(v => v.Servicioxventa)
                        .ThenInclude(sv => sv.FkServicioNavigation)
                            .ThenInclude(s => s.Categoria)
                    .FirstOrDefaultAsync(v => v.Id == venta.Id);

                _logger.LogInformation("🎉 VENTA COMPLETADA EXITOSAMENTE");

                return CreatedAtAction(nameof(GetVenta), new { id = venta.Id }, ventaConDetalles);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ ERROR en PostVenta: {ex.Message}");
                _logger.LogError($"❌ StackTrace: {ex.StackTrace}");
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // ✅ DELETE: api/Ventas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVenta(int id)
        {
            try
            {
                var venta = await _context.Ventas
                    .Include(v => v.Productoxventa)
                    .Include(v => v.Servicioxventa)
                    .FirstOrDefaultAsync(v => v.Id == id);

                if (venta == null)
                    return NotFound();

                _logger.LogInformation($"🗑️ Eliminando venta - ID: {id}");

                // 🔥 NOTA: La restauración de stock ahora la hace ProductoxventumsController automáticamente
                // cuando se eliminan las relaciones Productoxventa

                if (venta.Productoxventa != null && venta.Productoxventa.Any())
                    _context.Productoxventa.RemoveRange(venta.Productoxventa);

                if (venta.Servicioxventa != null && venta.Servicioxventa.Any())
                    _context.Servicioxventa.RemoveRange(venta.Servicioxventa);

                _context.Ventas.Remove(venta);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Venta eliminada - ID: {id}");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ ERROR eliminando venta: {ex.Message}");
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        private bool VentaExists(int id)
        {
            return _context.Ventas.Any(e => e.Id == id);
        }
    }
}