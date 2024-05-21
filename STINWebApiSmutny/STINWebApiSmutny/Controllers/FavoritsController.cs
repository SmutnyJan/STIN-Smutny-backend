using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using STINWebApiSmutny.Models;

namespace STINWebApiSmutny.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FavoritsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FavoritsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Favorits
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Favorit>>> GetFavorites()
        {
            return await _context.Favorites.ToListAsync();
        }

        // GET: api/Favorits/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Favorit>> GetFavorit(int id)
        {
            var favorit = await _context.Favorites.FindAsync(id);

            if (favorit == null)
            {
                return NotFound();
            }

            return favorit;
        }

        // GET: api/Favorits/5/London
        [HttpGet("{user_id}/{location}")]
        public async Task<ActionResult<bool>> GetFavorit(int user_id, string location)
        {
            var favorit = await _context.Favorites.Where(x => x.Users_id == user_id && x.city == location).FirstOrDefaultAsync();

            if (favorit == null)
            {
                return false;
            }

            return true;
        }

        // GET: api/Favorits/User/5
        [HttpGet("User/{user_id}")]
        public async Task<ActionResult<List<Favorit>>> GetUserFavorites(int user_id)
        {
            var favorites = await _context.Favorites.Where(x => x.Users_id == user_id).ToListAsync();


            return favorites;
        }

        // PUT: api/Favorits/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFavorit(int id, Favorit favorit)
        {
            if (id != favorit.id)
            {
                return BadRequest();
            }

            _context.Entry(favorit).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FavoritExists(id))
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

        // POST: api/Favorits
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Favorit>> PostFavorit(Favorit favorit)
        {
            if (_context.Favorites.Where(x => x.Users_id == favorit.Users_id && x.city == favorit.city).Any())
            {
                return BadRequest("Already added in favourites!");
            }
            _context.Favorites.Add(favorit);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetFavorit", new { id = favorit.id }, favorit);
        }

        // DELETE: api/Favorits/5
        [HttpDelete("{user_id}/{location}")]
        public async Task<IActionResult> DeleteFavorit(int user_id, string location)
        {
            var favorit_to_delete = await _context.Favorites.Where(x => x.Users_id == user_id && x.city == location).FirstAsync();
            if (favorit_to_delete == null)
            {
                return NotFound();
            }

            _context.Favorites.Remove(favorit_to_delete);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool FavoritExists(int id)
        {
            return _context.Favorites.Any(e => e.id == id);
        }
    }
}