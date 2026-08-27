using OrderService.Domain.Entities;

namespace OrderService.UnitTests.Domain;

public class CartTests
{
    [Fact]
    public void Constructor_CreatesEmptyCart_WhenUserIdIsValid()
    {
        var userId = Guid.NewGuid();

        var cart = new Cart(userId);

        Assert.Equal(userId, cart.UserId);
        Assert.Empty(cart.Items);
        Assert.NotEqual(Guid.Empty, cart.Id);
    }

    [Fact]
    public void Constructor_Throws_WhenUserIdIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => new Cart(Guid.Empty));
    }

    [Fact]
    public void AddItem_AddsNewItem_WhenProductNotInCart()
    {
        var cart = new Cart(Guid.NewGuid());
        var productId = Guid.NewGuid();

        cart.AddItem(productId, 2);

        var item = Assert.Single(cart.Items);
        Assert.Equal(productId, item.ProductId);
        Assert.Equal(2, item.Quantity);
    }

    [Fact]
    public void AddItem_IncreasesQuantity_WhenProductAlreadyInCart()
    {
        var cart = new Cart(Guid.NewGuid());
        var productId = Guid.NewGuid();
        cart.AddItem(productId, 2);

        cart.AddItem(productId, 3);

        var item = Assert.Single(cart.Items);
        Assert.Equal(5, item.Quantity);
    }

    [Fact]
    public void UpdateItemQuantity_UpdatesQuantity_WhenItemExists()
    {
        var cart = new Cart(Guid.NewGuid());
        var productId = Guid.NewGuid();
        cart.AddItem(productId, 2);

        cart.UpdateItemQuantity(productId, 5);

        Assert.Equal(5, cart.Items.Single().Quantity);
    }

    [Fact]
    public void UpdateItemQuantity_Throws_WhenItemNotInCart()
    {
        var cart = new Cart(Guid.NewGuid());

        Assert.Throws<InvalidOperationException>(() => cart.UpdateItemQuantity(Guid.NewGuid(), 1));
    }

    [Fact]
    public void UpdateItemQuantity_Throws_WhenQuantityIsNotPositive()
    {
        var cart = new Cart(Guid.NewGuid());
        var productId = Guid.NewGuid();
        cart.AddItem(productId, 2);

        Assert.Throws<ArgumentException>(() => cart.UpdateItemQuantity(productId, 0));
    }

    [Fact]
    public void RemoveItem_RemovesItem_WhenExists()
    {
        var cart = new Cart(Guid.NewGuid());
        var productId = Guid.NewGuid();
        cart.AddItem(productId, 2);

        cart.RemoveItem(productId);

        Assert.Empty(cart.Items);
    }

    [Fact]
    public void RemoveItem_DoesNothing_WhenProductNotInCart()
    {
        var cart = new Cart(Guid.NewGuid());
        cart.AddItem(Guid.NewGuid(), 2);

        cart.RemoveItem(Guid.NewGuid());

        Assert.Single(cart.Items);
    }

    [Fact]
    public void Clear_RemovesAllItems()
    {
        var cart = new Cart(Guid.NewGuid());
        cart.AddItem(Guid.NewGuid(), 1);
        cart.AddItem(Guid.NewGuid(), 2);

        cart.Clear();

        Assert.Empty(cart.Items);
    }
}
