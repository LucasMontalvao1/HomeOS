using Dapper;
using HomeOS.API.Modules.Shopping.Domain;
using System.Data;

namespace HomeOS.API.Modules.Shopping.Infrastructure;

public interface IShoppingListRepository
{
    Task<ShoppingList?> FindByIdAsync(Guid id, Guid householdId);
    Task<IEnumerable<ShoppingList>> GetAllAsync(Guid householdId);
    Task CreateAsync(ShoppingList list);
    Task UpdateAsync(ShoppingList list);
    Task DeleteAsync(Guid id, Guid householdId);

    Task<ShoppingListItem?> FindItemByIdAsync(Guid itemId, Guid listId);
    Task CreateItemAsync(ShoppingListItem item);
    Task UpdateItemAsync(ShoppingListItem item);
    Task DeleteItemAsync(Guid itemId, Guid listId);
}

public class ShoppingListRepository : IShoppingListRepository
{
    private readonly IDbConnection _db;

    public ShoppingListRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task<ShoppingList?> FindByIdAsync(Guid id, Guid householdId)
    {
        const string sqlList = """
            SELECT Id, HouseholdId, Name, Status, CreatedAt, UpdatedAt
            FROM ShoppingLists
            WHERE Id = @Id AND HouseholdId = @HouseholdId
            """;
        var row = await _db.QuerySingleOrDefaultAsync(sqlList, new { Id = id, HouseholdId = householdId });
        if (row is null) return null;

        var list = MapShoppingList(row);

        const string sqlItems = """
            SELECT Id, ShoppingListId, ProductId, Quantity, Unit, Checked, CreatedAt, UpdatedAt
            FROM ShoppingListItems
            WHERE ShoppingListId = @ListId
            """;
        var items = await _db.QueryAsync(sqlItems, new { ListId = id });
        foreach (var itemRow in items)
        {
            list.LoadItem(MapShoppingListItem(itemRow));
        }

        return list;
    }

    public async Task<IEnumerable<ShoppingList>> GetAllAsync(Guid householdId)
    {
        const string sql = """
            SELECT Id, HouseholdId, Name, Status, CreatedAt, UpdatedAt
            FROM ShoppingLists
            WHERE HouseholdId = @HouseholdId
            ORDER BY CreatedAt DESC
            """;
        var rows = await _db.QueryAsync(sql, new { HouseholdId = householdId });
        return rows.Select(r => (ShoppingList)MapShoppingList(r));
    }

    public async Task CreateAsync(ShoppingList list)
    {
        const string sql = """
            INSERT INTO ShoppingLists (Id, HouseholdId, Name, Status, CreatedAt, UpdatedAt)
            VALUES (@Id, @HouseholdId, @Name, @Status, @CreatedAt, @UpdatedAt)
            """;
        await _db.ExecuteAsync(sql, new
        {
            list.Id,
            list.HouseholdId,
            list.Name,
            Status = list.Status.ToString(),
            list.CreatedAt,
            list.UpdatedAt
        });
    }

    public async Task UpdateAsync(ShoppingList list)
    {
        const string sql = """
            UPDATE ShoppingLists
            SET Name = @Name,
                Status = @Status,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id AND HouseholdId = @HouseholdId
            """;
        await _db.ExecuteAsync(sql, new
        {
            list.Id,
            list.HouseholdId,
            list.Name,
            Status = list.Status.ToString(),
            list.UpdatedAt
        });
    }

    public async Task DeleteAsync(Guid id, Guid householdId)
    {
        const string sql = "DELETE FROM ShoppingLists WHERE Id = @Id AND HouseholdId = @HouseholdId";
        await _db.ExecuteAsync(sql, new { Id = id, HouseholdId = householdId });
    }

    public async Task<ShoppingListItem?> FindItemByIdAsync(Guid itemId, Guid listId)
    {
        const string sql = """
            SELECT Id, ShoppingListId, ProductId, Quantity, Unit, Checked, CreatedAt, UpdatedAt
            FROM ShoppingListItems
            WHERE Id = @Id AND ShoppingListId = @ListId
            """;
        var row = await _db.QuerySingleOrDefaultAsync(sql, new { Id = itemId, ListId = listId });
        return row is null ? null : MapShoppingListItem(row);
    }

