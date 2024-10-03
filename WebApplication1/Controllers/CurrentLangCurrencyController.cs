using WebApplication1.Models;

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
    public async Task<ActionResult<IEnumerable<CurrentLangCurrency>>> GetCurrentLangCurrencies()
    {
        return await _context.CurrentLangCurrencies.ToListAsync();
    }

    // GET: api/CurrentLangCurrency/5
    [HttpGet("{id}")]
    public async Task<ActionResult<CurrentLangCurrency>> GetCurrentLangCurrency(int id)
    {
        var currentLangCurrency = await _context.CurrentLangCurrencies.FindAsync(id);

        if (currentLangCurrency == null)
        {
            return NotFound();
        }

        return currentLangCurrency;
    }

    // GET: api/CurrentLangCurrency/currency/1
    [HttpGet("currency/{currencyId}")]
    public async Task<ActionResult<IEnumerable<CurrentLangCurrency>>> GetCurrentLangCurrenciesByCurrencyId(
        int currencyId)
    {
        return await _context.CurrentLangCurrencies
            .Where(clc => clc.CurrencyId == currencyId)
            .ToListAsync();
    }

    // GET: api/CurrentLangCurrency/language/1
    [HttpGet("language/{langId}")]
    public async Task<ActionResult<CurrentLangCurrency>> GetCurrentLangCurrencyByLangId(int langId)
    {
        var currentLangCurrency = await _context.CurrentLangCurrencies
            .FirstOrDefaultAsync(clc => clc.LangId == langId);

        if (currentLangCurrency == null)
        {
            return NotFound();
        }

        return currentLangCurrency;
    }

    // POST: api/CurrentLangCurrency
    [HttpPost]
    public async Task<ActionResult<CurrentLangCurrency>> PostCurrentLangCurrency(
        CurrentLangCurrency currentLangCurrency)
    {
        var count = await _context.CurrentLangCurrencies
            .CountAsync(c => c.CurrencyId == currentLangCurrency.CurrencyId);

        if (count >= 2)
        {
            return BadRequest("A currency cannot be associated with more than two languages.");
        }

        if (await _context.CurrentLangCurrencies.AnyAsync(c => c.LangId == currentLangCurrency.LangId))
        {
            return Conflict("This language is already associated with a currency.");
        }

        _context.CurrentLangCurrencies.Add(currentLangCurrency);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCurrentLangCurrency), new { id = currentLangCurrency.Id },
            currentLangCurrency);
    }

    // PUT: api/CurrentLangCurrency/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutCurrentLangCurrency(int id, CurrentLangCurrency currentLangCurrency)
    {
        if (id != currentLangCurrency.Id)
        {
            return BadRequest();
        }

        var existingEntry = await _context.CurrentLangCurrencies.FindAsync(id);
        if (existingEntry == null)
        {
            return NotFound();
        }

        if (existingEntry.CurrencyId != currentLangCurrency.CurrencyId)
        {
            var count = await _context.CurrentLangCurrencies
                .CountAsync(c => c.CurrencyId == currentLangCurrency.CurrencyId);

            if (count >= 2)
            {
                return BadRequest("A currency cannot be associated with more than two languages.");
            }
        }

        if (existingEntry.LangId != currentLangCurrency.LangId)
        {
            if (await _context.CurrentLangCurrencies.AnyAsync(c =>
                    c.LangId == currentLangCurrency.LangId && c.Id != id))
            {
                return Conflict("This language is already associated with a currency.");
            }
        }

        _context.Entry(currentLangCurrency).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!CurrentLangCurrencyExists(id))
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

    // DELETE: api/CurrentLangCurrency/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCurrentLangCurrency(int id)
    {
        var currentLangCurrency = await _context.CurrentLangCurrencies.FindAsync(id);
        if (currentLangCurrency == null)
        {
            return NotFound();
        }

        _context.CurrentLangCurrencies.Remove(currentLangCurrency);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool CurrentLangCurrencyExists(int id)
    {
        return _context.CurrentLangCurrencies.Any(e => e.Id == id);
    }
}