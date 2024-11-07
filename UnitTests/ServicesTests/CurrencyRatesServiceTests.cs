using AlphaVantageIntegration.Interfaces;
using Microsoft.Extensions.Logging;
using Models;
using Moq;
using Repository.Interfaces;
using Services;

namespace UnitTests.ServicesTests;

public class CurrencyRateServiceTests
{
    private readonly Mock<ICurrencyRateRepository> _mockRepository;
    private readonly Mock<IAlphaVantageConnector> _mockAlphaVantageConnector;
    private readonly Mock<ILogger<CurrencyRateService>> _mockLogger;
    private readonly Mock<IRabbitMQService> _mockRabbitMqService;
    private readonly CurrencyRateService _currencyRateService;

    public CurrencyRateServiceTests()
    {
        _mockRepository = new Mock<ICurrencyRateRepository>();
        _mockAlphaVantageConnector = new Mock<IAlphaVantageConnector>();
        _mockLogger = new Mock<ILogger<CurrencyRateService>>();
        _mockRabbitMqService = new Mock<IRabbitMQService>();
        _currencyRateService = new CurrencyRateService(
            _mockRepository.Object,
            _mockAlphaVantageConnector.Object,
            _mockRabbitMqService.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task GetCurrencyRateAsync_ReturnsRateFromDatabase_WhenRateExists()
    {
        // Arrange
        var currencyPair = "USD/EUR";
        var expectedRate = new CurrencyRate
        {
            Id = 1,
            CurrencyPair = currencyPair,
            Bid = 1.1234m,
            Ask = 1.2345m,
            LastUpdated = DateTime.UtcNow
        };

        _mockRepository.Setup(repo => repo.GetRateByPairAsync(currencyPair))
            .ReturnsAsync(expectedRate);

        // Act
        var result = await _currencyRateService.GetCurrencyRateAsync(currencyPair);

        // Assert
        Assert.Equal(expectedRate, result);
        _mockRepository.Verify(repo => repo.GetRateByPairAsync(currencyPair), Times.Once);
        _mockAlphaVantageConnector.Verify(connector => connector.FetchRateFromApiAsync(It.IsAny<string>()), Times.Never);
        _mockRabbitMqService.Verify(rabbit => rabbit.SendMessageAsync(It.Is<string>(msg => msg.Contains($"Currency rate for {currencyPair}"))), Times.Once);
    }

    [Fact]
    public async Task GetCurrencyRateAsync_FetchesFromApi_WhenRateNotInDatabase()
    {
        // Arrange
        var currencyPair = "USD/EUR";
        var apiRate = new CurrencyRate
        {
            CurrencyPair = currencyPair,
            Bid = 1.1234m,
            Ask = 1.2345m,
            LastUpdated = DateTime.UtcNow
        };

        _mockRepository.Setup(repo => repo.GetRateByPairAsync(currencyPair)).ReturnsAsync((CurrencyRate)null);
        _mockAlphaVantageConnector.Setup(connector => connector.FetchRateFromApiAsync(currencyPair)).ReturnsAsync(apiRate);

        // Act
        var result = await _currencyRateService.GetCurrencyRateAsync(currencyPair);

        // Assert
        Assert.Equal(apiRate, result);
        _mockRepository.Verify(repo => repo.AddRateAsync(apiRate), Times.Once);
        _mockAlphaVantageConnector.Verify(connector => connector.FetchRateFromApiAsync(currencyPair), Times.Once);
        _mockRabbitMqService.Verify(rabbit => rabbit.SendMessageAsync(It.Is<string>(msg => msg.Contains($"Currency rate for {currencyPair}"))), Times.Once);
    }

    [Fact]
    public async Task CreateCurrencyRateAsync_CallsRepositoryAdd()
    {
        // Arrange
        var newRate = new CurrencyRate
        {
            CurrencyPair = "USD/JPY",
            Bid = 110.123m,
            Ask = 110.456m,
            LastUpdated = DateTime.UtcNow
        };

        // Act
        await _currencyRateService.CreateCurrencyRateAsync(newRate);

        // Assert
        _mockRepository.Verify(repo => repo.AddRateAsync(newRate), Times.Once);
        _mockRabbitMqService.Verify(rabbit => rabbit.SendMessageAsync(It.Is<string>(msg => msg.Contains($"Currency rate for {newRate.CurrencyPair}"))), Times.Once);
    }

    [Fact]
    public async Task UpdateCurrencyRateAsync_ReturnsTrue_WhenRateExists()
    {
        // Arrange
        var existingRate = new CurrencyRate
        {
            Id = 1,
            CurrencyPair = "USD/GBP",
            Bid = 1.5678m,
            Ask = 1.6789m,
            LastUpdated = DateTime.UtcNow
        };

        var updatedRate = new CurrencyRate
        {
            Id = 1,
            CurrencyPair = "USD/GBP",
            Bid = 1.1234m,
            Ask = 1.2345m,
            LastUpdated = DateTime.UtcNow
        };

        _mockRepository.Setup(repo => repo.GetRateByIdAsync(existingRate.Id)).ReturnsAsync(existingRate);

        // Act
        var result = await _currencyRateService.UpdateCurrencyRateAsync(existingRate.Id, updatedRate);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(repo => repo.UpdateRateAsync(It.Is<CurrencyRate>(r => r.Bid == updatedRate.Bid && r.Ask == updatedRate.Ask)), Times.Once);
        _mockRabbitMqService.Verify(rabbit => rabbit.SendMessageAsync(It.Is<string>(msg => msg.Contains($"Currency rate for {updatedRate.CurrencyPair}"))), Times.Once);
    }

    [Fact]
    public async Task DeleteCurrencyRateAsync_ReturnsFalse_WhenRateNotFound()
    {
        // Arrange
        int id = 1;
        _mockRepository.Setup(repo => repo.GetRateByIdAsync(id)).ReturnsAsync((CurrencyRate)null);

        // Act
        var result = await _currencyRateService.DeleteCurrencyRateAsync(id);

        // Assert
        Assert.False(result);
        _mockRepository.Verify(repo => repo.DeleteRateAsync(It.IsAny<int>()), Times.Never);
        _mockRabbitMqService.Verify(rabbit => rabbit.SendMessageAsync(It.Is<string>(msg => msg.Contains($"Currency rate for null"))), Times.Never);
    }

    [Fact]
    public async Task DeleteCurrencyRateAsync_ReturnsTrue_WhenRateFound()
    {
        // Arrange
        int id = 1;
        CurrencyRate deletedRate = new CurrencyRate
        {
            Id = 1,
            CurrencyPair = "USD/GBP",
            Bid = 1.1234m,
            Ask = 1.2345m,
            LastUpdated = DateTime.UtcNow
        };

        _mockRepository.Setup(repo => repo.GetRateByIdAsync(id)).ReturnsAsync(deletedRate);

        // Act
        var result = await _currencyRateService.DeleteCurrencyRateAsync(id);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(repo => repo.DeleteRateAsync(It.IsAny<int>()), Times.Once);
        _mockRabbitMqService.Verify(rabbit => rabbit.SendMessageAsync(It.Is<string>(msg => msg.Contains($"Currency rate for {deletedRate.CurrencyPair}"))), Times.Once);
    }
}
