import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { formatCurrency, formatDate } from '@/lib/utils'
import { Trash2 } from 'lucide-react'
import type { Transaction } from '@/types/api'

interface TransactionItemProps {
  transaction: Transaction
  onDelete: (id: string) => void
}

export function TransactionItem({ transaction: tx, onDelete }: TransactionItemProps) {
  const isIncome = tx.transactionType === 'Deposit' || tx.transactionType === 'TransferIn'

  return (
    <div className="flex items-center gap-3 py-3">
      <div className="min-w-0 flex-1">
        <div className="flex items-center gap-2">
          <p className="text-sm font-medium truncate">
            {tx.description || tx.category}
          </p>
          <Badge variant="outline" className="text-xs shrink-0">
            {tx.category}
          </Badge>
        </div>
        <p className="text-xs text-muted-foreground">
          {tx.accountName} &middot; {formatDate(tx.date)}
        </p>
      </div>
      <p className={`text-sm font-semibold shrink-0 ${isIncome ? 'text-green-600' : 'text-red-600'}`}>
        {isIncome ? '+' : '-'}{formatCurrency(tx.amount)}
      </p>
      <Button variant="ghost" size="icon" className="h-8 w-8 shrink-0" onClick={() => onDelete(tx.id)}>
        <Trash2 className="h-3.5 w-3.5" />
      </Button>
    </div>
  )
}
