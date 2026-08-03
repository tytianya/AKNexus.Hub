import axios from 'axios'
import type {
  AxiosRequestConfig,
  AxiosResponse,
  InternalAxiosRequestConfig
} from 'axios'

// Backend unified response format: Result<T>
export interface Result<T = unknown> {
  success: boolean
  message: string
  data: T
  errorCode: string | null
}

// Helper type representing the unwrapped data of a Result<T>
export type UnwrappedResponse<T> = T

// Typed HTTP client surface: methods return the unwrapped `data` directly,
// because the response interceptor below strips the Result<T> envelope.
export interface HttpClient {
  get<T = unknown>(url: string, config?: AxiosRequestConfig): Promise<T>
  post<T = unknown>(url: string, data?: unknown, config?: AxiosRequestConfig): Promise<T>
  put<T = unknown>(url: string, data?: unknown, config?: AxiosRequestConfig): Promise<T>
  delete<T = unknown>(url: string, config?: AxiosRequestConfig): Promise<T>
  patch<T = unknown>(url: string, data?: unknown, config?: AxiosRequestConfig): Promise<T>
}

// 后端地址（不含 /api）：留空走同域名 nginx 代理，填 http://host:port 直连后端
const BACKEND_ORIGIN = import.meta.env.VITE_API_BASE_URL || ''

const http = axios.create({
  baseURL: `${BACKEND_ORIGIN}/api`,
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json'
  }
})

// Request interceptor - placeholder for auth token
http.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token = localStorage.getItem('aknexus_access_token')
    if (token) {
      config.headers.set('Authorization', `Bearer ${token}`)
    }
    return config
  },
  (error) => Promise.reject(error)
)

// Response interceptor - unwrap Result<T>
http.interceptors.response.use(
  (response: AxiosResponse<Result<any>>) => {
    const result = response.data
    if (!result.success) {
      return Promise.reject(new Error(result.message || 'Request failed'))
    }
    return result.data
  },
  (error) => Promise.reject(error)
)

export default http as unknown as HttpClient
