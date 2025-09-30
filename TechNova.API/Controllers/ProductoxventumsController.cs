using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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

        // GET: api/Productoxventums
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Productoxventum>>> GetProductoxventa()
        {
            return await _context.Productoxventa.ToListAsync();
        }

        // GET: api/Productoxventums/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Productoxventum>> GetProductoxventum(int id)
        {
            var productoxventum = await _context.Productoxventa.FindAsync(id);

            if (productoxventum == null)
            {
                return NotFound();
            }

            return productoxventum;
        }

        // PUT: api/Productoxventums/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProductoxventum(int id, Productoxventum productoxventum)
        {
            if (id != productoxventum.Id)
            {
                return BadRequest();
            }

            _context.Entry(productoxventum).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductoxventumExists(id))
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

        // POST: api/Productoxventums
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Productoxventum>> PostProductoxventum(Productoxventum productoxventum)
        {
            _context.Productoxventa.Add(productoxventum);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProductoxventum", new { id = productoxventum.Id }, productoxventum);
        }

        // DELETE: api/Productoxventums/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProductoxventum(int id)
        {
            var productoxventum = await _context.Productoxventa.FindAsync(id);
            if (productoxventum == null)
            {
                return NotFound();
            }

            _context.Productoxventa.Remove(productoxventum);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductoxventumExists(int id)
        {
            return _context.Productoxventa.Any(e => e.Id == id);
        }
    }
}
