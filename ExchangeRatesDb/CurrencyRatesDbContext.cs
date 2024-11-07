using Microsoft.EntityFrameworkCore;
using Models;

namespace Repository;

public class CurrencyRatesDbContext : DbContext
{
    public CurrencyRatesDbContext(DbContextOptions options) : base(options) { }

    public DbSet<CurrencyRate> CurrencyRates { get; set; }
}