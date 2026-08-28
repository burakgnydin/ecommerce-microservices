using OrderService.Domain.Entities;
using OrderService.Domain.Enums;

namespace OrderService.UnitTests.Domain;

public class OrderTests
{
    private static OrderItem CreateItem(decimal unitPrice = 10m, int quantity = 2)
        => new(Guid.NewGuid(), "Widget", unitPrice, quantity);

    [Fact]
    public void Constructor_CreatesOrder_WhenDataIsValid()
    {
        var items = new[] { CreateItem(10m, 2), CreateItem(5m, 1) };

        var order = new Order(Guid.NewGuid(), items);

        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Equal(25m, order.TotalAmount);
        Assert.Equal(2, order.Items.Count);
        Assert.NotEqual(Guid.Empty, order.Id);
    }

    [Fact]
    public void Constructor_Throws_WhenUserIdIsEmpty()
    {
        var items = new[] { CreateItem() };

        Assert.Throws<ArgumentException>(() => new Order(Guid.Empty, items));
    }

    [Fact]
    public void Constructor_Throws_WhenItemsIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new Order(Guid.NewGuid(), null!));
    }

    [Fact]
    public void Constructor_Throws_WhenItemsIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => new Order(Guid.NewGuid(), []));
    }

    [Fact]
    public void Cancel_SetsStatusToCancelled_WhenOrderIsPending()
    {
        var order = new Order(Guid.NewGuid(), [CreateItem()]);

        order.Cancel();

        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public void Cancel_Throws_WhenOrderIsNotPending()
    {
        var order = new Order(Guid.NewGuid(), [CreateItem()]);
        order.Cancel();

        Assert.Throws<InvalidOperationException>(() => order.Cancel());
    }

    [Fact]
    public void MarkAsPaid_SetsStatusToPaid_WhenOrderIsPending()
    {
        var order = new Order(Guid.NewGuid(), [CreateItem()]);

        order.MarkAsPaid();

        Assert.Equal(OrderStatus.Paid, order.Status);
    }

    [Fact]
    public void MarkAsPaid_Throws_WhenOrderIsNotPending()
    {
        var order = new Order(Guid.NewGuid(), [CreateItem()]);
        order.MarkAsPaid();

        Assert.Throws<InvalidOperationException>(() => order.MarkAsPaid());
    }
}
