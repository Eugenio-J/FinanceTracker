import axios from 'axios'
import { useAuthStore } from '@/stores/auth-store'
import type { ApiResult, AuthResponse } from '@/types/api'

const api = axios.create({
  baseURL: '/api',
  headers: {
    'Content-Type': 'application/json',
  },
})

api.interceptors.request.use((config) => {
  const token = useAuthStore.getState().token
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

let isRefreshing = false
let failedQueue: Array<{
  resolve: (token: string) => void
  reject: (error: unknown) => void
}> = []

function processQueue(error: unknown, token: string | null) {
  failedQueue.forEach(({ resolve, reject }) => {
    if (token) {
      resolve(token)
    } else {
      reject(error)
    }
  })
  failedQueue = []
}

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config

    if (error.response?.status !== 401 || originalRequest._retry) {
      return Promise.reject(error)
    }

    // Don't intercept auth endpoints (login/register/refresh)
    if (originalRequest.url?.includes('/auth/')) {
      return Promise.reject(error)
    }

    if (isRefreshing) {
      return new Promise<string>((resolve, reject) => {
        failedQueue.push({ resolve, reject })
      }).then((token) => {
        originalRequest.headers.Authorization = `Bearer ${token}`
        return api(originalRequest)
      })
    }

    originalRequest._retry = true
    isRefreshing = true

    const refreshToken = useAuthStore.getState().refreshToken

    if (!refreshToken) {
      isRefreshing = false
      useAuthStore.getState().logout()
      window.location.href = '/login'
      return Promise.reject(error)
    }

    try {
      // Use raw axios to avoid interceptor loop
      const res = await axios.post<ApiResult<AuthResponse>>('/api/auth/refresh', {
        refreshToken,
      })

      if (res.data.isSuccess && res.data.data) {
        const { token: newToken, refreshToken: newRefreshToken } = res.data.data
        useAuthStore.getState().setTokens(newToken, newRefreshToken)
        processQueue(null, newToken)
        originalRequest.headers.Authorization = `Bearer ${newToken}`
        return api(originalRequest)
      } else {
        processQueue(error, null)
        useAuthStore.getState().logout()
        window.location.href = '/login'
        return Promise.reject(error)
      }
    } catch (refreshError) {
      processQueue(refreshError, null)
      useAuthStore.getState().logout()
      window.location.href = '/login'
      return Promise.reject(refreshError)
    } finally {
      isRefreshing = false
    }
  }
)

export default api
