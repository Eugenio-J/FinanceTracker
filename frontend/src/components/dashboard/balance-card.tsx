import { Card, CardContent } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { formatCurrency } from '@/lib/utils'

interface BalanceCardProps {
  totalBalance: number | undefined
  isLoading: boolean
}

export function BalanceCard({ totalBalance, isLoading }: BalanceCardProps) {
  return (
    <Card>
      <CardContent className="pt-6">
        <p className="text-sm text-muted-foreground">Total Balance</p>
        {isLoading ? (
          <Skeleton className="mt-1 h-9 w-48" />
        ) : (
          <p className="text-3xl font-bold tracking-tight">
            {formatCurrency(totalBalance ?? 0)}
          </p>
        )}
      </CardContent>
    </Card>
  )
}
