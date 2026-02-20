import { useState } from 'react'
import { useExpenses, useDeleteExpense, useMonthlySummary } from '@/hooks/use-expenses'
import { ExpenseList } from '@/components/expenses/expense-list'
import { ExpenseForm } from '@/components/expenses/expense-form'
import { MonthlySummary } from '@/components/expenses/monthly-summary'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { Plus } from 'lucide-react'

export function ExpensesPage() {
  const [formOpen, setFormOpen] = useState(false)
  const now = new Date()

  const { data: expenses, isLoading } = useExpenses()
  const { data: summary, isLoading: summaryLoading } = useMonthlySummary(
    now.getFullYear(),
    now.getMonth() + 1
  )
  const deleteMutation = useDeleteExpense()

  const handleDelete = (id: string) => {
    if (confirm('Delete this expense?')) {
      deleteMutation.mutate(id)
    }
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Expenses</h1>
        <Button size="sm" onClick={() => setFormOpen(true)}>
          <Plus className="mr-1 h-4 w-4" />
          Add
        </Button>
      </div>

      <Tabs defaultValue="list">
        <TabsList className="grid w-full grid-cols-2">
          <TabsTrigger value="list">List</TabsTrigger>
          <TabsTrigger value="summary">Summary</TabsTrigger>
        </TabsList>
        <TabsContent value="list" className="mt-4">
          {isLoading ? (
            <div className="space-y-3">
              {[1, 2, 3, 4].map((i) => (
                <Skeleton key={i} className="h-16 w-full" />
              ))}
            </div>
          ) : (
            <ExpenseList expenses={expenses ?? []} onDelete={handleDelete} />
          )}
        </TabsContent>
        <TabsContent value="summary" className="mt-4">
          <MonthlySummary summary={summary} isLoading={summaryLoading} />
        </TabsContent>
      </Tabs>

      <ExpenseForm open={formOpen} onOpenChange={setFormOpen} />
    </div>
  )
}
