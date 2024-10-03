using Newtonsoft.Json;
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
public class CurrencyController : ControllerBase
{
    private readonly CurrencyDbContext _context;
    private readonly IHttpClientFactory _clientFactory;

    public CurrencyController(CurrencyDbContext context, IHttpClientFactory clientFactory)
    {
        _context = context;
        _clientFactory = clientFactory;
    }

    // GET: api/Currency/UpdateFromCoinDesk
    [HttpGet("UpdateFromCoinDesk")]
    public async Task<IActionResult> UpdateCurrenciesFromCoinDesk()
    {
        try
        {
            var client = _clientFactory.CreateClient();
            var response = await client.GetAsync("https://api.coindesk.com/v1/bpi/currentprice.json");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var coinDeskData = JsonConvert.DeserializeObject<CoinDeskResponse>(content);

                var updatedCurrencies = new List<Currency>();

                foreach (var (code, rate) in coinDeskData.Bpi)
                {
                    var currency = await _context.Currencies.FirstOrDefaultAsync(c => c.Code == code);

                    if (currency == null)
                    {
                        currency = new Currency
                        {
                            Code = code,
                            Description = rate.Description,
                            Rate = rate.RateFloat,
                            Symbol = rate.Symbol,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.Currencies.Add(currency);
                    }
                    else
                    {
                        currency.Description = rate.Description;
                        currency.Rate = rate.RateFloat;
                        currency.Symbol = rate.Symbol;
                        currency.UpdatedAt = DateTime.UtcNow;
                    }

                    updatedCurrencies.Add(currency);
                }

                await _context.SaveChangesAsync();

                return Ok($"Successfully updated {updatedCurrencies.Count} currencies.");
            }
            else
            {
                return StatusCode((int)response.StatusCode, "Failed to fetch data from CoinDesk API.");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }


    // GET: api/Currency
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Currency>>> GetCurrencies()
    {
        return await _context.Currencies.ToListAsync();
    }

    // GET: api/Currency/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Currency>> GetCurrency(int id)
    {
        var currency = await _context.Currencies.FindAsync(id);

        if (currency == null)
        {
            return NotFound();
        }

        return Ok(currency);
    }

    // POST: api/Currency
    [HttpPost]
    public async Task<ActionResult<Currency>> PostCurrency(Currency currency)
    {
        _context.Currencies.Add(currency);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCurrency), new { id = currency.Id }, currency);
    }

    // PUT: api/Currency/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutCurrency(int id, Currency currency)
    {
        if (id != currency.Id)
        {
            return BadRequest();
        }

        _context.Entry(currency).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!CurrencyExists(id))
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

    private bool CurrencyExists(int id)
    {
        return _context.Currencies.Any(e => e.Id == id);
    }
}