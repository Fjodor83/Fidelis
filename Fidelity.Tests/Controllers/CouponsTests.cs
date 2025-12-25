using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using Fidelity.Server.Controllers;
using Fidelity.Server.Data;
using Fidelity.Server.Services;
using Fidelity.Shared.Models;
using Fidelity.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AutoMapper;
using Fidelity.Server;

namespace Fidelity.Tests.Controllers
{
    public class CouponsTests
    {
        private readonly Mock<ICouponService> _mockCouponService;
        private readonly CouponsController _controller;

        public CouponsTests()
        {
            _mockCouponService = new Mock<ICouponService>();
            _controller = new CouponsController(_mockCouponService.Object);
        }

        [Fact]
        public async Task AssegnaCoupon_Should_ReturnBadRequest_IfArgumentException()
        {
            // Arrange
            var request = new AssegnaCouponRequest { CouponId = 1, ClienteId = 1 };

            _mockCouponService
                .Setup(s => s.AssegnaCouponAsync(1, 1))
                .ThrowsAsync(new ArgumentException("Coupon scaduto."));

            // Act
            var result = await _controller.AssegnaCoupon(request);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Coupon scaduto.", badRequest.Value);
        }

         [Fact]
        public async Task AssegnaCoupon_Should_ReturnOk_IfSuccess()
        {
            // Arrange
            var request = new AssegnaCouponRequest { CouponId = 2, ClienteId = 2 };

            _mockCouponService
                .Setup(s => s.AssegnaCouponAsync(2, 2))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.AssegnaCoupon(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            // Verify service called
            _mockCouponService.Verify(s => s.AssegnaCouponAsync(2, 2), Times.Once);
        }
    }
}
