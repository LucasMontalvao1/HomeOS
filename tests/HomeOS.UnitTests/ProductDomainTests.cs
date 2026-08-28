using HomeOS.API.Modules.Products.Domain;

namespace HomeOS.UnitTests;

public class ProductDomainTests
{
    private readonly Guid _householdId = Guid.NewGuid();

    [Fact]
    public void Product_Create_ShouldSucceed_WithValidData()
    {
        var product = Product.Create(_householdId, "Arroz Camil 5kg", "Camil", "Grãos", "pacote", "7896006700973");

        Assert.NotEqual(Guid.Empty, product.Id);
        Assert.Equal(_householdId, product.HouseholdId);
        Assert.Equal("Arroz Camil 5kg", product.Name);
        Assert.Equal("Camil", product.Brand);
        Assert.Equal("Grãos", product.Category);
        Assert.Equal("pacote", product.Unit);
        Assert.Equal("7896006700973", product.Barcode);
    }

    [Fact]
    public void Product_Create_ShouldSucceed_WithOnlyRequiredFields()
    {
        var product = Product.Create(_householdId, "Arroz genérico");

        Assert.NotEqual(Guid.Empty, product.Id);
        Assert.Equal("Arroz genérico", product.Name);
        Assert.Null(product.Brand);
        Assert.Null(product.Barcode);
    }

    [Fact]
    public void Product_Create_ShouldThrow_WhenNameIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => Product.Create(_householdId, ""));
    }

    [Fact]
    public void Product_Create_ShouldThrow_WhenHouseholdIdIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => Product.Create(Guid.Empty, "Arroz"));
    }

    [Fact]
    public void Product_Create_ShouldTrimName()
    {
        var product = Product.Create(_householdId, "  Arroz  ");
        Assert.Equal("Arroz", product.Name);
    }

    [Fact]
    public void Product_Update_ShouldChangeName()
    {
        var product = Product.Create(_householdId, "Arroz 5kg");
        var originalUpdatedAt = product.UpdatedAt;

        // Pequeno delay para garantir que UpdatedAt muda
        System.Threading.Thread.Sleep(5);
        product.Update("Arroz Camil 5kg", "Camil", null, "pacote", null);

        Assert.Equal("Arroz Camil 5kg", product.Name);
        Assert.Equal("Camil", product.Brand);
        Assert.True(product.UpdatedAt >= originalUpdatedAt);
    }

    [Fact]
    public void Product_Update_ShouldThrow_WhenNameIsEmpty()
    {
        var product = Product.Create(_householdId, "Arroz");
        Assert.Throws<ArgumentException>(() => product.Update("", null, null, null, null));
    }
}
