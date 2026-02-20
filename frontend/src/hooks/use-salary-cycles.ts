import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import api from '@/lib/axios'
import type { SalaryCycle, CreateSalaryCycle, SalaryCountdown, ApiResult } from '@/types/api'
import { toast } from 'sonner'

export function useRecentCycles() {
  return useQuery({
    queryKey: ['salary-cycles'],
    queryFn: async () => {
      const res = await api.get<ApiResult<SalaryCycle[]>>('/salarycycles')
      return res.data.data!
    },
  })
}

export function useNextPayDate() {
  return useQuery({
    queryKey: ['salary-cycles', 'next-payday'],
    queryFn: async () => {
      const res = await api.get<ApiResult<SalaryCountdown>>('/salarycycles/next-payday')
      return res.data.data!
    },
  })
}

export function useCreateCycle() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: CreateSalaryCycle) => {
      const res = await api.post<ApiResult<SalaryCycle>>('/salarycycles', data)
      return res.data
    },
    onSuccess: (result) => {
      if (result.isSuccess) {
        queryClient.invalidateQueries({ queryKey: ['salary-cycles'] })
        queryClient.invalidateQueries({ queryKey: ['dashboard'] })
        toast.success('Salary cycle created!')
      } else {
        toast.error(result.error || 'Failed to create salary cycle')
      }
    },
    onError: () => toast.error('Failed to create salary cycle'),
  })
}

export function useExecuteDistributions() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (cycleId: string) => {
      const res = await api.post<ApiResult<SalaryCycle>>(`/salarycycles/${cycleId}/execute`)
      return res.data
    },
    onSuccess: (result) => {
      if (result.isSuccess) {
        queryClient.invalidateQueries({ queryKey: ['salary-cycles'] })
        queryClient.invalidateQueries({ queryKey: ['accounts'] })
        queryClient.invalidateQueries({ queryKey: ['transactions'] })
        queryClient.invalidateQueries({ queryKey: ['dashboard'] })
        toast.success('Distributions executed!')
      } else {
        toast.error(result.error || 'Failed to execute distributions')
      }
    },
    onError: () => toast.error('Failed to execute distributions'),
  })
}
