using HomeOS.API.Modules.Products.Domain;
using Dapper;
using System.Data;

namespace HomeOS.API.Modules.Products.Infrastructure;

public interface IProductRepository
{
    Task<Product?> FindByIdAsync(Guid id, Guid householdId);
    Task<Product?> FindByBarcodeAsync(string barcode, Guid householdId);
    Task<IEnumerable<Product>> SearchAsync(string? query, Guid householdId);
    Task<IEnumerable<Product>> GetAllAsync(Guid householdId);
    Task CreateAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(Guid id, Guid householdId);
}

public class ProductRepository : IProductRepository
{
    private readonly IDbConnection _db;

    public ProductRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task<Product?> FindByIdAsync(Guid id, Guid householdId)
    {
        const string sql = """
            SELECT Id, HouseholdId, Name, Brand, Category, Unit, Barcode, ImageUrl, CreatedAt, UpdatedAt
            FROM Products
            WHERE Id = @Id AND HouseholdId = @HouseholdId
            """;
        var row = await _db.QuerySingleOrDefaultAsync(sql, new { Id = id, HouseholdId = householdId });
        return row is null ? null : MapProduct(row);
    }

    public async Task<Product?> FindByBarcodeAsync(string barcode, Guid householdId)
    {
        const string sql = """
            SELECT Id, HouseholdId, Name, Brand, Category, Unit, Barcode, ImageUrl, CreatedAt, UpdatedAt
            FROM Products
            WHERE Barcode = @Barcode AND HouseholdId = @HouseholdId
            """;
        var row = await _db.QuerySingleOrDefaultAsync(sql, new { Barcode = barcode, HouseholdId = householdId });
        return row is null ? null : MapProduct(row);
    }

    public async Task<IEnumerable<Product>> SearchAsync(string? query, Guid householdId)
    {
        const string sql = """
            SELECT Id, HouseholdId, Name, Brand, Category, Unit, Barcode, ImageUrl, CreatedAt, UpdatedAt
            FROM Products
            WHERE HouseholdId = @HouseholdId
              AND (@Query IS NULL OR Name ILIKE @Pattern OR Brand ILIKE @Pattern OR Barcode = @Query)
            ORDER BY Name ASC
            LIMIT 50
            """;
        var rows = await _db.QueryAsync(sql, new
        {
            HouseholdId = householdId,
            Query = query,
            Pattern = string.IsNullOrWhiteSpace(query) ? null : $"%{query}%"
        });
        return rows.Select(r => (Product)MapProduct(r));
    }

    public async Task<IEnumerable<Product>> GetAllAsync(Guid householdId)
    {
        const string sql = """
            SELECT Id, HouseholdId, Name, Brand, Category, Unit, Barcode, ImageUrl, CreatedAt, UpdatedAt
            FROM Products
            WHERE HouseholdId = @HouseholdId
            ORDER BY Name ASC
            """;
        var rows = await _db.QueryAsync(sql, new { HouseholdId = householdId });
        return rows.Select(r => (Product)MapProduct(r));
    }

    public async Task CreateAsync(Product product)
    {
        const string sql = """
            INSERT INTO Products (Id, HouseholdId, Name, Brand, Category, Unit, Barcode, ImageUrl, CreatedAt, UpdatedAt)
            VALUES (@Id, @HouseholdId, @Name, @Brand, @Category, @Unit, @Barcode, @ImageUrl, @CreatedAt, @UpdatedAt)
            """;
        await _db.ExecuteAsync(sql, new
        {
            product.Id,
            product.HouseholdId,
            product.Name,
            product.Brand,
            product.Category,
            product.Unit,
            product.Barcode,
            product.ImageUrl,
            product.CreatedAt,
            product.UpdatedAt
        });
    }

    public async Task UpdateAsync(Product product)
    {
        const string sql = """
            UPDATE Products
            SET Name = @Name,
                Brand = @Brand,
                Category = @Category,
                Unit = @Unit,
                Barcode = @Barcode,
                ImageUrl = @ImageUrl,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id AND HouseholdId = @HouseholdId
            """;
        await _db.ExecuteAsync(sql, new
        {
            product.Id,
            product.HouseholdId,
            product.Name,
            product.Brand,
            product.Category,
            product.Unit,
            product.Barcode,
            product.ImageUrl,
            product.UpdatedAt
        });
    }

    public async Task DeleteAsync(Guid id, Guid householdId)
    {
        const string sql = "DELETE FROM Products WHERE Id = @Id AND HouseholdId = @HouseholdId";
        await _db.ExecuteAsync(sql, new { Id = id, HouseholdId = householdId });
    }

    private static Product MapProduct(dynamic row)
    {
        var p = (Product)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(Product));
        typeof(Product).GetProperty(nameof(Product.Id))!.SetValue(p, (Guid)row.id);
        typeof(Product).GetProperty(nameof(Product.HouseholdId))!.SetValue(p, (Guid)row.householdid);
        typeof(Product).GetProperty(nameof(Product.Name))!.SetValue(p, (string)row.name);
        typeof(Product).GetProperty(nameof(Product.Brand))!.SetValue(p, (string?)row.brand);
        typeof(Product).GetProperty(nameof(Product.Category))!.SetValue(p, (string?)row.category);
        typeof(Product).GetProperty(nameof(Product.Unit))!.SetValue(p, (string?)row.unit);
        typeof(Product).GetProperty(nameof(Product.Barcode))!.SetValue(p, (string?)row.barcode);
        typeof(Product).GetProperty(nameof(Product.ImageUrl))!.SetValue(p, (string?)row.imageurl);
        typeof(Product).GetProperty(nameof(Product.CreatedAt))!.SetValue(p, (DateTime)row.createdat);
        typeof(Product).GetProperty(nameof(Product.UpdatedAt))!.SetValue(p, (DateTime)row.updatedat);
        return p;
    }
}
