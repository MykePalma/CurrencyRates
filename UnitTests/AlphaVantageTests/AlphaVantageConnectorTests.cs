using AlphaVantageIntegration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;

namespace UnitTests.AlphaVantageTests;

public class AlphaVantageConnectorTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<AlphaVantageConnector>> _mockLogger;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;

    public AlphaVantageConnectorTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<AlphaVantageConnector>>();
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
    }

    [Fact]
    public async Task FetchRateFromApiAsync_ReturnsCurrencyRate_WhenApiResponseIsValid()
    {
        // Arrange
        var expectedResponse = "{ \"Realtime Currency Exchange Rate\": { \"8. Bid Price\": \"1.1234\", \"9. Ask Price\": \"1.2345\" }}";
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(expectedResponse)
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);

        // Set up the HttpClientFactory to return the mocked HttpClient
        _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

        // Set up the configuration mock
        _mockConfiguration.Setup(x => x["AlphaVantage:ApiKey"]).Returns("test_api_key");

        // Initialize the AlphaVantageConnector
        var connector = new AlphaVantageConnector(_mockHttpClientFactory.Object, _mockConfiguration.Object, _mockLogger.Object);

        // Act
        var result = await connector.FetchRateFromApiAsync("USD/EUR");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("USD/EUR", result.CurrencyPair);
        Assert.Equal(1.1234m, result.Bid);
        Assert.Equal(1.2345m, result.Ask);
    }

    [Fact]
    public async Task FetchRateFromApiAsync_ReturnsNull_WhenApiResponseIsMalformed()
    {
        // Arrange
        var malformedResponse = "{ \"Realtime Currency Exchange Rate\": { \"8. Bid Price\": \"not_a_number\" }}";
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(malformedResponse)
            });
        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);
        _mockConfiguration.Setup(x => x["AlphaVantage:ApiKey"]).Returns("test_api_key");

        var connector = new AlphaVantageConnector(_mockHttpClientFactory.Object, _mockConfiguration.Object, _mockLogger.Object);

        // Act
        var result = await connector.FetchRateFromApiAsync("USD/EUR");

        // Assert
        Assert.Null(result);
        Assert.Null(result);
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An error occurred while fetching the currency rate")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }


    [Fact]
    public async Task FetchRateFromApiAsync_ReturnsNull_WhenApiKeyIsMissing()
    {
        // Arrange
        _mockConfiguration.Setup(x => x["AlphaVantage:ApiKey"]).Returns((string)null);

        var connector = new AlphaVantageConnector(_mockHttpClientFactory.Object, _mockConfiguration.Object, _mockLogger.Object);

        // Act
        var result = await connector.FetchRateFromApiAsync("USD/EUR");

        // Assert
        Assert.Null(result);
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Alpha Vantage API key is not configured.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task FetchRateFromApiAsync_ReturnsNull_WhenApiResponseFails()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });
        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

        _mockConfiguration.Setup(x => x["AlphaVantage:ApiKey"]).Returns("test_api_key");

        var connector = new AlphaVantageConnector(_mockHttpClientFactory.Object, _mockConfiguration.Object, _mockLogger.Object);

        // Act
        var result = await connector.FetchRateFromApiAsync("USD/EUR");

        // Assert
        Assert.Null(result);
        Assert.Null(result);
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to fetch currency rate from API. Status Code")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task FetchRateFromApiAsync_ReturnsNull_WhenNetworkFails()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));
        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);
        _mockConfiguration.Setup(x => x["AlphaVantage:ApiKey"]).Returns("test_api_key");

        var connector = new AlphaVantageConnector(_mockHttpClientFactory.Object, _mockConfiguration.Object, _mockLogger.Object);

        // Act
        var result = await connector.FetchRateFromApiAsync("USD/EUR");

        // Assert
        Assert.Null(result);
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An error occurred while fetching the currency rate")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
