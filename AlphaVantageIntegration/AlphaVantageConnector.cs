using AlphaVantageIntegration.APIModels;
using AlphaVantageIntegration.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Models;
using System.Text.Json;

namespace AlphaVantageIntegration;

public class AlphaVantageConnector : IAlphaVantageConnector
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AlphaVantageConnector> _logger;

    public AlphaVantageConnector(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<AlphaVantageConnector> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<CurrencyRate?> FetchRateFromApiAsync(string currencyPair)
    {
        try
        {
            // Retrieve API key from configuration
            var apiKey = _configuration["AlphaVantage:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogError("Alpha Vantage API key is not configured.");
                return null;
            }

            // Format the request URL
            var url = $"https://www.alphavantage.co/query?function=CURRENCY_EXCHANGE_RATE&from_currency={currencyPair.Substring(0, 3)}&to_currency={currencyPair.Substring(4, 3)}&apikey={apiKey}";

            // Send request to Alpha Vantage API
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to fetch currency rate from API. Status Code: {response.StatusCode}");
                return null;
            }

            // Parse the JSON response
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<AlphaVantageResponse>(jsonResponse);

            if (apiResponse?.RealtimeCurrencyExchangeRate == null)
            {
                _logger.LogError("API response is missing expected data.");
                return null;
            }

            return new CurrencyRate
            {
                CurrencyPair = currencyPair,
                Bid = Convert.ToDecimal(apiResponse.RealtimeCurrencyExchangeRate.BidPrice),
                Ask = Convert.ToDecimal(apiResponse.RealtimeCurrencyExchangeRate.AskPrice),
                LastUpdated = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"An error occurred while fetching the currency rate: {ex.Message}");
            return null;
        }
    }
}