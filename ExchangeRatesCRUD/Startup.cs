using AlphaVantageIntegration;
using AlphaVantageIntegration.Interfaces;
using Microsoft.EntityFrameworkCore;
using Repository;
using Repository.Interfaces;
using Services;
using Services.Interfaces;

namespace CurrencyRateAPI;

public class Startup(IConfiguration configuration)
{
    private readonly IConfiguration _configuration = configuration;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddHttpClient();

        //Repository
        services.AddDbContext<CurrencyRatesDbContext>(options =>
                    options.UseSqlServer(_configuration.GetConnectionString("sqlServerConnection")));

        services.AddScoped<ICurrencyRateRepository, CurrencyRateRepository>();

        //Services
        services.AddScoped<ICurrencyRateService, CurrencyRateService>();
        services.AddScoped<IAlphaVantageConnector, AlphaVantageConnector>();
        services.AddScoped<IRabbitMQService, RabbitMQService>();
    }
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
