export interface RegisterRequest {
  name: string
  email: string
  password: string
}

export interface LoginRequest {
  email: string
  password: string
}

export interface LoginResponse {
  accessToken: string
  refreshToken: string
  expiresInSeconds: number
}

export interface UserResponse {
  id: string
  name: string
  email: string
  role: 'Customer' | 'Admin'
  createdAt: string
}

export interface Category {
  id: string
  name: string
}

export interface Product {
  id: string
  name: string
  description: string | null
  price: number
  stock: number
  categoryId: string
  categoryName: string | null
  allowsPreOrder: boolean
  imageUrl: string | null
  createdAt: string
}

export interface PagedResult<T> {
  items: T[]
  pageNumber: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export interface CartItem {
  productId: string
  quantity: number
}

export interface Cart {
  id: string
  userId: string
  items: CartItem[]
  updatedAt: string
}

export interface OrderItem {
  productId: string
  productName: string
  unitPrice: number
  quantity: number
  subtotal: number
}

export interface Order {
  id: string
  userId: string
  status: 'Pending' | 'Paid' | 'Cancelled'
  totalAmount: number
  createdAt: string
  items: OrderItem[]
}
