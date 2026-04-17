export interface InvoiceItem {
  productId: string;
  productCode: string;
  productDescription: string;
  quantity: number;
}

export interface Invoice {
  id: string;
  number: number;
  status: 'Open' | 'Closed';
  createdAtUtc: string;
  closedAtUtc: string | null;
  items: InvoiceItem[];
}

export interface CreateInvoiceItemRequest {
  productId: string;
  quantity: number;
}

export interface CreateInvoiceRequest {
  items: CreateInvoiceItemRequest[];
}
