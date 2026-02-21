import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod/v4'
import { Sheet, SheetContent, SheetHeader, SheetTitle } from '@/components/ui/sheet'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Separator } from '@/components/ui/separator'
import { DistributionRow } from './distribution-row'
import { useCreateCycle } from '@/hooks/use-salary-cycles'
import { useAccounts } from '@/hooks/use-accounts'
import { Plus } from 'lucide-react'
import { format } from 'date-fns'
import { philippineNow } from '@/lib/utils'
import type { CreateDistribution } from '@/types/api'

const schema = z.object({
  payDate: z.string().min(1, 'Pay date is required'),
  grossSalary: z.string().min(1, 'Gross salary is required'),
  netSalary: z.string().min(1, 'Net salary is required'),
})

type FormValues = z.infer<typeof schema>

interface CreateCycleFormProps {
  open: boolean
  onOpenChange: (open: boolean) => void
}

interface DistRow {
  targetAccountId: string
  amount: number
  distributionType: string
}

export function CreateCycleForm({ open, onOpenChange }: CreateCycleFormProps) {
  const createMutation = useCreateCycle()
  const { data: accounts } = useAccounts(open)
  const [distributions, setDistributions] = useState<DistRow[]>([])

  const form = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      payDate: format(philippineNow(), 'yyyy-MM-dd'),
      grossSalary: '',
      netSalary: '',
    },
  })

  const addDistribution = () => {
    setDistributions([...distributions, { targetAccountId: '', amount: 0, distributionType: 'Fixed' }])
  }

  const updateDistribution = (index: number, field: string, value: string | number) => {
    const updated = [...distributions]
    updated[index] = { ...updated[index], [field]: value }
    setDistributions(updated)
  }

  const removeDistribution = (index: number) => {
    setDistributions(distributions.filter((_, i) => i !== index))
  }

  const onSubmit = (data: FormValues) => {
    const dists: CreateDistribution[] = distributions.map((d, i) => ({
      targetAccountId: d.targetAccountId,
      amount: d.amount,
      distributionType: d.distributionType,
      orderIndex: i + 1,
    }))

    createMutation.mutate(
      {
        payDate: new Date(data.payDate).toISOString(),
        grossSalary: parseFloat(data.grossSalary),
        netSalary: parseFloat(data.netSalary),
        distributions: dists,
      },
      {
        onSuccess: () => {
          onOpenChange(false)
          form.reset()
          setDistributions([])
        },
      }
    )
  }

  return (
    <Sheet open={open} onOpenChange={onOpenChange}>
      <SheetContent side="bottom" className="rounded-t-2xl max-h-[90vh] overflow-y-auto">
        <SheetHeader>
          <SheetTitle>New Salary Cycle</SheetTitle>
        </SheetHeader>
        <form onSubmit={form.handleSubmit(onSubmit)} className="mt-4 space-y-4">
          <div className="space-y-2">
            <Label>Pay Date</Label>
            <Input type="date" {...form.register('payDate')} />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label>Gross Salary</Label>
              <Input type="number" step="0.01" {...form.register('grossSalary')} />
            </div>
            <div className="space-y-2">
              <Label>Net Salary</Label>
              <Input type="number" step="0.01" {...form.register('netSalary')} />
            </div>
          </div>

          <Separator />

          <div className="space-y-3">
            <div className="flex items-center justify-between">
              <Label>Distributions</Label>
              <Button type="button" variant="outline" size="sm" onClick={addDistribution}>
                <Plus className="mr-1 h-3 w-3" /> Add
              </Button>
            </div>
            {distributions.map((dist, index) => (
              <DistributionRow
                key={index}
                index={index}
                accounts={accounts ?? []}
                targetAccountId={dist.targetAccountId}
                amount={dist.amount}
                distributionType={dist.distributionType}
                onChange={(field, value) => updateDistribution(index, field, value)}
                onRemove={() => removeDistribution(index)}
              />
            ))}
            {distributions.length === 0 && (
              <p className="text-sm text-muted-foreground">No distributions added yet.</p>
            )}
          </div>

          <Button type="submit" className="w-full" disabled={createMutation.isPending}>
            {createMutation.isPending ? 'Creating...' : 'Create Salary Cycle'}
          </Button>
        </form>
      </SheetContent>
    </Sheet>
  )
}
