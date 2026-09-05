using HomeOS.API.Modules.Products.Application.DTOs;
using HomeOS.API.Modules.Products.Domain;
using HomeOS.API.Modules.Products.Infrastructure;

namespace HomeOS.API.Modules.Products.Application;

public class ProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request, Guid householdId)
    {
        // Se vier código de barras, checar duplicidade dentro da mesma casa
        if (!string.IsNullOrWhiteSpace(request.Barcode))
        {
            var existing = await _productRepository.FindByBarcodeAsync(request.Barcode, householdId);
            if (existing is not null)
                throw new InvalidOperationException($"Já existe um produto com o código de barras '{request.Barcode}' nesta casa.");
        }

        var product = Product.Create(
            householdId,
            request.Name,
            request.Brand,
            request.Category,
            request.Unit,
            request.Barcode,
            request.ImageUrl);

        await _productRepository.CreateAsync(product);
        return ToResponse(product);
    }

    public async Task<ProductResponse?> GetByIdAsync(Guid id, Guid householdId)
    {
        var product = await _productRepository.FindByIdAsync(id, householdId);
        return product is null ? null : ToResponse(product);
    }

    public async Task<IEnumerable<ProductResponse>> SearchAsync(string? query, Guid householdId)
    {
        var products = await _productRepository.SearchAsync(query, householdId);
        return products.Select(ToResponse);
    }

    public async Task<IEnumerable<ProductResponse>> GetAllAsync(Guid householdId)
    {
        var products = await _productRepository.GetAllAsync(householdId);
        return products.Select(ToResponse);
    }

    public async Task<ProductResponse> UpdateAsync(Guid id, UpdateProductRequest request, Guid householdId)
    {
        var product = await _productRepository.FindByIdAsync(id, householdId);
        if (product is null)
            throw new KeyNotFoundException("Produto não encontrado.");

        // Checar duplicidade de barcode se mudou
        if (!string.IsNullOrWhiteSpace(request.Barcode) && request.Barcode != product.Barcode)
        {
            var duplicate = await _productRepository.FindByBarcodeAsync(request.Barcode, householdId);
            if (duplicate is not null && duplicate.Id != id)
                throw new InvalidOperationException($"Já existe um produto com o código de barras '{request.Barcode}' nesta casa.");
        }

        product.Update(request.Name, request.Brand, request.Category, request.Unit, request.Barcode, request.ImageUrl);
        await _productRepository.UpdateAsync(product);
        return ToResponse(product);
    }

    public async Task DeleteAsync(Guid id, Guid householdId)
    {
        var product = await _productRepository.FindByIdAsync(id, householdId);
        if (product is null)
            throw new KeyNotFoundException("Produto não encontrado.");

        await _productRepository.DeleteAsync(id, householdId);
    }

    private static ProductResponse ToResponse(Product p) =>
        new(p.Id, p.HouseholdId, p.Name, p.Brand, p.Category, p.Unit, p.Barcode, p.ImageUrl, p.CreatedAt, p.UpdatedAt);
}
