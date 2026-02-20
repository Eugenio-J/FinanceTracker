import { Separator } from '@/components/ui/separator'
import { Button } from '@/components/ui/button'
import { TransactionItem } from './transaction-item'
import type { Transaction, PagedResult } from '@/types/api'

interface TransactionListProps {
  data: PagedResult<Transaction> | undefined
  onDelete: (id: string) => void
  onPageChange: (page: number) => void
}

export function TransactionList({ data, onDelete, onPageChange }: TransactionListProps) {
  if (!data?.items.length) {
    return <p className="py-8 text-center text-sm text-muted-foreground">No transactions found.</p>
  }

  return (
    <div>
      <div className="divide-y">
        {data.items.map((tx) => (
          <TransactionItem key={tx.id} transaction={tx} onDelete={onDelete} />
        ))}
      </div>
      {data.totalPages > 1 && (
        <>
          <Separator className="my-2" />
          <div className="flex items-center justify-between">
            <p className="text-xs text-muted-foreground">
              Page {data.pageNumber} of {data.totalPages} ({data.totalCount} total)
            </p>
            <div className="flex gap-2">
              <Button
                variant="outline"
                size="sm"
                disabled={data.pageNumber <= 1}
                onClick={() => onPageChange(data.pageNumber - 1)}
              >
                Previous
              </Button>
              <Button
                variant="outline"
                size="sm"
                disabled={data.pageNumber >= data.totalPages}
                onClick={() => onPageChange(data.pageNumber + 1)}
              >
                Next
              </Button>
            </div>
          </div>
        </>
      )}
    </div>
  )
}