    public async Task CreateItemAsync(ShoppingListItem item)
    {
        const string sql = """
            INSERT INTO ShoppingListItems (Id, ShoppingListId, ProductId, Quantity, Unit, Checked, CreatedAt, UpdatedAt)
            VALUES (@Id, @ShoppingListId, @ProductId, @Quantity, @Unit, @Checked, @CreatedAt, @UpdatedAt)
            """;
        await _db.ExecuteAsync(sql, new
        {
            item.Id,
            item.ShoppingListId,
            item.ProductId,
            item.Quantity,
            item.Unit,
            item.Checked,
            item.CreatedAt,
            item.UpdatedAt
        });
    }

    public async Task UpdateItemAsync(ShoppingListItem item)
    {
        const string sql = """
            UPDATE ShoppingListItems
            SET Quantity = @Quantity,
                Unit = @Unit,
                Checked = @Checked,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id AND ShoppingListId = @ShoppingListId
            """;
        await _db.ExecuteAsync(sql, new
        {
            item.Id,
            item.ShoppingListId,
            item.Quantity,
            item.Unit,
            item.Checked,
            item.UpdatedAt
        });
    }

    public async Task DeleteItemAsync(Guid itemId, Guid listId)
    {
        const string sql = "DELETE FROM ShoppingListItems WHERE Id = @Id AND ShoppingListId = @ListId";
        await _db.ExecuteAsync(sql, new { Id = itemId, ListId = listId });
    }

    private static ShoppingList MapShoppingList(dynamic row)
    {
        var l = (ShoppingList)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(ShoppingList));
        typeof(ShoppingList).GetProperty(nameof(ShoppingList.Id))!.SetValue(l, (Guid)row.id);
        typeof(ShoppingList).GetProperty(nameof(ShoppingList.HouseholdId))!.SetValue(l, (Guid)row.householdid);
        typeof(ShoppingList).GetProperty(nameof(ShoppingList.Name))!.SetValue(l, (string)row.name);
        typeof(ShoppingList).GetProperty(nameof(ShoppingList.Status))!.SetValue(l, Enum.Parse<ShoppingListStatus>((string)row.status));
        typeof(ShoppingList).GetProperty(nameof(ShoppingList.CreatedAt))!.SetValue(l, (DateTime)row.createdat);
        typeof(ShoppingList).GetProperty(nameof(ShoppingList.UpdatedAt))!.SetValue(l, (DateTime)row.updatedat);
        return l;
    }

    private static ShoppingListItem MapShoppingListItem(dynamic row)
    {
        var i = (ShoppingListItem)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(ShoppingListItem));
        typeof(ShoppingListItem).GetProperty(nameof(ShoppingListItem.Id))!.SetValue(i, (Guid)row.id);
        typeof(ShoppingListItem).GetProperty(nameof(ShoppingListItem.ShoppingListId))!.SetValue(i, (Guid)row.shoppinglistid);
        typeof(ShoppingListItem).GetProperty(nameof(ShoppingListItem.ProductId))!.SetValue(i, (Guid)row.productid);
        typeof(ShoppingListItem).GetProperty(nameof(ShoppingListItem.Quantity))!.SetValue(i, (decimal)row.quantity);
        typeof(ShoppingListItem).GetProperty(nameof(ShoppingListItem.Unit))!.SetValue(i, (string?)row.unit);
        typeof(ShoppingListItem).GetProperty(nameof(ShoppingListItem.Checked))!.SetValue(i, (bool)row.@checked);
        typeof(ShoppingListItem).GetProperty(nameof(ShoppingListItem.CreatedAt))!.SetValue(i, (DateTime)row.createdat);
        typeof(ShoppingListItem).GetProperty(nameof(ShoppingListItem.UpdatedAt))!.SetValue(i, (DateTime)row.updatedat);
        return i;
    }
}
