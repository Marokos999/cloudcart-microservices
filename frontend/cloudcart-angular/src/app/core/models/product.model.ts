export interface Product {
  id: string;
  name: string;
  category: string;
  description: string;
  imageFile: string;
  images?: string[];
  price: number;
}

export interface ProductsResponse {
  data: Product[];
  total: number;
  page: number;
  pageSize: number;
}