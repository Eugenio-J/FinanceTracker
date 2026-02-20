import { Card, CardContent } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Skeleton } from '@/components/ui/skeleton'
import { formatCurrency } from '@/lib/utils'
import type { AccountBalance } from '@/types/api'

interface AccountBalancesProps {
  balances: AccountBalance[] | undefined
  isLoading: boolean
}

export function AccountBalances({ balances, isLoading }: AccountBalancesProps) {
  if (isLoading) {
    return (
      <div className="flex gap-3 overflow-x-auto pb-2">
        {[1, 2, 3].map((i) => (
          <Skeleton key={i} className="h-24 min-w-[180px] rounded-lg" />
        ))}
      </div>
    )
  }

  if (!balances?.length) {
    return <p className="text-sm text-muted-foreground">No accounts yet.</p>
  }

  return (
    <div className="flex gap-3 overflow-x-auto pb-2 -mx-1 px-1">
      {balances.map((account) => (
        <Card key={account.id} className="min-w-[180px] shrink-0">
          <CardContent className="p-4">
            <div className="flex items-center justify-between gap-2">
              <p className="text-sm font-medium truncate">{account.name}</p>
              <Badge variant="secondary" className="text-xs shrink-0">
                {account.accountType}
              </Badge>
            </div>
            <p className="mt-2 text-lg font-semibold">{formatCurrency(account.balance)}</p>
          </CardContent>
        </Card>
      ))}
    </div>
  )
}
