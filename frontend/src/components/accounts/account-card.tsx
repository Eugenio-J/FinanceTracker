import { useNavigate } from 'react-router'
import { Card, CardContent } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { formatCurrency } from '@/lib/utils'
import type { Account } from '@/types/api'

interface AccountCardProps {
  account: Account
}

export function AccountCard({ account }: AccountCardProps) {
  const navigate = useNavigate()

  return (
    <Card
      className="cursor-pointer transition-shadow hover:shadow-md active:scale-[0.98]"
      onClick={() => navigate(`/accounts/${account.id}`)}
    >
      <CardContent className="p-4">
        <div className="flex items-start justify-between gap-2">
          <div className="min-w-0">
            <p className="font-medium truncate">{account.name}</p>
            <p className="text-2xl font-bold mt-1">{formatCurrency(account.currentBalance)}</p>
          </div>
          <Badge variant="secondary">{account.accountType}</Badge>
        </div>
        <p className="mt-2 text-xs text-muted-foreground">
          {account.transactionCount} transaction{account.transactionCount !== 1 ? 's' : ''}
        </p>
      </CardContent>
    </Card>
  )
}
