using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LanguageController : ControllerBase
    {
        private readonly CurrencyDbContext _context;

        public LanguageController(CurrencyDbContext context)
        {
            _context = context;
        }

        // GET: api/Language
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Language>>> GetLanguages()
        {
            return await _context.Languages.ToListAsync();
        }

        // GET: api/Language/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Language>> GetLanguage(int id)
        {
            var language = await _context.Languages.FindAsync(id);

            if (language == null)
            {
                return NotFound();
            }

            return language;
        }

        // GET: api/Language/code/en-US
        [HttpGet("code/{langCode}")]
        public async Task<ActionResult<Language>> GetLanguageByCode(string langCode)
        {
            var language = await _context.Languages.FirstOrDefaultAsync(l => l.LangCode == langCode);

            if (language == null)
            {
                return NotFound();
            }

            return language;
        }

        // POST: api/Language
        [HttpPost]
        public async Task<ActionResult<Language>> PostLanguage(Language language)
        {
            if (await _context.Languages.AnyAsync(l => l.LangCode == language.LangCode))
            {
                return Conflict("A language with this code already exists.");
            }

            _context.Languages.Add(language);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetLanguage), new { id = language.Id }, language);
        }

        // PUT: api/Language/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLanguage(int id, Language language)
        {
            if (id != language.Id)
            {
                return BadRequest();
            }

            if (await _context.Languages.AnyAsync(l => l.LangCode == language.LangCode && l.Id != id))
            {
                return Conflict("Another language with this code already exists.");
            }

            _context.Entry(language).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LanguageExists(id))
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

        // DELETE: api/Language/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLanguage(int id)
        {
            var language = await _context.Languages.FindAsync(id);
            if (language == null)
            {
                return NotFound();
            }

            _context.Languages.Remove(language);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LanguageExists(int id)
        {
            return _context.Languages.Any(e => e.Id == id);
        }
    }
}