using Fidelity.Client.Services;
using Fidelity.Shared.DTOs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Fidelis.Tests;

public class CouponServiceTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly Mock<IMemoryCache> _memoryCacheMock;
    private readonly Mock<ILogger<CouponService>> _loggerMock;
    private readonly CouponService _service;

    public CouponServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost/")
        };
        _memoryCacheMock = new Mock<IMemoryCache>();
        _loggerMock = new Mock<ILogger<CouponService>>();

        // Setup cache to return null (cache miss) by default
        object? cacheEntry = null;
        _memoryCacheMock
            .Setup(m => m.TryGetValue(It.IsAny<object>(), out cacheEntry))
            .Returns(false);

        var cacheEntryMock = new Mock<ICacheEntry>();
        _memoryCacheMock
            .Setup(m => m.CreateEntry(It.IsAny<object>()))
            .Returns(cacheEntryMock.Object);

        _service = new CouponService(_httpClient, _memoryCacheMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetCouponsDisponibiliAsync_ShouldReturnList_WhenApiCallSucceeds()
    {
        // Arrange
        var mockCoupons = new List<CouponDTO>
        {
            new CouponDTO { Id = 1, Descrizione = "Test Coupon" }
        };

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(mockCoupons)
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().EndsWith("api/Coupons/disponibili")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

        // Act
        var result = await _service.GetCouponsDisponibiliAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Test Coupon", result[0].Descrizione);
    }

    [Fact]
    public async Task GetCouponsDisponibiliAsync_ShouldReturnEmptyList_WhenHttpThrowsException()
    {
        // Arrange
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result = await _service.GetCouponsDisponibiliAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        
        // Verify logger was called (Exceptions are logged)
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }
}
