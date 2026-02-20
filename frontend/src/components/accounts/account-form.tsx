import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod/v4'
import { Sheet, SheetContent, SheetHeader, SheetTitle } from '@/components/ui/sheet'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { useCreateAccount, useUpdateAccount } from '@/hooks/use-accounts'
import { AccountTypes } from '@/types/api'
import type { Account } from '@/types/api'

const createSchema = z.object({
  name: z.string().min(1, 'Name is required'),
  accountType: z.string().min(1, 'Account type is required'),
  initialBalance: z.string().optional(),
})

const updateSchema = z.object({
  name: z.string().min(1, 'Name is required'),
})

type CreateForm = z.infer<typeof createSchema>
type UpdateForm = z.infer<typeof updateSchema>

interface AccountFormProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  account?: Account | null
}

export function AccountForm({ open, onOpenChange, account }: AccountFormProps) {
  const isEditing = !!account
  const createMutation = useCreateAccount()
  const updateMutation = useUpdateAccount()

  const createForm = useForm<CreateForm>({
    resolver: zodResolver(createSchema),
    defaultValues: { name: '', accountType: '', initialBalance: '0' },
  })

  const updateForm = useForm<UpdateForm>({
    resolver: zodResolver(updateSchema),
    values: account ? { name: account.name } : { name: '' },
  })

  const onCreateSubmit = (data: CreateForm) => {
    createMutation.mutate(
      { name: data.name, accountType: data.accountType, initialBalance: parseFloat(data.initialBalance || '0') },
      { onSuccess: () => { onOpenChange(false); createForm.reset() } }
    )
  }

  const onUpdateSubmit = (data: UpdateForm) => {
    if (account) {
      updateMutation.mutate(
        { id: account.id, data: { name: data.name } },
        { onSuccess: () => { onOpenChange(false); updateForm.reset() } }
      )
    }
  }

  return (
    <Sheet open={open} onOpenChange={onOpenChange}>
      <SheetContent side="bottom" className="rounded-t-2xl">
        <SheetHeader>
          <SheetTitle>{isEditing ? 'Edit Account' : 'Create Account'}</SheetTitle>
        </SheetHeader>
        {isEditing ? (
          <form onSubmit={updateForm.handleSubmit(onUpdateSubmit)} className="mt-4 space-y-4">
            <div className="space-y-2">
              <Label>Name</Label>
              <Input {...updateForm.register('name')} placeholder="Account name" />
              {updateForm.formState.errors.name && (
                <p className="text-sm text-destructive">{updateForm.formState.errors.name.message}</p>
              )}
            </div>
            <Button type="submit" className="w-full" disabled={updateMutation.isPending}>
              Update Account
            </Button>
          </form>
        ) : (
          <form onSubmit={createForm.handleSubmit(onCreateSubmit)} className="mt-4 space-y-4">
            <div className="space-y-2">
              <Label>Name</Label>
              <Input {...createForm.register('name')} placeholder="Account name" />
              {createForm.formState.errors.name && (
                <p className="text-sm text-destructive">{createForm.formState.errors.name.message}</p>
              )}
            </div>
            <div className="space-y-2">
              <Label>Account Type</Label>
              <Select onValueChange={(value) => createForm.setValue('accountType', value)} defaultValue="">
                <SelectTrigger>
                  <SelectValue placeholder="Select type" />
                </SelectTrigger>
                <SelectContent>
                  {AccountTypes.map((type) => (
                    <SelectItem key={type} value={type}>{type}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
              {createForm.formState.errors.accountType && (
                <p className="text-sm text-destructive">{createForm.formState.errors.accountType.message}</p>
              )}
            </div>
            <div className="space-y-2">
              <Label>Initial Balance</Label>
              <Input type="number" step="0.01" {...createForm.register('initialBalance')} placeholder="0.00" />
            </div>
            <Button type="submit" className="w-full" disabled={createMutation.isPending}>
              Create Account
            </Button>
          </form>
        )}
      </SheetContent>
    </Sheet>
  )
}
