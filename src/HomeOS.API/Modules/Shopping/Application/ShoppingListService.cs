using HomeOS.API.Modules.Products.Infrastructure;
using HomeOS.API.Modules.Shopping.Application.DTOs;
using HomeOS.API.Modules.Shopping.Domain;
using HomeOS.API.Modules.Shopping.Infrastructure;

namespace HomeOS.API.Modules.Shopping.Application;

public class ShoppingListService
{
    private readonly IShoppingListRepository _shoppingListRepository;
    private readonly IProductRepository _productRepository;

    public ShoppingListService(
        IShoppingListRepository shoppingListRepository,
        IProductRepository productRepository)
    {
        _shoppingListRepository = shoppingListRepository;
        _productRepository = productRepository;
    }

    public async Task<ShoppingListResponse> CreateAsync(CreateShoppingListRequest request, Guid householdId)
    {
        var list = ShoppingList.Create(householdId, request.Name);
        await _shoppingListRepository.CreateAsync(list);
        return await MapToResponseAsync(list);
    }

    public async Task<ShoppingListResponse?> GetByIdAsync(Guid id, Guid householdId)
    {
        var list = await _shoppingListRepository.FindByIdAsync(id, householdId);
        return list is null ? null : await MapToResponseAsync(list);
    }

    public async Task<IEnumerable<ShoppingListSummaryResponse>> GetAllAsync(Guid householdId)
    {
        var lists = await _shoppingListRepository.GetAllAsync(householdId);
        var summaries = new List<ShoppingListSummaryResponse>();
        foreach (var list in lists)
        {
            // Reload list with items to calculate summary
            var fullList = await _shoppingListRepository.FindByIdAsync(list.Id, householdId);
            if (fullList != null)
            {
                summaries.Add(new ShoppingListSummaryResponse(
                    fullList.Id,
                    fullList.HouseholdId,
                    fullList.Name,
                    fullList.Status.ToString(),
                    fullList.CreatedAt,
                    fullList.UpdatedAt,
                    fullList.Items.Count,
                    fullList.Items.Count(i => i.Checked)
                ));
            }
        }
        return summaries;
    }

    public async Task<ShoppingListResponse> UpdateAsync(Guid id, UpdateShoppingListRequest request, Guid householdId)
    {
        var list = await _shoppingListRepository.FindByIdAsync(id, householdId);
        if (list is null)
            throw new KeyNotFoundException("Lista não encontrada.");

        list.UpdateName(request.Name);
        await _shoppingListRepository.UpdateAsync(list);
        return await MapToResponseAsync(list);
    }

    public async Task DeleteAsync(Guid id, Guid householdId)
    {
        var list = await _shoppingListRepository.FindByIdAsync(id, householdId);
        if (list is null)
            throw new KeyNotFoundException("Lista não encontrada.");

        await _shoppingListRepository.DeleteAsync(id, householdId);
    }

    public async Task<ShoppingListResponse> StartShoppingAsync(Guid id, Guid householdId)
    {
        var list = await _shoppingListRepository.FindByIdAsync(id, householdId);
        if (list is null)
            throw new KeyNotFoundException("Lista não encontrada.");

        list.StartShopping();
        await _shoppingListRepository.UpdateAsync(list);
        return await MapToResponseAsync(list);
    }

    public async Task<ShoppingListResponse> CompleteShoppingAsync(Guid id, Guid householdId)
    {
        var list = await _shoppingListRepository.FindByIdAsync(id, householdId);
        if (list is null)
            throw new KeyNotFoundException("Lista não encontrada.");

        list.CompleteShopping();
        await _shoppingListRepository.UpdateAsync(list);
        return await MapToResponseAsync(list);
    }

    public async Task<ShoppingListResponse> CancelShoppingAsync(Guid id, Guid householdId)
    {
        var list = await _shoppingListRepository.FindByIdAsync(id, householdId);
        if (list is null)
            throw new KeyNotFoundException("Lista não encontrada.");

        list.CancelShopping();
        await _shoppingListRepository.UpdateAsync(list);
        return await MapToResponseAsync(list);
    }

