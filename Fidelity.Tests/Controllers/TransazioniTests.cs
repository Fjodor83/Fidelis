using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Fidelity.Server.Controllers;
using Fidelity.Server.Services;
using Fidelity.Shared.DTOs;

namespace Fidelity.Tests.Controllers
{
    public class TransazioniTests
    {
        private readonly Mock<ITransazioneService> _mockTransazioneService;
        private readonly TransazioniController _controller;

        public TransazioniTests()
        {
            _mockTransazioneService = new Mock<ITransazioneService>();

            _controller = new TransazioniController(_mockTransazioneService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = CreateResponsabileUser()
                    }
                }
            };
        }

        private static ClaimsPrincipal CreateResponsabileUser(
            int userId = 1,
            string username = "mock",
            int puntoVenditaId = 1)
        {
            return new ClaimsPrincipal(
                new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, "Responsabile"),
                    new Claim("PuntoVenditaId", puntoVenditaId.ToString())
                }, "TestAuth")
            );
        }

        [Fact]
        public async Task AssegnaPunti_Should_ReturnOk_WithCorrectPoints()
        {
            // Arrange
            var request = new AssegnaPuntiRequest
            {
                CodiceFidelity = "TEST1234",
                ImportoSpesa = 100
            };

            var expectedResponse = new TransazioneResponse
            {
                PuntiAssegnati = 10
            };

            _mockTransazioneService
                .Setup(s => s.AssegnaPuntiAsync(
                    request,
                    1,      // puntoVenditaId
                    1,      // responsabileId
                    "mock"  // username
                ))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.AssegnaPunti(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

            var response = Assert.IsType<TransazioneResponse>(okResult.Value);
            Assert.Equal(10, response.PuntiAssegnati);

            _mockTransazioneService.Verify(
                s => s.AssegnaPuntiAsync(request, 1, 1, "mock"),
                Times.Once);
        }

        [Fact]
        public async Task AssegnaPunti_Should_ReturnBadRequest_WhenArgumentExceptionThrown()
        {
            // Arrange
            var request = new AssegnaPuntiRequest
            {
                CodiceFidelity = "TEST1234",
                ImportoSpesa = 5
            };

            _mockTransazioneService
                .Setup(s => s.AssegnaPuntiAsync(
                    It.IsAny<AssegnaPuntiRequest>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>()))
                .ThrowsAsync(new ArgumentException("Too low"));

            // Act
            var result = await _controller.AssegnaPunti(request);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequest.StatusCode);
        }

        [Fact]
        public async Task AssegnaPunti_Should_ReturnNotFound_WhenClienteNotFound()
        {
            // Arrange
            var request = new AssegnaPuntiRequest
            {
                CodiceFidelity = "NOTFOUND",
                ImportoSpesa = 50
            };

            _mockTransazioneService
                .Setup(s => s.AssegnaPuntiAsync(
                    It.IsAny<AssegnaPuntiRequest>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>()))
                .ThrowsAsync(new KeyNotFoundException("Cliente non trovato"));

            // Act
            var result = await _controller.AssegnaPunti(request);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status404NotFound, notFound.StatusCode);
        }
    }
}
