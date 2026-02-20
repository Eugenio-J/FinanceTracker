import { ExpenseItem } from './expense-item'
import type { Expense } from '@/types/api'

interface ExpenseListProps {
  expenses: Expense[]
  onDelete: (id: string) => void
}

export function ExpenseList({ expenses, onDelete }: ExpenseListProps) {
  if (!expenses.length) {
    return <p className="py-8 text-center text-sm text-muted-foreground">No expenses found.</p>
  }

  return (
    <div className="divide-y">
      {expenses.map((expense) => (
        <ExpenseItem key={expense.id} expense={expense} onDelete={onDelete} />
      ))}
    </div>
  )
}
