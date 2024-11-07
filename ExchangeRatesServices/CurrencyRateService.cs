using AlphaVantageIntegration.Interfaces;
using Microsoft.Extensions.Logging;
using Models;
using Repository.Interfaces;
using Services;
using Services.Interfaces;

public class CurrencyRateService : ICurrencyRateService
{
    private readonly ICurrencyRateRepository _currencyRateRepository;
    private readonly IAlphaVantageConnector _alphaVantageConnector;
    private readonly ILogger<CurrencyRateService> _logger;
    private readonly IRabbitMQService _rabbitMQService;

    public CurrencyRateService(
        ICurrencyRateRepository currencyRateRepository,
        IAlphaVantageConnector alphaVantageConnector,
        IRabbitMQService rabbitMQService,
        ILogger<CurrencyRateService> logger)
    {
        _currencyRateRepository = currencyRateRepository;
        _alphaVantageConnector = alphaVantageConnector;
        _rabbitMQService = rabbitMQService;
        _logger = logger;
    }

    public async Task<CurrencyRate?> GetCurrencyRateAsync(string currencyPair)
    {
        _logger.LogInformation("Attempting to retrieve currency rate for {CurrencyPair} from the database.", currencyPair);

        // Check if the rate exists in the database
        var rate = await _currencyRateRepository.GetRateByPairAsync(currencyPair);
        if (rate != null)
        {
            _logger.LogInformation("Currency rate for {CurrencyPair} found in database.", currencyPair);
        }
        else
        {
            _logger.LogInformation("Currency rate for {CurrencyPair} not found in database. Fetching from external API.", currencyPair);
            rate = await _alphaVantageConnector.FetchRateFromApiAsync(currencyPair);

            if (rate != null)
            {
                _logger.LogInformation("Currency rate for {CurrencyPair} retrieved from API. Saving to database.", currencyPair);
                await _currencyRateRepository.AddRateAsync(rate);
            }
            else
            {
                _logger.LogWarning("Currency rate for {CurrencyPair} could not be retrieved from the external API.", currencyPair);
            }
        }

        await _rabbitMQService.SendMessageAsync($"Currency rate for {currencyPair} - ${rate}.");
        _logger.LogInformation("Currency rate for {CurrencyPair} sent to messaging queue.", currencyPair);

        return rate;
    }

    public async Task CreateCurrencyRateAsync(CurrencyRate currencyRate)
    {
        _logger.LogInformation("Adding a new currency rate for {CurrencyPair} to the database.", currencyRate.CurrencyPair);
        await _currencyRateRepository.AddRateAsync(currencyRate);

        await _rabbitMQService.SendMessageAsync($"Currency rate for {currencyRate.CurrencyPair} created.");
        _logger.LogInformation("Currency rate for {CurrencyPair} sent to messaging queue.", currencyRate.CurrencyPair);
    }

    public async Task<bool> UpdateCurrencyRateAsync(int id, CurrencyRate updatedRate)
    {
        _logger.LogInformation("Attempting to update currency rate with ID {Id}.", id);

        var existingRate = await _currencyRateRepository.GetRateByIdAsync(id);
        if (existingRate == null)
        {
            _logger.LogWarning("Currency rate with ID {Id} not found.", id);
            return false;
        }

        existingRate.Bid = updatedRate.Bid;
        existingRate.Ask = updatedRate.Ask;
        existingRate.LastUpdated = updatedRate.LastUpdated;

        await _currencyRateRepository.UpdateRateAsync(existingRate);
        _logger.LogInformation("Currency rate with ID {Id} successfully updated.", id);

        await _rabbitMQService.SendMessageAsync($"Currency rate for {updatedRate.CurrencyPair} updated.");
        _logger.LogInformation("Currency rate for {CurrencyPair} sent to messaging queue.", updatedRate.CurrencyPair);

        return true;
    }

    public async Task<bool> DeleteCurrencyRateAsync(int id)
    {
        _logger.LogInformation("Attempting to delete currency rate with ID {Id}.", id);

        var rate = await _currencyRateRepository.GetRateByIdAsync(id);
        if (rate == null)
        {
            _logger.LogWarning("Currency rate with ID {Id} not found for deletion.", id);
            return false;
        }

        await _currencyRateRepository.DeleteRateAsync(id);
        _logger.LogInformation("Currency rate with ID {Id} successfully deleted.", id);

        await _rabbitMQService.SendMessageAsync($"Currency rate for {rate.CurrencyPair} deleted.");
        _logger.LogInformation("Currency rate for {CurrencyPair} sent to messaging queue.", rate.CurrencyPair);

        return true;
    }
}
