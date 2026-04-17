import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { apiConfig } from '../config/api.config';
import { CreateInvoiceRequest, Invoice } from '../models/invoice';

@Injectable({ providedIn: 'root' })
export class InvoicesApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = apiConfig.billingBaseUrl;

  getInvoices(): Observable<Invoice[]> {
    return this.http.get<Invoice[]>(`${this.baseUrl}/invoices`);
  }

  createInvoice(request: CreateInvoiceRequest): Observable<Invoice> {
    return this.http.post<Invoice>(`${this.baseUrl}/invoices`, request);
  }

  printInvoice(invoiceId: string): Observable<Invoice> {
    return this.http.post<Invoice>(`${this.baseUrl}/invoices/${invoiceId}/print`, {});
  }
}
