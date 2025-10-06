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
        private readonly ILogger<ProductoxventumsController> _logger;

        public ProductoxventumsController(TechNovaContext context, ILogger<ProductoxventumsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ✅ GET: api/Productoxventums
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Productoxventum>>> GetAll()
        {
            return await _context.Productoxventa
                .Include(p => p.FkproductoNavigation)
                .Include(v => v.FkVentaNavigation)
                .ToListAsync();
        }

        // ✅ GET: api/Productoxventums/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Productoxventum>> GetById(int id)
        {
            var productoVenta = await _context.Productoxventa
                .Include(p => p.FkproductoNavigation)
                .Include(v => v.FkVentaNavigation)
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
                .Include(p => p.FkproductoNavigation)
                .Where(x => x.VentaId == ventaId)
                .ToListAsync();

            return productos;
        }

        // ✅ GET: api/Productoxventums/byproducto/3 (ventas en las que participa un producto)
        [HttpGet("byproducto/{productoId}")]
        public async Task<ActionResult<IEnumerable<Productoxventum>>> GetByProductoId(int productoId)
        {
            var ventas = await _context.Productoxventa
                .Include(v => v.FkVentaNavigation)
                .Where(x => x.ProductoId == productoId)
                .ToListAsync();

            return ventas;
        }

        // ✅ POST: api/Productoxventums
        [HttpPost]
        public async Task<ActionResult<Productoxventum>> Create(Productoxventum productoVenta)
        {
            try
            {
                _logger.LogInformation($"🎯 PRODUCTOXVENTA - Creando relación para producto {productoVenta.ProductoId} en venta {productoVenta.VentaId}");

                // 1. ACTUALIZAR STOCK DEL PRODUCTO
                var producto = await _context.Productos.FindAsync(productoVenta.ProductoId);
                if (producto == null)
                {
                    _logger.LogError($"❌ PRODUCTOXVENTA - Producto no encontrado: {productoVenta.ProductoId}");
                    return BadRequest($"Producto con ID {productoVenta.ProductoId} no existe");
                }

                if (!producto.Cantidad.HasValue || !productoVenta.Cantidad.HasValue)
                {
                    _logger.LogError($"❌ PRODUCTOXVENTA - Cantidades inválidas: Producto={producto.Cantidad}, Venta={productoVenta.Cantidad}");
                    return BadRequest("Cantidades inválidas");
                }

                if (producto.Cantidad.Value < productoVenta.Cantidad.Value)
                {
                    _logger.LogError($"❌ PRODUCTOXVENTA - Stock insuficiente: {producto.Nombre} (Stock: {producto.Cantidad}, Solicitado: {productoVenta.Cantidad})");
                    return BadRequest($"Stock insuficiente: {producto.Nombre} (Stock: {producto.Cantidad}, Solicitado: {productoVenta.Cantidad})");
                }

                // 🔥 ACTUALIZAR STOCK
                int cantidadAnterior = producto.Cantidad.Value;
                producto.Cantidad = cantidadAnterior - productoVenta.Cantidad.Value;
                _logger.LogInformation($"✅ PRODUCTOXVENTA - Stock actualizado: {producto.Nombre} {cantidadAnterior} -> {producto.Cantidad}");

                // 2. CREAR LA RELACIÓN PRODUCTO-VENTA
                if (!productoVenta.ValorTotal.HasValue && productoVenta.Cantidad.HasValue)
                {
                    productoVenta.ValorTotal = productoVenta.ValorUnitario * productoVenta.Cantidad.Value;
                }

                _context.Productoxventa.Add(productoVenta);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"🎉 PRODUCTOXVENTA CREADA - ID: {productoVenta.Id}");
                return CreatedAtAction(nameof(GetById), new { id = productoVenta.Id }, productoVenta);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ ERROR en Productoxventa: {ex.Message}");
                _logger.LogError($"❌ StackTrace: {ex.StackTrace}");
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        // ✅ PUT: api/Productoxventums/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Productoxventum productoVenta)
        {
            try
            {
                if (id != productoVenta.Id)
                    return BadRequest();

                _logger.LogInformation($"🎯 PRODUCTOXVENTA - Actualizando relación {id}");

                if (!productoVenta.ValorTotal.HasValue && productoVenta.Cantidad.HasValue)
                {
                    productoVenta.ValorTotal = productoVenta.ValorUnitario * productoVenta.Cantidad.Value;
                }

                _context.Entry(productoVenta).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ PRODUCTOXVENTA ACTUALIZADA - ID: {id}");
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Productoxventa.Any(e => e.Id == id))
                    return NotFound();
                else
                    throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ ERROR actualizando Productoxventa: {ex.Message}");
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        // ✅ DELETE: api/Productoxventums/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation($"🎯 PRODUCTOXVENTA - Eliminando relación {id}");

                var productoVenta = await _context.Productoxventa.FindAsync(id);
                if (productoVenta == null)
                {
                    _logger.LogWarning($"❌ PRODUCTOXVENTA - No encontrada: {id}");
                    return NotFound();
                }

                // 🔥 RESTAURAR STOCK AL ELIMINAR
                if (productoVenta.Cantidad.HasValue)
                {
                    var producto = await _context.Productos.FindAsync(productoVenta.ProductoId);
                    if (producto != null)
                    {
                        int cantidadActual = producto.Cantidad ?? 0;
                        producto.Cantidad = cantidadActual + productoVenta.Cantidad.Value;
                        _logger.LogInformation($"🔄 PRODUCTOXVENTA - Stock restaurado: {producto.Nombre} {cantidadActual} -> {producto.Cantidad}");
                    }
                }

                _context.Productoxventa.Remove(productoVenta);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ PRODUCTOXVENTA ELIMINADA - ID: {id}");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ ERROR eliminando Productoxventa: {ex.Message}");
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }
    }
}