using Models;

namespace Repository.Interfaces;


public interface ICurrencyRateRepository
{
    Task<CurrencyRate?> GetRateByPairAsync(string currencyPair);
    Task<CurrencyRate?> GetRateByIdAsync(int id);
    Task AddRateAsync(CurrencyRate currencyRate);
    Task UpdateRateAsync(CurrencyRate currencyRate);
    Task DeleteRateAsync(int id);
}
