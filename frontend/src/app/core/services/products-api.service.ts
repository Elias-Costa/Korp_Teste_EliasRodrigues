import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { apiConfig } from '../config/api.config';
import {
  CreateProductRequest,
  FailureModeResponse,
  Product,
  UpdateProductRequest,
} from '../models/product';

@Injectable({ providedIn: 'root' })
export class ProductsApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = apiConfig.inventoryBaseUrl;

  getProducts(): Observable<Product[]> {
    return this.http.get<Product[]>(`${this.baseUrl}/products`);
  }

  createProduct(request: CreateProductRequest): Observable<Product> {
    return this.http.post<Product>(`${this.baseUrl}/products`, request);
  }

  updateProduct(productId: string, request: UpdateProductRequest): Observable<Product> {
    return this.http.put<Product>(`${this.baseUrl}/products/${productId}`, request);
  }

  getFailureMode(): Observable<FailureModeResponse> {
    return this.http.get<FailureModeResponse>(`${this.baseUrl}/simulation/failure-mode`);
  }

  setFailureMode(enabled: boolean): Observable<FailureModeResponse> {
    return this.http.post<FailureModeResponse>(`${this.baseUrl}/simulation/failure-mode`, {
      enabled,
    });
  }
}
