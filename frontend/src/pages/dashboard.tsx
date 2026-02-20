import { useDashboard } from '@/hooks/use-dashboard'
import { BalanceCard } from '@/components/dashboard/balance-card'
import { AccountBalances } from '@/components/dashboard/account-balances'
import { SalaryCountdown } from '@/components/dashboard/salary-countdown'
import { ExpenseSummaryChart } from '@/components/dashboard/expense-summary-chart'

export function DashboardPage() {
  const { data, isLoading } = useDashboard()

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold md:hidden">Dashboard</h1>

      <BalanceCard totalBalance={data?.totalBalance} isLoading={isLoading} />

      <div>
        <h2 className="mb-3 text-sm font-medium text-muted-foreground">Account Balances</h2>
        <AccountBalances balances={data?.accountBalances} isLoading={isLoading} />
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        <SalaryCountdown countdown={data?.salaryCountdown} isLoading={isLoading} />
        <ExpenseSummaryChart summary={data?.monthToDateExpenses} isLoading={isLoading} />
      </div>
    </div>
  )
}
