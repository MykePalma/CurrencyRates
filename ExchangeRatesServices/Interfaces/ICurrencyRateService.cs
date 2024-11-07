using Models;

namespace Services.Interfaces;

public interface ICurrencyRateService
{
    Task<CurrencyRate?> GetCurrencyRateAsync(string currencyPair);
    Task CreateCurrencyRateAsync(CurrencyRate currencyRate);
    Task<bool> UpdateCurrencyRateAsync(int id, CurrencyRate updatedRate);
    Task<bool> DeleteCurrencyRateAsync(int id);
}
