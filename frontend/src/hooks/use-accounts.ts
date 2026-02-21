import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import api from '@/lib/axios'
import type { Account, CreateAccount, UpdateAccount, ApiResult } from '@/types/api'
import { toast } from 'sonner'

export function useAccounts(enabled = true) {
  return useQuery({
    queryKey: ['accounts'],
    queryFn: async () => {
      const res = await api.get<ApiResult<Account[]>>('/accounts')
      //try commit
      return res.data.data!
    },
    enabled,
  })
}

export function useAccount(id: string) {
  return useQuery({
    queryKey: ['accounts', id],
    queryFn: async () => {
      const res = await api.get<ApiResult<Account>>(`/accounts/${id}`)
      return res.data.data!
    },
    enabled: !!id,
  })
}

export function useTotalBalance() {
  return useQuery({
    queryKey: ['accounts', 'total-balance'],
    queryFn: async () => {
      const res = await api.get<ApiResult<number>>('/accounts/total-balance')
      return res.data.data!
    },
  })
}

export function useCreateAccount() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: CreateAccount) => {
      const res = await api.post<ApiResult<Account>>('/accounts', data)
      return res.data
    },
    onSuccess: (result) => {
      if (result.isSuccess) {
        queryClient.invalidateQueries({ queryKey: ['accounts'] })
        toast.success('Account created!')
      } else {
        toast.error(result.error || 'Failed to create account')
      }
    },
    onError: () => toast.error('Failed to create account'),
  })
}

export function useUpdateAccount() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ id, data }: { id: string; data: UpdateAccount }) => {
      const res = await api.put<ApiResult<Account>>(`/accounts/${id}`, data)
      return res.data
    },
    onSuccess: (result) => {
      if (result.isSuccess) {
        queryClient.invalidateQueries({ queryKey: ['accounts'] })
        toast.success('Account updated!')
      } else {
        toast.error(result.error || 'Failed to update account')
      }
    },
    onError: () => toast.error('Failed to update account'),
  })
}

export function useDeleteAccount() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      const res = await api.delete(`/accounts/${id}`)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['accounts'] })
      toast.success('Account deleted!')
    },
    onError: () => toast.error('Failed to delete account'),
  })
}
