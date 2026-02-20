import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Button } from '@/components/ui/button'
import { TransactionTypes, TransactionCategories } from '@/types/api'
import type { Account, TransactionFilter } from '@/types/api'
import { X } from 'lucide-react'

interface TransactionFiltersProps {
  filter: TransactionFilter
  onFilterChange: (filter: TransactionFilter) => void
  accounts: Account[]
}

export function TransactionFilters({ filter, onFilterChange, accounts }: TransactionFiltersProps) {
  const hasFilters = filter.accountId || filter.transactionType || filter.category || filter.startDate || filter.endDate

  return (
    <div className="space-y-3">
      <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
        <div>
          <Label className="text-xs">Account</Label>
          <Select
            value={filter.accountId ?? 'all'}
            onValueChange={(v) => onFilterChange({ ...filter, accountId: v === 'all' ? undefined : v, pageNumber: 1 })}
          >
            <SelectTrigger className="h-9">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All</SelectItem>
              {accounts.map((a) => (
                <SelectItem key={a.id} value={a.id}>{a.name}</SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
        <div>
          <Label className="text-xs">Type</Label>
          <Select
            value={filter.transactionType ?? 'all'}
            onValueChange={(v) => onFilterChange({ ...filter, transactionType: v === 'all' ? undefined : v, pageNumber: 1 })}
          >
            <SelectTrigger className="h-9">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All</SelectItem>
              {TransactionTypes.map((t) => (
                <SelectItem key={t} value={t}>{t}</SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
        <div>
          <Label className="text-xs">Category</Label>
          <Select
            value={filter.category ?? 'all'}
            onValueChange={(v) => onFilterChange({ ...filter, category: v === 'all' ? undefined : v, pageNumber: 1 })}
          >
            <SelectTrigger className="h-9">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All</SelectItem>
              {TransactionCategories.map((c) => (
                <SelectItem key={c} value={c}>{c}</SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
        <div className="flex items-end">
          {hasFilters && (
            <Button
              variant="ghost"
              size="sm"
              onClick={() => onFilterChange({ pageNumber: 1, pageSize: 20 })}
            >
              <X className="mr-1 h-3 w-3" /> Clear
            </Button>
          )}
        </div>
      </div>
      <div className="grid grid-cols-2 gap-3">
        <div>
          <Label className="text-xs">From</Label>
          <Input
            type="date"
            className="h-9"
            value={filter.startDate ?? ''}
            onChange={(e) => onFilterChange({ ...filter, startDate: e.target.value || undefined, pageNumber: 1 })}
          />
        </div>
        <div>
          <Label className="text-xs">To</Label>
          <Input
            type="date"
            className="h-9"
            value={filter.endDate ?? ''}
            onChange={(e) => onFilterChange({ ...filter, endDate: e.target.value || undefined, pageNumber: 1 })}
          />
        </div>
      </div>
    </div>
  )
}
