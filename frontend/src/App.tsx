import { Routes, Route, Navigate } from 'react-router'
import { ProtectedRoute } from '@/components/layout/protected-route'
import { AppLayout } from '@/components/layout/app-layout'
import { LoginPage } from '@/pages/login'
import { RegisterPage } from '@/pages/register'
import { DashboardPage } from '@/pages/dashboard'
import { AccountsPage } from '@/pages/accounts'
import { AccountDetailPage } from '@/pages/account-detail'
import { TransactionsPage } from '@/pages/transactions'
import { ExpensesPage } from '@/pages/expenses'
import { SalaryCyclesPage } from '@/pages/salary-cycles'
import { useSilentRefresh } from '@/hooks/use-auth'

export function App() {
  useSilentRefresh()
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />
      <Route
        element={
          <ProtectedRoute>
            <AppLayout />
          </ProtectedRoute>
        }
      >
        <Route path="/dashboard" element={<DashboardPage />} />
        <Route path="/accounts" element={<AccountsPage />} />
        <Route path="/accounts/:id" element={<AccountDetailPage />} />
        <Route path="/transactions" element={<TransactionsPage />} />
        <Route path="/expenses" element={<ExpensesPage />} />
        <Route path="/salary-cycles" element={<SalaryCyclesPage />} />
      </Route>
      <Route path="*" element={<Navigate to="/login" replace />} />
    </Routes>
  )
}
