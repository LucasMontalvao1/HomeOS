import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Product {
  id: string;
  householdId: string;
  name: string;
  brand?: string;
  category?: string;
  unit?: string;
  barcode?: string;
  imageUrl?: string;
  createdAt: string;
  updatedAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private http = inject(HttpClient);
  private baseUrl = '/api/products';

  getProducts(householdId: string): Observable<Product[]> {
    return this.http.get<Product[]>(`${this.baseUrl}?householdId=${householdId}`);
  }

  getProductById(id: string, householdId: string): Observable<Product> {
    return this.http.get<Product>(`${this.baseUrl}/${id}?householdId=${householdId}`);
  }

  searchProducts(query: string, householdId: string): Observable<Product[]> {
    return this.http.get<Product[]>(`${this.baseUrl}/search?householdId=${householdId}&q=${encodeURIComponent(query)}`);
  }

  createProduct(product: { name: string; brand?: string; category?: string; unit?: string; barcode?: string; imageUrl?: string }, householdId: string): Observable<Product> {
    return this.http.post<Product>(`${this.baseUrl}?householdId=${householdId}`, product);
  }

  updateProduct(id: string, product: { name: string; brand?: string; category?: string; unit?: string; barcode?: string; imageUrl?: string }, householdId: string): Observable<Product> {
    return this.http.put<Product>(`${this.baseUrl}/${id}?householdId=${householdId}`, product);
  }

  deleteProduct(id: string, householdId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}?householdId=${householdId}`);
  }
}
