namespace HomeOS.API.Modules.Products.Domain;

public class Product
{
    public Guid Id { get; private set; }
    public Guid HouseholdId { get; private set; }
    public string Name { get; private set; }
    public string? Brand { get; private set; }
    public string? Category { get; private set; }
    public string? Unit { get; private set; }
    public string? Barcode { get; private set; }
    public string? ImageUrl { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Product()
    {
        Name = string.Empty;
    }

    public static Product Create(
        Guid householdId,
        string name,
        string? brand = null,
        string? category = null,
        string? unit = null,
        string? barcode = null,
        string? imageUrl = null)
    {
        if (householdId == Guid.Empty)
            throw new ArgumentException("HouseholdId é obrigatório.", nameof(householdId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nome do produto é obrigatório.", nameof(name));

        var now = DateTime.UtcNow;
        return new Product
        {
            Id = Guid.NewGuid(),
            HouseholdId = householdId,
            Name = name.Trim(),
            Brand = brand?.Trim(),
            Category = category?.Trim(),
            Unit = unit?.Trim(),
            Barcode = barcode?.Trim(),
            ImageUrl = imageUrl?.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Update(string name, string? brand, string? category, string? unit, string? barcode, string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nome do produto é obrigatório.", nameof(name));

        Name = name.Trim();
        Brand = brand?.Trim();
        Category = category?.Trim();
        Unit = unit?.Trim();
        Barcode = barcode?.Trim();
        ImageUrl = imageUrl?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }
}
