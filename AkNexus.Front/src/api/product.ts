import http from './http'

export interface Product {
  id: number
  productCode: string
  productName: string
  spec: string
  batchNo: string
  status: string
  boundFlowCode: string | null
  createdAt: string
}

export interface ProductCreateDto {
  productCode: string
  productName: string
  spec?: string
  batchNo?: string
  status?: string
}

export const getProducts = () => http.get<Product[]>('/Product')

export const getProductById = (id: number) => http.get<Product>(`/Product/${id}`)

export const createProduct = (data: ProductCreateDto) => http.post<Product>('/Product', data)

export const updateProduct = (id: number, data: ProductCreateDto) =>
  http.put<void>(`/Product/${id}`, data)

export const deleteProduct = (id: number) => http.delete<void>(`/Product/${id}`)

export const bindFlow = (productId: number, flowId: number) =>
  http.post<void>(`/Product/${productId}/bind-flow/${flowId}`)
