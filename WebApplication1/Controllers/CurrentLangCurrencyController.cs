using WebApplication1.Models;
using WebApplication1.Models.Dto;

namespace WebApplication1.Controllers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class CurrentLangCurrencyController : ControllerBase
{
    private readonly CurrencyDbContext _context;

    public CurrentLangCurrencyController(CurrencyDbContext context)
    {
        _context = context;
    }

    // GET: api/CurrentLangCurrency
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CurrentLangCurrencyDto>>> GetCurrentLangCurrencies()
    {
        var currentLangCurrencies = await _context.CurrentLangCurrencies
            .Select(clc => new CurrentLangCurrencyDto
            {
                CurrentLang = clc.CurrentLang,
                LangId = clc.LangId,
                CurrencyId = clc.CurrencyId,
                LangTitle = clc.LangTitle,
                UpdatedAt = clc.UpdatedAt
            })
            .ToListAsync();

        return Ok(currentLangCurrencies);
    }

    // GET: api/CurrentLangCurrency/5
    [HttpGet("{id}")]
    public async Task<ActionResult<IEnumerable<CurrentLangCurrencyDto>>> GetCurrentLangCurrency(int id)
    {
        var currentLangCurrencies = await _context.CurrentLangCurrencies
            .Where(clc => clc.Id == id)
            .Select(clc => new CurrentLangCurrencyDto
            {
                CurrentLang = clc.CurrentLang,
                LangId = clc.LangId,
                CurrencyId = clc.CurrencyId,
                LangTitle = clc.LangTitle,
                UpdatedAt = clc.UpdatedAt
            })
            .ToListAsync();

        if (!currentLangCurrencies.Any())
        {
            return NotFound();
        }

        return Ok(currentLangCurrencies);
    }

    // GET: api/CurrentLangCurrency/{cid}/{lanid}
    [HttpGet("{cid}/{lanid}")]
    public async Task<ActionResult<CurrentLangCurrencyDto>> GetCurrentLangCurrency(int cid, int lanid)
    {
        var currentLangCurrency = await _context.CurrentLangCurrencies
            .FirstOrDefaultAsync(clc => clc.LangId == lanid && clc.CurrencyId == cid);

        if (currentLangCurrency == null)
        {
            return NotFound();
        }

        var dto = new CurrentLangCurrencyDto
        {
            CurrentLang = currentLangCurrency.CurrentLang,
            LangId = currentLangCurrency.LangId,
            CurrencyId = currentLangCurrency.CurrencyId,
            LangTitle = currentLangCurrency.LangTitle,
            UpdatedAt = currentLangCurrency.UpdatedAt
        };

        return Ok(dto);
    }


    // POST: api/CurrentLangCurrency/{cid}/{lanid}
    [HttpPost("{cid}/{lanid}")]
    public async Task<ActionResult<CurrentLangCurrencyDto>> PostCurrentLangCurrency(int cid, int lanid, string langName)
    {
        // Check if Currency exists
        var currency = await _context.Currencies.FindAsync(cid);
        if (currency == null)
        {
            return BadRequest($"Currency with id {cid} does not exist.");
        }

        // Check if Language exists
        var language = await _context.Languages.FindAsync(lanid);
        if (language == null)
        {
            return BadRequest($"Language with id {lanid} does not exist.");
        }

        // Check if a CurrentLangCurrency already exists for this currency and language combination
        var existingEntry = await _context.CurrentLangCurrencies
            .FirstOrDefaultAsync(clc => clc.CurrencyId == cid && clc.LangId == lanid);

        CurrentLangCurrency entry;

        if (existingEntry != null)
        {
            // Update existing entry
            existingEntry.LangTitle = langName;
            existingEntry.UpdatedAt = DateTime.UtcNow;
            entry = existingEntry;
        }
        else
        {
            // Create new entry
            entry = new CurrentLangCurrency
            {
                CurrentLang = language.LangCode,
                LangId = lanid,
                CurrencyId = cid,
                LangTitle = langName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.CurrentLangCurrencies.Add(entry);
        }

        try
        {
            await _context.SaveChangesAsync();

            var dto = new CurrentLangCurrencyDto
            {
                CurrentLang = entry.CurrentLang,
                LangId = entry.LangId,
                CurrencyId = entry.CurrencyId,
                LangTitle = entry.LangTitle,
                UpdatedAt = entry.UpdatedAt
            };

            return Ok(dto);
        }
        catch (DbUpdateException)
        {
            return StatusCode(500, "An error occurred while saving the entry. Please try again.");
        }
    }
}