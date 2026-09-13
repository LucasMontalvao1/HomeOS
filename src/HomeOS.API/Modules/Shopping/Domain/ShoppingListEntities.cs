namespace HomeOS.API.Modules.Shopping.Domain;

public enum ShoppingListStatus
{
    Pending,
    InProgress,
    Completed,
    Cancelled
}

public class ShoppingList
{
    public Guid Id { get; private set; }
    public Guid HouseholdId { get; private set; }
    public string Name { get; private set; }
    public ShoppingListStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private readonly List<ShoppingListItem> _items = new();
    public IReadOnlyCollection<ShoppingListItem> Items => _items.AsReadOnly();

    private ShoppingList()
    {
        Name = string.Empty;
    }

    public static ShoppingList Create(Guid householdId, string name)
    {
        if (householdId == Guid.Empty)
            throw new ArgumentException("HouseholdId é obrigatório.", nameof(householdId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nome da lista é obrigatório.", nameof(name));

        var now = DateTime.UtcNow;
        return new ShoppingList
        {
            Id = Guid.NewGuid(),
            HouseholdId = householdId,
            Name = name.Trim(),
            Status = ShoppingListStatus.Pending,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nome da lista é obrigatório.", nameof(name));

        Name = name.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void StartShopping()
    {
        if (Status != ShoppingListStatus.Pending)
            throw new InvalidOperationException("Apenas listas pendentes podem ser iniciadas.");

        Status = ShoppingListStatus.InProgress;
        UpdatedAt = DateTime.UtcNow;
    }

    public void CompleteShopping()
    {
        if (Status != ShoppingListStatus.InProgress)
            throw new InvalidOperationException("Apenas listas em andamento podem ser concluídas.");

        Status = ShoppingListStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void CancelShopping()
    {
        if (Status == ShoppingListStatus.Completed)
            throw new InvalidOperationException("Listas concluídas não podem ser canceladas.");

        Status = ShoppingListStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public ShoppingListItem AddItem(Guid productId, decimal quantity, string? unit, decimal? price = null)
    {
        if (Status == ShoppingListStatus.Completed || Status == ShoppingListStatus.Cancelled)
            throw new InvalidOperationException("Não é possível adicionar itens a uma lista finalizada.");

        if (quantity <= 0)
            throw new ArgumentException("A quantidade deve ser maior que zero.", nameof(quantity));

        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem != null)
        {
            throw new InvalidOperationException("Produto já existe na lista.");
        }

        var item = ShoppingListItem.Create(Id, productId, quantity, unit, price);
        _items.Add(item);
        UpdatedAt = DateTime.UtcNow;
        return item;
    }

    public void RemoveItem(Guid itemId)
    {
        if (Status == ShoppingListStatus.Completed || Status == ShoppingListStatus.Cancelled)
            throw new InvalidOperationException("Não é possível remover itens de uma lista finalizada.");

        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item != null)
        {
            _items.Remove(item);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    // For rehydration from repository
    public void LoadItem(ShoppingListItem item)
    {
        _items.Add(item);
    }
}

public class ShoppingListItem
{
    public Guid Id { get; private set; }
    public Guid ShoppingListId { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal Quantity { get; private set; }
    public string? Unit { get; private set; }
    public decimal? Price { get; private set; }
    public bool Checked { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private ShoppingListItem() { }

    internal static ShoppingListItem Create(Guid shoppingListId, Guid productId, decimal quantity, string? unit, decimal? price = null)
    {
        var now = DateTime.UtcNow;
        return new ShoppingListItem
        {
            Id = Guid.NewGuid(),
            ShoppingListId = shoppingListId,
            ProductId = productId,
            Quantity = quantity,
            Unit = unit?.Trim(),
            Price = price,
            Checked = false,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Check()
    {
        Checked = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Uncheck()
    {
        Checked = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(decimal quantity, decimal? price)
    {
        if (quantity <= 0)
            throw new ArgumentException("A quantidade deve ser maior que zero.", nameof(quantity));
        if (price < 0)
            throw new ArgumentException("O preço não pode ser negativo.", nameof(price));

        Quantity = quantity;
        Price = price;
        UpdatedAt = DateTime.UtcNow;
    }
}
