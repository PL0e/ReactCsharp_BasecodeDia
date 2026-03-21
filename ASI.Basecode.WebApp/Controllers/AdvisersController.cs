using ASI.Basecode.Data;
using ASI.Basecode.Data.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AdvisersController : ControllerBase
    {
        private readonly AsiBasecodeDBContext _context;

        public AdvisersController(AsiBasecodeDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Adviser>>> GetAll()
        {
            return Ok(await _context.Advisers.AsNoTracking().ToListAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Adviser>> GetById(int id)
        {
            var adviser = await _context.Advisers.FindAsync(id);
            if (adviser == null) return NotFound();
            return Ok(adviser);
        }

        [HttpPost]
        public async Task<ActionResult<Adviser>> Create([FromBody] Adviser adviser)
        {
            _context.Advisers.Add(adviser);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = adviser.Id }, adviser);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Adviser adviser)
        {
            if (id != adviser.Id) return BadRequest();
            if (!await _context.Advisers.AnyAsync(x => x.Id == id)) return NotFound();

            _context.Entry(adviser).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var adviser = await _context.Advisers.FindAsync(id);
            if (adviser == null) return NotFound();

            _context.Advisers.Remove(adviser);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
