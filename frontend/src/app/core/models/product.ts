export interface Product {
  id: string;
  code: string;
  description: string;
  stock: number;
  createdAtUtc: string;
}

export interface CreateProductRequest {
  code: string;
  description: string;
  stock: number;
}

export type UpdateProductRequest = CreateProductRequest;

export interface FailureModeResponse {
  enabled: boolean;
}
