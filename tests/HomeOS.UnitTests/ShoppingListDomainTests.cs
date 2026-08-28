using HomeOS.API.Modules.Shopping.Domain;

namespace HomeOS.UnitTests;

public class ShoppingListDomainTests
{
    private readonly Guid _householdId = Guid.NewGuid();
    private readonly Guid _productId = Guid.NewGuid();

    [Fact]
    public void ShoppingList_Create_ShouldSucceed_WithValidData()
    {
        var list = ShoppingList.Create(_householdId, "Supermercado");

        Assert.NotEqual(Guid.Empty, list.Id);
        Assert.Equal(_householdId, list.HouseholdId);
        Assert.Equal("Supermercado", list.Name);
        Assert.Equal(ShoppingListStatus.Pending, list.Status);
        Assert.Empty(list.Items);
    }

    [Fact]
    public void ShoppingList_Create_ShouldThrow_WhenNameIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => ShoppingList.Create(_householdId, ""));
    }

    [Fact]
    public void ShoppingList_Create_ShouldThrow_WhenHouseholdIdIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => ShoppingList.Create(Guid.Empty, "Mercado"));
    }

    [Fact]
    public void ShoppingList_AddItem_ShouldSucceed()
    {
        var list = ShoppingList.Create(_householdId, "Mercado");
        var item = list.AddItem(_productId, 2.5m, "kg");

        Assert.Single(list.Items);
        Assert.Equal(_productId, item.ProductId);
        Assert.Equal(2.5m, item.Quantity);
        Assert.Equal("kg", item.Unit);
        Assert.False(item.Checked);
    }

    [Fact]
    public void ShoppingList_AddItem_ShouldThrow_WhenQuantityIsZero()
    {
        var list = ShoppingList.Create(_householdId, "Mercado");
        Assert.Throws<ArgumentException>(() => list.AddItem(_productId, 0, null));
    }

    [Fact]
    public void ShoppingList_AddItem_ShouldThrow_WhenProductAlreadyExists()
    {
        var list = ShoppingList.Create(_householdId, "Mercado");
        list.AddItem(_productId, 1, null);
        
        Assert.Throws<InvalidOperationException>(() => list.AddItem(_productId, 2, null));
    }

    [Fact]
    public void ShoppingList_RemoveItem_ShouldSucceed()
    {
        var list = ShoppingList.Create(_householdId, "Mercado");
        var item = list.AddItem(_productId, 1, null);
        
        list.RemoveItem(item.Id);
        
        Assert.Empty(list.Items);
    }

    [Fact]
    public void ShoppingList_StartShopping_ShouldChangeStatusToInProgress()
    {
        var list = ShoppingList.Create(_householdId, "Mercado");
        list.StartShopping();
        
        Assert.Equal(ShoppingListStatus.InProgress, list.Status);
    }

    [Fact]
    public void ShoppingList_StartShopping_ShouldThrow_WhenAlreadyInProgress()
    {
        var list = ShoppingList.Create(_householdId, "Mercado");
        list.StartShopping();
        
        Assert.Throws<InvalidOperationException>(() => list.StartShopping());
    }

    [Fact]
    public void ShoppingList_CompleteShopping_ShouldSucceed()
    {
        var list = ShoppingList.Create(_householdId, "Mercado");
        list.StartShopping();
        list.CompleteShopping();
        
        Assert.Equal(ShoppingListStatus.Completed, list.Status);
    }

    [Fact]
    public void ShoppingList_AddItem_ShouldThrow_WhenCompleted()
    {
        var list = ShoppingList.Create(_householdId, "Mercado");
        list.StartShopping();
        list.CompleteShopping();
        
        Assert.Throws<InvalidOperationException>(() => list.AddItem(_productId, 1, null));
    }

    [Fact]
    public void ShoppingListItem_Check_ShouldSucceed()
    {
        var list = ShoppingList.Create(_householdId, "Mercado");
        var item = list.AddItem(_productId, 1, null);
        
        item.Check();
        
        Assert.True(item.Checked);
    }
}
