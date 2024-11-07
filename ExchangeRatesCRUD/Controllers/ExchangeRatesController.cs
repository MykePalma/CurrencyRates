using Microsoft.AspNetCore.Mvc;
using Models;
using Services.Interfaces;

namespace CurrencyRateAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CurrencyRatesController : ControllerBase
{
    private readonly ICurrencyRateService _currencyRateService;

    public CurrencyRatesController(ICurrencyRateService currencyRateService)
    {
        _currencyRateService = currencyRateService;
    }

    // GET: api/CurrencyRates/{currencyPair}
    [HttpGet("{currencyPair}")]
    public async Task<IActionResult> GetCurrencyRate(string currencyPair)
    {
        currencyPair = Uri.UnescapeDataString(currencyPair);
        var rate = await _currencyRateService.GetCurrencyRateAsync(currencyPair);
        if (rate == null)
            return NotFound("Rate not available.");
        return Ok(rate);
    }

    // POST: api/CurrencyRates
    [HttpPost]
    public async Task<IActionResult> CreateCurrencyRate([FromBody] CurrencyRate currencyRate)
    {
        if (currencyRate == null)
            return BadRequest("Invalid data.");

        await _currencyRateService.CreateCurrencyRateAsync(currencyRate);
        return CreatedAtAction(nameof(GetCurrencyRate), new { currencyPair = currencyRate.CurrencyPair }, currencyRate);
    }

    // PUT: api/CurrencyRates/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCurrencyRate(int id, [FromBody] CurrencyRate updatedRate)
    {
        if (updatedRate == null || updatedRate.Id != id)
            return BadRequest("Invalid data.");

        var result = await _currencyRateService.UpdateCurrencyRateAsync(id, updatedRate);
        if (!result)
            return NotFound();

        return NoContent();
    }

    // DELETE: api/CurrencyRates/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCurrencyRate(int id)
    {
        var result = await _currencyRateService.DeleteCurrencyRateAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }
}
