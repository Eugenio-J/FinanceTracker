import { useQuery } from '@tanstack/react-query'
import api from '@/lib/axios'
import type { Dashboard, AccountBalance, SalaryCountdown, ExpenseSummary, ApiResult } from '@/types/api'

export function useDashboard() {
  return useQuery({
    queryKey: ['dashboard'],
    queryFn: async () => {
      const res = await api.get<ApiResult<Dashboard>>('/dashboard/get-dashboard')
      console.warn(res.data.data)
      return res.data.data
    },
  })
}

export function useDashboardBalances() {
  return useQuery({
    queryKey: ['dashboard', 'balances'],
    queryFn: async () => {
      const res = await api.get<ApiResult<AccountBalance[]>>('/dashboard/balances')
      return res.data.data!
    },
  })
}

export function useSalaryCountdown() {
  return useQuery({
    queryKey: ['dashboard', 'salary-countdown'],
    queryFn: async () => {
      const res = await api.get<ApiResult<SalaryCountdown>>('/dashboard/salary-countdown')
      return res.data.data!
    },
  })
}

export function useDashboardExpenses() {
  return useQuery({
    queryKey: ['dashboard', 'expenses'],
    queryFn: async () => {
      const res = await api.get<ApiResult<ExpenseSummary>>('/dashboard/expenses')
      return res.data.data!
    },
  })
}
