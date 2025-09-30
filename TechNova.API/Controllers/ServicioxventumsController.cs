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
    public class ServicioxventumsController : ControllerBase
    {
        private readonly TechNovaContext _context;

        public ServicioxventumsController(TechNovaContext context)
        {
            _context = context;
        }

        // GET: api/Servicioxventums
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Servicioxventum>>> GetServicioxventa()
        {
            return await _context.Servicioxventa.ToListAsync();
        }

        // GET: api/Servicioxventums/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Servicioxventum>> GetServicioxventum(int id)
        {
            var servicioxventum = await _context.Servicioxventa.FindAsync(id);

            if (servicioxventum == null)
            {
                return NotFound();
            }

            return servicioxventum;
        }

        // PUT: api/Servicioxventums/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutServicioxventum(int id, Servicioxventum servicioxventum)
        {
            if (id != servicioxventum.Id)
            {
                return BadRequest();
            }

            _context.Entry(servicioxventum).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServicioxventumExists(id))
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

        // POST: api/Servicioxventums
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Servicioxventum>> PostServicioxventum(Servicioxventum servicioxventum)
        {
            _context.Servicioxventa.Add(servicioxventum);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetServicioxventum", new { id = servicioxventum.Id }, servicioxventum);
        }

        // DELETE: api/Servicioxventums/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteServicioxventum(int id)
        {
            var servicioxventum = await _context.Servicioxventa.FindAsync(id);
            if (servicioxventum == null)
            {
                return NotFound();
            }

            _context.Servicioxventa.Remove(servicioxventum);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ServicioxventumExists(int id)
        {
            return _context.Servicioxventa.Any(e => e.Id == id);
        }
    }
}
