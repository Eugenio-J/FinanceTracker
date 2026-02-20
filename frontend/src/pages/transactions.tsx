import { useState } from 'react'
import { useTransactions, useDeleteTransaction } from '@/hooks/use-transactions'
import { useAccounts } from '@/hooks/use-accounts'
import { TransactionList } from '@/components/transactions/transaction-list'
import { TransactionFilters } from '@/components/transactions/transaction-filters'
import { TransactionForm } from '@/components/transactions/transaction-form'
import { Card, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { Plus, SlidersHorizontal } from 'lucide-react'
import type { TransactionFilter } from '@/types/api'

export function TransactionsPage() {
  const [filter, setFilter] = useState<TransactionFilter>({ pageNumber: 1, pageSize: 20 })
  const [showFilters, setShowFilters] = useState(false)
  const [formOpen, setFormOpen] = useState(false)

  const { data, isLoading } = useTransactions(filter)
  const { data: accounts } = useAccounts()
  const deleteMutation = useDeleteTransaction()

  const handleDelete = (id: string) => {
    if (confirm('Delete this transaction?')) {
      deleteMutation.mutate(id)
    }
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Transactions</h1>
        <div className="flex gap-2">
          <Button
            variant="outline"
            size="sm"
            onClick={() => setShowFilters(!showFilters)}
          >
            <SlidersHorizontal className="mr-1 h-4 w-4" />
            Filter
          </Button>
          <Button size="sm" onClick={() => setFormOpen(true)}>
            <Plus className="mr-1 h-4 w-4" />
            Add
          </Button>
        </div>
      </div>

      {showFilters && (
        <Card>
          <CardContent className="pt-4">
            <TransactionFilters
              filter={filter}
              onFilterChange={setFilter}
              accounts={accounts ?? []}
            />
          </CardContent>
        </Card>
      )}

      {isLoading ? (
        <div className="space-y-3">
          {[1, 2, 3, 4, 5].map((i) => (
            <Skeleton key={i} className="h-16 w-full" />
          ))}
        </div>
      ) : (
        <TransactionList
          data={data}
          onDelete={handleDelete}
          onPageChange={(page) => setFilter({ ...filter, pageNumber: page })}
        />
      )}

      <TransactionForm open={formOpen} onOpenChange={setFormOpen} />
    </div>
  )
}
