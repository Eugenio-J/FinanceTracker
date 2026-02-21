import { useEffect } from 'react'
import { useMutation } from '@tanstack/react-query'
import { useNavigate } from 'react-router'
import axios from 'axios'
import api from '@/lib/axios'
import { useAuthStore } from '@/stores/auth-store'
import type { LoginRequest, RegisterRequest, AuthResponse, ApiResult } from '@/types/api'
import { toast } from 'sonner'

export function useLogin() {
  const login = useAuthStore((s) => s.login)
  const navigate = useNavigate()

  return useMutation({
    mutationFn: async (data: LoginRequest) => {
      const res = await api.post<ApiResult<AuthResponse>>('/auth/login', data)
      return res.data
    },
    onSuccess: (result) => {
      if (result.isSuccess && result.data) {
        login(result.data.token, {
          email: result.data.email,
          firstName: result.data.firstName,
          lastName: result.data.lastName,
        })
        navigate('/dashboard')
      } else {
        toast.error(result.error || 'Login failed')
      }
    },
    onError: (error: unknown) => {
      const msg = (error as { response?: { data?: { error?: string } } })?.response?.data?.error || 'Login failed'
      toast.error(msg)
    },
  })
}

export function useRegister() {
  const login = useAuthStore((s) => s.login)
  const navigate = useNavigate()

  return useMutation({
    mutationFn: async (data: RegisterRequest) => {
      const res = await api.post<ApiResult<AuthResponse>>('/auth/register', data)
      return res.data
    },
    onSuccess: (result) => {
      if (result.isSuccess && result.data) {
        login(result.data.token, {
          email: result.data.email,
          firstName: result.data.firstName,
          lastName: result.data.lastName,
        })
        toast.success('Account created!')
        navigate('/dashboard')
      } else {
        toast.error(result.error || 'Registration failed')
      }
    },
    onError: (error: unknown) => {
      const msg = (error as { response?: { data?: { error?: string } } })?.response?.data?.error || 'Registration failed'
      toast.error(msg)
    },
  })
}

export function useLogout() {
  const logout = useAuthStore((s) => s.logout)
  const navigate = useNavigate()

  return useMutation({
    mutationFn: async () => {
      await api.post('/auth/logout')
    },
    onSettled: () => {
      logout()
      navigate('/login')
    },
  })
}

// Module-level singleton: ensures only one refresh request is ever in-flight,
// even when React StrictMode double-mounts the component.
let silentRefreshPromise: Promise<void> | null = null

export function useSilentRefresh() {
  useEffect(() => {
    if (!silentRefreshPromise) {
      silentRefreshPromise = axios
        .post<ApiResult<AuthResponse>>('/api/auth/refresh', {}, { withCredentials: true })
        .then((res) => {
          if (res.data.isSuccess && res.data.data) {
            useAuthStore.getState().login(res.data.data.token, {
              email: res.data.data.email,
              firstName: res.data.data.firstName,
              lastName: res.data.data.lastName,
            })
          }
        })
        .catch(() => {
          // No valid refresh token — user stays logged out
        })
        .finally(() => {
          useAuthStore.getState().setInitialized(true)
          silentRefreshPromise = null
        })
    } else {
      // Second mount (StrictMode) — wait for the existing request to finish
      silentRefreshPromise.then(() => {
        if (!useAuthStore.getState().isInitialized) {
          useAuthStore.getState().setInitialized(true)
        }
      })
    }
  }, [])
}
