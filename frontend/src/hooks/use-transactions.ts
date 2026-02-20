import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import api from '@/lib/axios'
import type { Transaction, CreateTransaction, TransactionFilter, PagedResult, ApiResult } from '@/types/api'
import { toast } from 'sonner'

export function useTransactions(filter: TransactionFilter = {}) {
  return useQuery({
    queryKey: ['transactions', filter],
    queryFn: async () => {
      const params = new URLSearchParams()
      if (filter.accountId) params.set('accountId', filter.accountId)
      if (filter.transactionType) params.set('transactionType', filter.transactionType)
      if (filter.category) params.set('category', filter.category)
      if (filter.startDate) params.set('startDate', filter.startDate)
      if (filter.endDate) params.set('endDate', filter.endDate)
      if (filter.pageNumber) params.set('pageNumber', filter.pageNumber.toString())
      if (filter.pageSize) params.set('pageSize', filter.pageSize.toString())
      const res = await api.get<ApiResult<PagedResult<Transaction>>>(`/transactions?${params}`)
      return res.data.data!
    },
  })
}

export function useTransaction(id: string) {
  return useQuery({
    queryKey: ['transactions', id],
    queryFn: async () => {
      const res = await api.get<ApiResult<Transaction>>(`/transactions/${id}`)
      return res.data.data!
    },
    enabled: !!id,
  })
}

export function useAccountTransactions(accountId: string) {
  return useQuery({
    queryKey: ['transactions', 'account', accountId],
    queryFn: async () => {
      const res = await api.get<ApiResult<Transaction[]>>(`/transactions/account/${accountId}`)
      return res.data.data!
    },
    enabled: !!accountId,
  })
}

export function useCreateTransaction() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: CreateTransaction) => {
      const res = await api.post<ApiResult<Transaction>>('/transactions', data)
      return res.data
    },
    onSuccess: (result) => {
      if (result.isSuccess) {
        queryClient.invalidateQueries({ queryKey: ['transactions'] })
        queryClient.invalidateQueries({ queryKey: ['accounts'] })
        queryClient.invalidateQueries({ queryKey: ['dashboard'] })
        toast.success('Transaction created!')
      } else {
        toast.error(result.error || 'Failed to create transaction')
      }
    },
    onError: () => toast.error('Failed to create transaction'),
  })
}

export function useDeleteTransaction() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      const res = await api.delete(`/transactions/${id}`)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['transactions'] })
      queryClient.invalidateQueries({ queryKey: ['accounts'] })
      queryClient.invalidateQueries({ queryKey: ['dashboard'] })
      toast.success('Transaction deleted!')
    },
    onError: () => toast.error('Failed to delete transaction'),
  })
}
