using Models;

namespace AlphaVantageIntegration.Interfaces;

public interface IAlphaVantageConnector
{
    public Task<CurrencyRate?> FetchRateFromApiAsync(string currencyPair);
}
