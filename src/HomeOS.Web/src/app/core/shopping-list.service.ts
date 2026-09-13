import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export type ShoppingListStatus = 'Pending' | 'InProgress' | 'Completed' | 'Cancelled';

export interface ShoppingListItem {
  id: string;
  productId: string;
  productName: string;
  productBrand?: string;
  productBarcode?: string;
  quantity: number;
  unit?: string;
  price?: number;
  checked: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface ShoppingList {
  id: string;
  householdId: string;
  name: string;
  status: ShoppingListStatus;
  createdAt: string;
  updatedAt: string;
  items: ShoppingListItem[];
}

export interface ShoppingListSummary {
  id: string;
  householdId: string;
  name: string;
  status: ShoppingListStatus;
  createdAt: string;
  updatedAt: string;
  totalItems: number;
  checkedItems: number;
}

@Injectable({
  providedIn: 'root'
})
export class ShoppingListService {
  private http = inject(HttpClient);
  private baseUrl = '/api/shopping-lists';

  getLists(householdId: string): Observable<ShoppingListSummary[]> {
    return this.http.get<ShoppingListSummary[]>(`${this.baseUrl}?householdId=${householdId}`);
  }

  getListById(id: string, householdId: string): Observable<ShoppingList> {
    return this.http.get<ShoppingList>(`${this.baseUrl}/${id}?householdId=${householdId}`);
  }

  createList(name: string, householdId: string): Observable<ShoppingList> {
    return this.http.post<ShoppingList>(`${this.baseUrl}?householdId=${householdId}`, { name });
  }

  deleteList(id: string, householdId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}?householdId=${householdId}`);
  }

  startShopping(id: string, householdId: string): Observable<ShoppingList> {
    return this.http.post<ShoppingList>(`${this.baseUrl}/${id}/start?householdId=${householdId}`, {});
  }

  completeShopping(id: string, householdId: string): Observable<ShoppingList> {
    return this.http.post<ShoppingList>(`${this.baseUrl}/${id}/complete?householdId=${householdId}`, {});
  }

  addItem(listId: string, productId: string, quantity: number, householdId: string, unit?: string, price?: number): Observable<ShoppingListItem> {
    return this.http.post<ShoppingListItem>(`${this.baseUrl}/${listId}/items?householdId=${householdId}`, { productId, quantity, unit, price });
  }

  removeItem(listId: string, itemId: string, householdId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${listId}/items/${itemId}?householdId=${householdId}`);
  }

  checkItem(listId: string, itemId: string, householdId: string): Observable<ShoppingListItem> {
    return this.http.put<ShoppingListItem>(`${this.baseUrl}/${listId}/items/${itemId}/check?householdId=${householdId}`, {});
  }

  uncheckItem(listId: string, itemId: string, householdId: string): Observable<ShoppingListItem> {
    return this.http.put<ShoppingListItem>(`${this.baseUrl}/${listId}/items/${itemId}/uncheck?householdId=${householdId}`, {});
  }

  updateItemDetails(listId: string, itemId: string, quantity: number, householdId: string, price?: number): Observable<ShoppingListItem> {
    return this.http.put<ShoppingListItem>(`${this.baseUrl}/${listId}/items/${itemId}/details?householdId=${householdId}`, { quantity, price });
  }
}
