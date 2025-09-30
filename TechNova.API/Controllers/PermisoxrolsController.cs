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
    public class PermisoxrolsController : ControllerBase
    {
        private readonly TechNovaContext _context;

        public PermisoxrolsController(TechNovaContext context)
        {
            _context = context;
        }

        // GET: api/Permisoxrols
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Permisoxrol>>> GetPermisoxrols()
        {
            return await _context.Permisoxrols.ToListAsync();
        }

        // GET: api/Permisoxrols/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Permisoxrol>> GetPermisoxrol(int id)
        {
            var permisoxrol = await _context.Permisoxrols.FindAsync(id);

            if (permisoxrol == null)
            {
                return NotFound();
            }

            return permisoxrol;
        }

        // PUT: api/Permisoxrols/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPermisoxrol(int id, Permisoxrol permisoxrol)
        {
            if (id != permisoxrol.IdPermisoRol)
            {
                return BadRequest();
            }

            _context.Entry(permisoxrol).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PermisoxrolExists(id))
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

        // POST: api/Permisoxrols
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Permisoxrol>> PostPermisoxrol(Permisoxrol permisoxrol)
        {
            _context.Permisoxrols.Add(permisoxrol);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPermisoxrol", new { id = permisoxrol.IdPermisoRol }, permisoxrol);
        }

        // DELETE: api/Permisoxrols/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePermisoxrol(int id)
        {
            var permisoxrol = await _context.Permisoxrols.FindAsync(id);
            if (permisoxrol == null)
            {
                return NotFound();
            }

            _context.Permisoxrols.Remove(permisoxrol);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PermisoxrolExists(int id)
        {
            return _context.Permisoxrols.Any(e => e.IdPermisoRol == id);
        }
    }
}
