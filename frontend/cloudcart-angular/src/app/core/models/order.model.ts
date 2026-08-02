export interface OrderItem {
  orderId: string;
  productId: string;
  quantity: number;
  price: number;
  productName?: string;
  imageFile?: string;
}

export interface OrderAddress {
  firstName: string;
  lastName: string;
  emailAddress: string;
  addressLine: string;
  country: string;
  state: string;
  zipCode: string;
}

export interface Order {
  id: string;
  customerId: string;
  orderName: string;
  shippingAddress: OrderAddress;
  billingAddress: OrderAddress;
  status: string;
  orderItems: OrderItem[];
  totalPrice: number;
  createdAt: string;
}
