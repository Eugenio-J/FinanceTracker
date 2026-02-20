import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { formatCurrency } from '@/lib/utils'
import type { ExpenseSummary } from '@/types/api'

interface MonthlySummaryProps {
  summary: ExpenseSummary | undefined
  isLoading: boolean
}

export function MonthlySummary({ summary, isLoading }: MonthlySummaryProps) {
  if (isLoading) {
    return <Skeleton className="h-64 w-full" />
  }

  if (!summary || summary.totalAmount === 0) {
    return (
      <Card>
        <CardContent className="pt-6">
          <p className="text-sm text-muted-foreground">No expenses this month.</p>
        </CardContent>
      </Card>
    )
  }

  const categories = Object.entries(summary.byCategory).sort((a, b) => b[1] - a[1])

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-base">Monthly Summary</CardTitle>
        <p className="text-2xl font-bold">{formatCurrency(summary.totalAmount)}</p>
      </CardHeader>
      <CardContent className="space-y-3">
        {categories.map(([category, amount]) => {
          const percentage = (amount / summary.totalAmount) * 100
          return (
            <div key={category}>
              <div className="flex items-center justify-between text-sm">
                <span>{category}</span>
                <span className="font-medium">{formatCurrency(amount)}</span>
              </div>
              <div className="mt-1 h-2 rounded-full bg-muted">
                <div
                  className="h-2 rounded-full bg-primary"
                  style={{ width: `${percentage}%` }}
                />
              </div>
            </div>
          )
        })}

        {Object.keys(summary.byAccount).length > 0 && (
          <>
            <div className="pt-2 border-t">
              <p className="text-sm font-medium mb-2">By Account</p>
              {Object.entries(summary.byAccount).map(([account, amount]) => (
                <div key={account} className="flex items-center justify-between text-sm py-1">
                  <span className="text-muted-foreground">{account}</span>
                  <span className="font-medium">{formatCurrency(amount)}</span>
                </div>
              ))}
            </div>
          </>
        )}
      </CardContent>
    </Card>
  )
}
