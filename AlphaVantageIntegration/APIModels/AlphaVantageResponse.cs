namespace AlphaVantageIntegration.APIModels
{
    using System.Text.Json.Serialization;

    public class AlphaVantageResponse
    {
        [JsonPropertyName("Realtime Currency Exchange Rate")]
        public required ExchangeRateData RealtimeCurrencyExchangeRate { get; set; }
    }

    public class ExchangeRateData
    {
        [JsonPropertyName("8. Bid Price")]
        public required string BidPrice { get; set; }

        [JsonPropertyName("9. Ask Price")]
        public required string AskPrice { get; set; }
    }
}
