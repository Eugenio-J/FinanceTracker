import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Separator } from '@/components/ui/separator'
import { formatCurrency, formatDate } from '@/lib/utils'
import type { Account, Transaction } from '@/types/api'

interface AccountDetailViewProps {
  account: Account
  transactions: Transaction[]
}

export function AccountDetailView({ account, transactions }: AccountDetailViewProps) {
  return (
    <div className="space-y-6">
      <Card>
        <CardContent className="pt-6">
          <div className="flex items-start justify-between">
            <div>
              <p className="text-sm text-muted-foreground">Balance</p>
              <p className="text-3xl font-bold">{formatCurrency(account.currentBalance)}</p>
            </div>
            <Badge variant="secondary">{account.accountType}</Badge>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle className="text-base">Recent Transactions</CardTitle>
        </CardHeader>
        <CardContent>
          {transactions.length === 0 ? (
            <p className="text-sm text-muted-foreground">No transactions yet.</p>
          ) : (
            <div className="space-y-1">
              {transactions.slice(0, 20).map((tx) => (
                <div key={tx.id}>
                  <div className="flex items-center justify-between py-2">
                    <div className="min-w-0">
                      <p className="text-sm font-medium truncate">
                        {tx.description || tx.category}
                      </p>
                      <p className="text-xs text-muted-foreground">
                        {formatDate(tx.date)} &middot; {tx.transactionType}
                      </p>
                    </div>
                    <p
                      className={`text-sm font-semibold shrink-0 ml-3 ${
                        tx.transactionType === 'Deposit' || tx.transactionType === 'TransferIn'
                          ? 'text-green-600'
                          : 'text-red-600'
                      }`}
                    >
                      {tx.transactionType === 'Deposit' || tx.transactionType === 'TransferIn'
                        ? '+'
                        : '-'}
                      {formatCurrency(tx.amount)}
                    </p>
                  </div>
                  <Separator />
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  )
}
