import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod/v4'
import { Sheet, SheetContent, SheetHeader, SheetTitle } from '@/components/ui/sheet'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { useCreateTransaction } from '@/hooks/use-transactions'
import { useAccounts } from '@/hooks/use-accounts'
import { TransactionTypes, TransactionCategories } from '@/types/api'
import { format } from 'date-fns'

const schema = z.object({
  accountId: z.string().min(1, 'Account is required'),
  amount: z.string().min(1, 'Amount is required'),
  transactionType: z.string().min(1, 'Type is required'),
  category: z.string().min(1, 'Category is required'),
  description: z.string().optional(),
  date: z.string().min(1, 'Date is required'),
})

type FormValues = z.infer<typeof schema>

interface TransactionFormProps {
  open: boolean
  onOpenChange: (open: boolean) => void
}

export function TransactionForm({ open, onOpenChange }: TransactionFormProps) {
  const createMutation = useCreateTransaction()
  const { data: accounts } = useAccounts()

  const form = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      accountId: '',
      amount: '',
      transactionType: '',
      category: '',
      description: '',
      date: format(new Date(), 'yyyy-MM-dd'),
    },
  })

  const onSubmit = (data: FormValues) => {
    createMutation.mutate(
      {
        accountId: data.accountId,
        amount: parseFloat(data.amount),
        transactionType: data.transactionType,
        category: data.category,
        description: data.description || undefined,
        date: new Date(data.date).toISOString(),
      },
      {
        onSuccess: () => {
          onOpenChange(false)
          form.reset()
        },
      }
    )
  }

  return (
    <Sheet open={open} onOpenChange={onOpenChange}>
      <SheetContent side="bottom" className="rounded-t-2xl max-h-[85vh] overflow-y-auto">
        <SheetHeader>
          <SheetTitle>New Transaction</SheetTitle>
        </SheetHeader>
        <form onSubmit={form.handleSubmit(onSubmit)} className="mt-4 space-y-4">
          <div className="space-y-2">
            <Label>Account</Label>
            <Select onValueChange={(v) => form.setValue('accountId', v)}>
              <SelectTrigger>
                <SelectValue placeholder="Select account" />
              </SelectTrigger>
              <SelectContent>
                {accounts?.map((a) => (
                  <SelectItem key={a.id} value={a.id}>{a.name}</SelectItem>
                ))}
              </SelectContent>
            </Select>
            {form.formState.errors.accountId && (
              <p className="text-sm text-destructive">{form.formState.errors.accountId.message}</p>
            )}
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label>Type</Label>
              <Select onValueChange={(v) => form.setValue('transactionType', v)}>
                <SelectTrigger>
                  <SelectValue placeholder="Select type" />
                </SelectTrigger>
                <SelectContent>
                  {TransactionTypes.map((t) => (
                    <SelectItem key={t} value={t}>{t}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label>Category</Label>
              <Select onValueChange={(v) => form.setValue('category', v)}>
                <SelectTrigger>
                  <SelectValue placeholder="Select" />
                </SelectTrigger>
                <SelectContent>
                  {TransactionCategories.map((c) => (
                    <SelectItem key={c} value={c}>{c}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label>Amount</Label>
              <Input type="number" step="0.01" {...form.register('amount')} />
              {form.formState.errors.amount && (
                <p className="text-sm text-destructive">{form.formState.errors.amount.message}</p>
              )}
            </div>
            <div className="space-y-2">
              <Label>Date</Label>
              <Input type="date" {...form.register('date')} />
            </div>
          </div>

          <div className="space-y-2">
            <Label>Description (optional)</Label>
            <Input {...form.register('description')} placeholder="Description" />
          </div>

          <Button type="submit" className="w-full" disabled={createMutation.isPending}>
            {createMutation.isPending ? 'Creating...' : 'Create Transaction'}
          </Button>
        </form>
      </SheetContent>
    </Sheet>
  )
}
