import { useMutation } from '@tanstack/react-query'
import { useNavigate } from 'react-router'
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
        login(result.data.token, result.data.refreshToken, {
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
        login(result.data.token, result.data.refreshToken, {
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
