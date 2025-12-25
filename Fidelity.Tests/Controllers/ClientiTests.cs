using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Fidelity.Server.Controllers;
using Fidelity.Server.Services;
using Fidelity.Shared.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Fidelity.Tests.Controllers
{
    public class ClientiTests
    {
        private readonly Mock<IClienteService> _mockClienteService;
        private readonly ClientiController _controller;

        public ClientiTests()
        {
            _mockClienteService = new Mock<IClienteService>();
            _controller = new ClientiController(_mockClienteService.Object);
            
            // Setup default context needed by controller logic (User claims)
             var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "Responsabile"),
                new Claim("PuntoVenditaId", "1")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task CercaCliente_Should_ReturnOk_WhenResultsFound()
        {
            // Arrange
            var query = "Test";
            var expected = new List<ClienteResponse> { new ClienteResponse { Id = 1, NomeCompleto = "Test User" } };

            _mockClienteService
                .Setup(s => s.CercaClientiAsync(query, "Responsabile", 1))
                .ReturnsAsync(expected);

            // Act
            var result = await _controller.CercaCliente(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var items = Assert.IsType<List<ClienteResponse>>(okResult.Value);
            Assert.Single(items);
            Assert.Equal("Test User", items[0].NomeCompleto);
        }

        [Fact]
        public async Task GetClientiMioPuntoVendita_Should_ReturnOk()
        {
            // Arrange
            var expected = new List<ClienteResponse> 
            { 
                new ClienteResponse { Id = 1, NomeCompleto = "C1", PuntoVenditaRegistrazione = "Store 1" } 
            };

            _mockClienteService
                .Setup(s => s.GetClientiByPuntoVenditaAsync("Responsabile", 1))
                .ReturnsAsync(expected);

            // Act
            var result = await _controller.GetClientiMioPuntoVendita();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var items = Assert.IsType<List<ClienteResponse>>(okResult.Value);
            Assert.Single(items);
        }

         [Fact]
        public async Task GetCliente_Should_ReturnNotFound_WhenNull()
        {
            // Arrange
            _mockClienteService
                .Setup(s => s.GetClienteByIdAsync(99, "Responsabile", 1))
                .ReturnsAsync((ClienteResponse?)null);

            // Act
            var result = await _controller.GetCliente(99);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }
    }
}
