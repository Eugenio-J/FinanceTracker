import { useState } from 'react'
import { useRecentCycles, useExecuteDistributions, useNextPayDate } from '@/hooks/use-salary-cycles'
import { SalaryCycleList } from '@/components/salary/salary-cycle-list'
import { CreateCycleForm } from '@/components/salary/create-cycle-form'
import { SalaryCountdown } from '@/components/dashboard/salary-countdown'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { Plus } from 'lucide-react'

export function SalaryCyclesPage() {
  const [formOpen, setFormOpen] = useState(false)
  const { data: cycles, isLoading } = useRecentCycles()
  const { data: countdown, isLoading: countdownLoading } = useNextPayDate()
  const executeMutation = useExecuteDistributions()

  const handleExecute = (cycleId: string) => {
    if (confirm('Execute distributions for this salary cycle?')) {
      executeMutation.mutate(cycleId)
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Salary Cycles</h1>
        <Button size="sm" onClick={() => setFormOpen(true)}>
          <Plus className="mr-1 h-4 w-4" />
          New Cycle
        </Button>
      </div>

      <SalaryCountdown countdown={countdown} isLoading={countdownLoading} />

      {isLoading ? (
        <div className="space-y-4">
          {[1, 2, 3].map((i) => (
            <Skeleton key={i} className="h-32 w-full" />
          ))}
        </div>
      ) : (
        <SalaryCycleList
          cycles={cycles ?? []}
          onExecute={handleExecute}
          isExecuting={executeMutation.isPending}
        />
      )}

      <CreateCycleForm open={formOpen} onOpenChange={setFormOpen} />
    </div>
  )
}
