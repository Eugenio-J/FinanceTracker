import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import api from '@/lib/axios'
import type { Expense, CreateExpense, ExpenseSummary, ApiResult } from '@/types/api'
import { toast } from 'sonner'

export function useExpenses(startDate?: string, endDate?: string) {
  return useQuery({
    queryKey: ['expenses', startDate, endDate],
    queryFn: async () => {
      const params = new URLSearchParams()
      if (startDate) params.set('startDate', startDate)
      if (endDate) params.set('endDate', endDate)
      const res = await api.get<ApiResult<Expense[]>>(`/expenses?${params}`)
      return res.data.data!
    },
  })
}

export function useMonthlySummary(year: number, month: number) {
  return useQuery({
    queryKey: ['expenses', 'summary', year, month],
    queryFn: async () => {
      const res = await api.get<ApiResult<ExpenseSummary>>(`/expenses/summary?year=${year}&month=${month}`)
      return res.data.data!
    },
  })
}

export function useCreateExpense() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: CreateExpense) => {
      const res = await api.post<ApiResult<Expense>>('/expenses', data)
      return res.data
    },
    onSuccess: (result) => {
      if (result.isSuccess) {
        queryClient.invalidateQueries({ queryKey: ['expenses'] })
        queryClient.invalidateQueries({ queryKey: ['accounts'] })
        queryClient.invalidateQueries({ queryKey: ['dashboard'] })
        toast.success('Expense created!')
      } else {
        toast.error(result.error || 'Failed to create expense')
      }
    },
    onError: () => toast.error('Failed to create expense'),
  })
}

export function useDeleteExpense() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      const res = await api.delete(`/expenses/${id}`)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['expenses'] })
      queryClient.invalidateQueries({ queryKey: ['accounts'] })
      queryClient.invalidateQueries({ queryKey: ['dashboard'] })
      toast.success('Expense deleted!')
    },
    onError: () => toast.error('Failed to delete expense'),
  })
}
