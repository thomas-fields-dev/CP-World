using Microsoft.EntityFrameworkCore;
using CpWorld.Models;
using CpWorld.Infrastructure;
using CpWorld.Services;
using Moq;

namespace CPTest.Tests;

public class UnitTest
{
    [Fact]
    public void Returns_Two_Results()
    {
        // Arrange
        var data = new List<Order>
        {
            new Order
            {
                CustomerName = "Testa Man",
                OrderId = 999,
                OrderStatus = CpWorld.Enums.OrderStatus.Pending
            },
            new Order
            {
                CustomerName = "Testa Man",
                OrderId = 998,
                OrderStatus = CpWorld.Enums.OrderStatus.Pending
            },
            new Order
            {
                CustomerName = "Testa Man",
                OrderId = 997,
                OrderStatus = CpWorld.Enums.OrderStatus.Pending
            },

        }
        .AsQueryable();

        var mockSet = new Mock<DbSet<Order>>();
        mockSet.As<IQueryable<Order>>().Setup(m => m.Provider).Returns(data.Provider);
        mockSet.As<IQueryable<Order>>().Setup(m => m.Expression).Returns(data.Expression);
        mockSet.As<IQueryable<Order>>().Setup(m => m.ElementType).Returns(data.ElementType);
        mockSet.As<IQueryable<Order>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

        var mockContext = new Mock<CPWorldDbContent>();
        mockContext.Setup(c => c.Orders).Returns(mockSet.Object);

        //act
        var result = OrderService.GetAllOrders("", CpWorld.Enums.OrderStatus.All, 1, mockContext.Object);

        //assert
        Assert.Equal(2, result.Response.Count);
    }

}

