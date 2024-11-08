using Microsoft.EntityFrameworkCore;
using Models;
using Repository.Interfaces;


namespace Repository;

public class CurrencyRateRepository : ICurrencyRateRepository
{
    private readonly CurrencyRatesDbContext _context;

    public CurrencyRateRepository(CurrencyRatesDbContext context)
    {
        _context = context;
    }

    public async Task<CurrencyRate?> GetRateByPairAsync(string currencyPair)
    {
        return await _context.Set<CurrencyRate>()
                             .FirstOrDefaultAsync(rate => rate.CurrencyPair == currencyPair);
    }

    public async Task<CurrencyRate?> GetRateByIdAsync(int id)
    {
        return await _context.Set<CurrencyRate>()
                             .FirstOrDefaultAsync(rate => rate.Id == id);
    }

    public async Task AddRateAsync(CurrencyRate currencyRate)
    {
        _context.Set<CurrencyRate>().Add(currencyRate);

        await _context.SaveChangesAsync();
    }

    public async Task UpdateRateAsync(CurrencyRate currencyRate)
    {
        _context.Entry(currencyRate).State = EntityState.Modified;

        // Save changes to the database
        await _context.SaveChangesAsync();
    }

    public async Task DeleteRateAsync(int id)
    {
        var currencyRate = await GetRateByIdAsync(id);
        if (currencyRate != null)
        {
            _context.Set<CurrencyRate>().Remove(currencyRate);
            await _context.SaveChangesAsync();
        }
    }
}
