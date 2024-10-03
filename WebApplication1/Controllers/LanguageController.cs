using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using WebApplication1.Models.Dto;

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
        public async Task<ActionResult<IEnumerable<LanguageDto>>> GetLanguages()
        {
            var languages = await _context.Languages.ToListAsync();

            // combined
            var combinedData = languages.Select(c => new LanguageDto
            {
                Id = c.Id,
                LangCode = c.LangCode,
                LangName = c.LangName,
                UpdatedAt = c.UpdatedAt
            });

            return Ok(combinedData);
        }

        // GET: api/Language/5
        [NonAction]
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
        [NonAction]
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

        [HttpPost]
        public async Task<IActionResult> PostLanguage(string langCode, string langName)
        {
            if (string.IsNullOrWhiteSpace(langCode))
            {
                return BadRequest("Language code cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(langName))
            {
                return BadRequest("Language name cannot be empty.");
            }

            // 檢查 LangCode 是否已存在
            if (await _context.Languages.AnyAsync(l => l.LangCode == langCode))
            {
                return Conflict("A language with this code already exists.");
            }

            var language = new Language
            {
                LangCode = langCode,
                LangName = langName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Languages.Add(language);
            await _context.SaveChangesAsync();

            return StatusCode(201, new { message = "Language created successfully" });
        }

        // PUT: api/Language/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLanguage(int id, string langName)
        {
            if (string.IsNullOrWhiteSpace(langName))
            {
                return BadRequest("Language name cannot be empty.");
            }

            var language = await _context.Languages.FindAsync(id);
            if (language == null)
            {
                return NotFound();
            }

            language.LangName = langName;
            language.UpdatedAt = DateTime.UtcNow;

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

            try
            {
                _context.Languages.Remove(language);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Check if the deletion failed due to foreign key constraint
                if (ex.InnerException?.Message.Contains(
                        "The DELETE statement conflicted with the REFERENCE constraint") == true)
                {
                    return BadRequest("Cannot delete the language because it is referenced by other entities.");
                }
                else
                {
                    // For other types of DbUpdateException, we can choose to re-throw or return a generic error
                    return StatusCode(500, "An error occurred while deleting the language.");
                }
            }

            return NoContent();
        }

        private bool LanguageExists(int id)
        {
            return _context.Languages.Any(e => e.Id == id);
        }
    }
}