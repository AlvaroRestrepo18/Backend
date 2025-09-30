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
    public class ProductosController : ControllerBase
    {
        private readonly TechNovaContext _context;

        public ProductosController(TechNovaContext context)
        {
            _context = context;
        }

        // ✅ GET: api/Productos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Producto>>> GetProductos()
        {
            try
            {
                var productos = await _context.Productos
                    .Include(p => p.Categoria)
                    .ToListAsync();

                return Ok(productos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error interno del servidor al obtener productos",
                    detalle = ex.Message
                });
            }
        }

        // 🔹 NUEVO ENDPOINT: lista simple (id, nombre, precio)
        [HttpGet("lista-simple")]
        public async Task<ActionResult<IEnumerable<object>>> GetProductosSimple()
        {
            try
            {
                var productos = await _context.Productos
                    .Select(p => new
                    {
                        id = p.Id,
                        nombre = p.Nombre,
                        precio = p.Precio
                    })
                    .ToListAsync();

                return Ok(productos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error interno al obtener lista simple de productos",
                    detalle = ex.Message
                });
            }
        }

        // ✅ GET: api/Productos/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Producto>> GetProducto(int id)
        {
            try
            {
                var producto = await _context.Productos
                    .Include(p => p.Categoria)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (producto == null)
                {
                    return NotFound(new { mensaje = $"No se encontró un producto con ID {id}" });
                }

                return Ok(producto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error al obtener producto",
                    detalle = ex.Message
                });
            }
        }

        // ✅ PUT: api/Productos/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutProducto(int id, Producto producto)
        {
            if (id != producto.Id)
            {
                return BadRequest(new { mensaje = "El ID del producto no coincide con el de la URL" });
            }

            if (string.IsNullOrWhiteSpace(producto.Nombre))
            {
                return BadRequest(new { mensaje = "El nombre del producto es obligatorio" });
            }

            try
            {
                _context.Entry(producto).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return Ok(new { mensaje = "Producto actualizado correctamente", producto });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductoExists(id))
                {
                    return NotFound(new { mensaje = $"No existe un producto con ID {id}" });
                }
                throw;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al actualizar producto", detalle = ex.Message });
            }
        }

        // ✅ POST: api/Productos
        [HttpPost]
        public async Task<ActionResult<Producto>> PostProducto(Producto producto)
        {
            if (string.IsNullOrWhiteSpace(producto.Nombre))
            {
                return BadRequest(new { mensaje = "El nombre del producto es obligatorio" });
            }

            try
            {
                producto.FechaCreacion = DateTime.Now;
                _context.Productos.Add(producto);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetProducto), new { id = producto.Id }, producto);
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
                return StatusCode(500, new { mensaje = "Error inesperado al guardar producto", detalle = ex.Message });
            }
        }

        // ✅ DELETE: api/Productos/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteProducto(int id)
        {
            try
            {
                var producto = await _context.Productos.FindAsync(id);
                if (producto == null)
                {
                    return NotFound(new { mensaje = $"No existe un producto con ID {id}" });
                }

                _context.Productos.Remove(producto);
                await _context.SaveChangesAsync();

                return Ok(new { mensaje = $"Producto con ID {id} eliminado correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al eliminar producto", detalle = ex.Message });
            }
        }

        // 🔍 Helper
        private bool ProductoExists(int id) =>
            _context.Productos.Any(e => e.Id == id);
    }
}
