namespace Models;

public class CurrencyRate
{
    public int Id { get; set; }
    public required string CurrencyPair { get; set; }
    public decimal Bid { get; set; }
    public decimal Ask { get; set; }
    public DateTime LastUpdated { get; set; }
}
