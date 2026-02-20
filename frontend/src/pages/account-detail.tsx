import { useState } from 'react'
import { useParams, useNavigate } from 'react-router'
import { useAccount, useDeleteAccount } from '@/hooks/use-accounts'
import { useAccountTransactions } from '@/hooks/use-transactions'
import { AccountDetailView } from '@/components/accounts/account-detail'
import { AccountForm } from '@/components/accounts/account-form'
import { Skeleton } from '@/components/ui/skeleton'
import { Button } from '@/components/ui/button'
import { ArrowLeft, Pencil, Trash2 } from 'lucide-react'

export function AccountDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { data: account, isLoading: accountLoading } = useAccount(id!)
  const { data: transactions, isLoading: txLoading } = useAccountTransactions(id!)
  const deleteMutation = useDeleteAccount()
  const [editOpen, setEditOpen] = useState(false)

  const handleDelete = () => {
    if (confirm('Delete this account? This cannot be undone.')) {
      deleteMutation.mutate(id!, {
        onSuccess: () => navigate('/accounts'),
      })
    }
  }

  if (accountLoading || txLoading) {
    return (
      <div className="space-y-4">
        <Skeleton className="h-8 w-32" />
        <Skeleton className="h-32 w-full" />
        <Skeleton className="h-64 w-full" />
      </div>
    )
  }

  if (!account) {
    return <p className="text-muted-foreground">Account not found.</p>
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2">
          <Button variant="ghost" size="icon" onClick={() => navigate('/accounts')}>
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <h1 className="text-xl font-bold">{account.name}</h1>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" size="icon" onClick={() => setEditOpen(true)}>
            <Pencil className="h-4 w-4" />
          </Button>
          <Button variant="outline" size="icon" onClick={handleDelete}>
            <Trash2 className="h-4 w-4" />
          </Button>
        </div>
      </div>

      <AccountDetailView account={account} transactions={transactions ?? []} />
      <AccountForm open={editOpen} onOpenChange={setEditOpen} account={account} />
    </div>
  )
}
