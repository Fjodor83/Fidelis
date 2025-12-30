using Xunit;
using Fidelity.Domain.Entities;

namespace Fidelis.Tests;

public class DomainTests
{
    [Fact]
    public void Cliente_ShouldHaveZeroPoints_WhenCreated()
    {
        // Arrange
        var cliente = new Cliente();

        // Act & Assert
        Assert.Equal(0, cliente.PuntiTotali);
    }

    [Fact]
    public void Cliente_ShouldUpdatePoints_WhenPointsAdded()
    {
        // Arrange
        var cliente = new Cliente { PuntiTotali = 100 };

        // Act
        cliente.PuntiTotali += 50;

        // Assert
        Assert.Equal(150, cliente.PuntiTotali);
    }
}
