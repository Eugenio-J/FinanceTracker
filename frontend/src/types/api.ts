// Auth
export interface LoginRequest {
  email: string
  password: string
}

export interface RegisterRequest {
  email: string
  password: string
  firstName: string
  lastName: string
}

export interface AuthResponse {
  token: string
  email: string
  firstName: string
  lastName: string
}

// API Result wrapper (matches backend Result<T>)
export interface ApiResult<T> {
  isSuccess: boolean
  data: T | null
  error: string | null
  statusCode: number
}

export interface ApiResultVoid {
  isSuccess: boolean
  error: string | null
  statusCode: number
}

// Accounts
export interface Account {
  id: string
  name: string
  accountType: string
  currentBalance: number
  createdAt: string
  transactionCount: number
}

export interface CreateAccount {
  name: string
  accountType: string
  initialBalance?: number
}

export interface UpdateAccount {
  name: string
}

// Transactions
export interface Transaction {
  id: string
  accountId: string
  accountName: string
  amount: number
  transactionType: string
  category: string
  description?: string | null
  date: string
  relatedTransactionId?: string | null
  createdAt: string
}

export interface CreateTransaction {
  accountId: string
  amount: number
  transactionType: string
  category: string
  description?: string | null
  date: string
}

export interface TransactionFilter {
  accountId?: string | null
  transactionType?: string | null
  category?: string | null
  startDate?: string | null
  endDate?: string | null
  pageNumber?: number
  pageSize?: number
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
}

// Expenses
export interface Expense {
  id: string
  accountId: string
  accountName: string
  amount: number
  category: string
  description?: string | null
  date: string
  createdAt: string
}

export interface CreateExpense {
  accountId: string
  amount: number
  category: string
  description?: string | null
  date: string
}

export interface ExpenseSummary {
  totalAmount: number
  byCategory: Record<string, number>
  byAccount: Record<string, number>
}

// Salary Cycles
export interface SalaryCycle {
  id: string
  payDate: string
  grossSalary: number
  netSalary: number
  status: string
  createdAt: string
  completedAt?: string | null
  distributions: SalaryDistribution[]
}

export interface CreateSalaryCycle {
  payDate: string
  grossSalary: number
  netSalary: number
  distributions: CreateDistribution[]
}

export interface CreateDistribution {
  targetAccountId: string
  amount: number
  distributionType: string
  orderIndex: number
}

export interface SalaryDistribution {
  id: string
  targetAccountId: string
  targetAccountName: string
  amount: number
  distributionType: string
  isExecuted: boolean
  executedAt?: string | null
}

// Dashboard
export interface Dashboard {
  totalBalance: number
  accountBalances: AccountBalance[]
  monthToDateExpenses: ExpenseSummary
  salaryCountdown: SalaryCountdown
  recentSalaryCycles: SalaryCycleHistory[]
}

export interface AccountBalance {
  id: string
  name: string
  accountType: string
  balance: number
}

export interface SalaryCountdown {
  nextPayDate: string
  daysUntilPayday: number
}

export interface SalaryCycleHistory {
  id: string
  payDate: string
  netSalary: number
  status: string
}

// Enums
export const AccountTypes = ['Payroll', 'Hub', 'Parking', 'Savings', 'CashHolding'] as const
export const TransactionTypes = ['Deposit', 'Withdrawal', 'TransferIn', 'TransferOut'] as const
export const TransactionCategories = ['Salary', 'Transfer', 'Distribution', 'Expense', 'Adjustment'] as const
export const ExpenseCategories = ['Parking', 'Shopee', 'Tiktok',  'Insurance', 'Cash', 'Food', 'Transport', 'Others', 'Bills'] as const
export const DistributionTypes = ['Fixed', 'Percentage', 'Remainder'] as const
export const SalaryCycleStatuses = ['Pending', 'InProgress', 'Completed', 'Failed'] as const