    public async Task<ShoppingListItemResponse> AddItemAsync(Guid listId, AddItemRequest request, Guid householdId)
    {
        var list = await _shoppingListRepository.FindByIdAsync(listId, householdId);
        if (list is null)
            throw new KeyNotFoundException("Lista não encontrada.");

        var product = await _productRepository.FindByIdAsync(request.ProductId, householdId);
        if (product is null)
            throw new KeyNotFoundException("Produto não encontrado.");

        var item = list.AddItem(request.ProductId, request.Quantity, request.Unit);
        
        await _shoppingListRepository.UpdateAsync(list); // Updates list timestamp
        await _shoppingListRepository.CreateItemAsync(item);
        
        return new ShoppingListItemResponse(
            item.Id,
            item.ProductId,
            product.Name,
            product.Brand,
            product.Barcode,
            item.Quantity,
            item.Unit,
            item.Checked,
            item.CreatedAt,
            item.UpdatedAt);
    }

    public async Task RemoveItemAsync(Guid listId, Guid itemId, Guid householdId)
    {
        var list = await _shoppingListRepository.FindByIdAsync(listId, householdId);
        if (list is null)
            throw new KeyNotFoundException("Lista não encontrada.");

        list.RemoveItem(itemId); // Domain logic validation
        
        await _shoppingListRepository.UpdateAsync(list);
        await _shoppingListRepository.DeleteItemAsync(itemId, listId);
    }

    public async Task<ShoppingListItemResponse> CheckItemAsync(Guid listId, Guid itemId, Guid householdId)
    {
        var list = await _shoppingListRepository.FindByIdAsync(listId, householdId);
        if (list is null)
            throw new KeyNotFoundException("Lista não encontrada.");

        var item = await _shoppingListRepository.FindItemByIdAsync(itemId, listId);
        if (item is null)
            throw new KeyNotFoundException("Item não encontrado.");

        item.Check();
        await _shoppingListRepository.UpdateItemAsync(item);

        return await MapItemToResponseAsync(item, householdId);
    }

    public async Task<ShoppingListItemResponse> UncheckItemAsync(Guid listId, Guid itemId, Guid householdId)
    {
        var list = await _shoppingListRepository.FindByIdAsync(listId, householdId);
        if (list is null)
            throw new KeyNotFoundException("Lista não encontrada.");

        var item = await _shoppingListRepository.FindItemByIdAsync(itemId, listId);
        if (item is null)
            throw new KeyNotFoundException("Item não encontrado.");

        item.Uncheck();
        await _shoppingListRepository.UpdateItemAsync(item);

        return await MapItemToResponseAsync(item, householdId);
    }
    
    public async Task<ShoppingListItemResponse> UpdateItemQuantityAsync(Guid listId, Guid itemId, UpdateItemQuantityRequest request, Guid householdId)
    {
        var list = await _shoppingListRepository.FindByIdAsync(listId, householdId);
        if (list is null)
            throw new KeyNotFoundException("Lista não encontrada.");

        var item = await _shoppingListRepository.FindItemByIdAsync(itemId, listId);
        if (item is null)
            throw new KeyNotFoundException("Item não encontrado.");

        item.UpdateQuantity(request.Quantity);
        await _shoppingListRepository.UpdateItemAsync(item);

        return await MapItemToResponseAsync(item, householdId);
    }

    private async Task<ShoppingListResponse> MapToResponseAsync(ShoppingList list)
    {
        var itemResponses = new List<ShoppingListItemResponse>();
        foreach (var item in list.Items)
        {
            var product = await _productRepository.FindByIdAsync(item.ProductId, list.HouseholdId);
            if (product != null)
            {
                itemResponses.Add(new ShoppingListItemResponse(
                    item.Id,
                    item.ProductId,
                    product.Name,
                    product.Brand,
                    product.Barcode,
                    item.Quantity,
                    item.Unit,
                    item.Checked,
                    item.CreatedAt,
                    item.UpdatedAt));
            }
        }

        return new ShoppingListResponse(
            list.Id,
            list.HouseholdId,
            list.Name,
            list.Status.ToString(),
            list.CreatedAt,
            list.UpdatedAt,
            itemResponses);
    }
    
    private async Task<ShoppingListItemResponse> MapItemToResponseAsync(ShoppingListItem item, Guid householdId)
    {
        var product = await _productRepository.FindByIdAsync(item.ProductId, householdId);
        if (product == null) throw new InvalidOperationException("Product not found");
        
        return new ShoppingListItemResponse(
            item.Id,
            item.ProductId,
            product.Name,
            product.Brand,
            product.Barcode,
            item.Quantity,
            item.Unit,
            item.Checked,
            item.CreatedAt,
            item.UpdatedAt);
    }
}
