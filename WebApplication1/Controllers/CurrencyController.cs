using Newtonsoft.Json;
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
public class CurrencyController : ControllerBase
{
    private readonly CurrencyDbContext _context;
    private readonly IHttpClientFactory _clientFactory;

    public CurrencyController(CurrencyDbContext context, IHttpClientFactory clientFactory)
    {
        _context = context;
        _clientFactory = clientFactory;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CurrencyInfoDto>>> GetCombinedCurrencies()
    {
        var currencies = await _context.Currencies.ToListAsync();

        // get all lang info
        var currentLangCurrencies = await _context.CurrentLangCurrencies.ToListAsync();

        // combined
        var combinedData = currencies.Select(c => new CurrencyInfoDto
        {
            Id = c.Id,
            Code = c.Code,
            Symbol = c.Symbol,
            Description = c.Description,
            Rate = c.Rate,
            RateFloat = c.RateFloat,
            LangTitle = currentLangCurrencies
                .FirstOrDefault(clc => clc.CurrencyId == c.Id && clc.CurrentLang == "zh-tw")?.LangTitle,
            UpdatedAt = c.UpdatedAt
        });

        return Ok(combinedData.OrderBy(c => c.Code));
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
                        currency.Rate = rate.Rate;
                        currency.RateFloat = rate.RateFloat;
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

    // GET: api/Currency/1
    [HttpGet("{id}")]
    public async Task<ActionResult<CurrencyInfoDto>> GetCurrency(int id)
    {
        var currency = await _context.Currencies.FindAsync(id);

        if (currency == null)
        {
            return NotFound();
        }

        // get all lang info
        var currentLangCurrencies = await _context.CurrentLangCurrencies.ToListAsync();

        return Ok(new CurrencyInfoDto
        {
            Id = currency.Id,
            Code = currency.Code,
            Symbol = currency.Symbol,
            Description = currency.Description,
            Rate = currency.Rate,
            RateFloat = currency.RateFloat,
            LangTitle = currentLangCurrencies
                .FirstOrDefault(clc => clc.CurrencyId == currency.Id && clc.CurrentLang == "zh-tw")?.LangTitle,
            UpdatedAt = currency.UpdatedAt
        });
    }

    // POST: api/Currency
    [NonAction]
    [HttpPost]
    public async Task<ActionResult<Currency>> PostCurrency(Currency currency)
    {
        _context.Currencies.Add(currency);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCurrency), new { id = currency.Id }, currency);
    }

    // PUT: api/Currency/5
    [NonAction]
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