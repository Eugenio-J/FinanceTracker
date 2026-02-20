import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { formatCurrency, formatDate } from '@/lib/utils'
import { Trash2 } from 'lucide-react'
import type { Expense } from '@/types/api'

interface ExpenseItemProps {
  expense: Expense
  onDelete: (id: string) => void
}

export function ExpenseItem({ expense, onDelete }: ExpenseItemProps) {
  return (
    <div className="flex items-center gap-3 py-3">
      <div className="min-w-0 flex-1">
        <div className="flex items-center gap-2">
          <p className="text-sm font-medium truncate">
            {expense.description || expense.category}
          </p>
          <Badge variant="outline" className="text-xs shrink-0">
            {expense.category}
          </Badge>
        </div>
        <p className="text-xs text-muted-foreground">
          {expense.accountName} &middot; {formatDate(expense.date)}
        </p>
      </div>
      <p className="text-sm font-semibold text-red-600 shrink-0">
        -{formatCurrency(expense.amount)}
      </p>
      <Button variant="ghost" size="icon" className="h-8 w-8 shrink-0" onClick={() => onDelete(expense.id)}>
        <Trash2 className="h-3.5 w-3.5" />
      </Button>
    </div>
  )
}
