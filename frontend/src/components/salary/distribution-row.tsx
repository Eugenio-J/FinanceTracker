import { Input } from '@/components/ui/input'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Button } from '@/components/ui/button'
import { Trash2 } from 'lucide-react'
import { DistributionTypes } from '@/types/api'
import type { Account } from '@/types/api'

interface DistributionRowProps {
  index: number
  accounts: Account[]
  targetAccountId: string
  amount: number
  distributionType: string
  onChange: (field: string, value: string | number) => void
  onRemove: () => void
}

export function DistributionRow({
  index,
  accounts,
  targetAccountId,
  amount,
  distributionType,
  onChange,
  onRemove,
}: DistributionRowProps) {
  return (
    <div className="flex items-end gap-2">
      <div className="flex-1 space-y-1">
        <label className="text-xs text-muted-foreground">Account</label>
        <Select value={targetAccountId} onValueChange={(v) => onChange('targetAccountId', v)}>
          <SelectTrigger className="h-9">
            <SelectValue placeholder="Select" />
          </SelectTrigger>
          <SelectContent>
            {accounts.map((a) => (
              <SelectItem key={a.id} value={a.id}>{a.name}</SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>
      <div className="w-24 space-y-1">
        <label className="text-xs text-muted-foreground">Amount</label>
        <Input
          type="number"
          step="0.01"
          className="h-9"
          value={amount}
          onChange={(e) => onChange('amount', parseFloat(e.target.value) || 0)}
        />
      </div>
      <div className="w-28 space-y-1">
        <label className="text-xs text-muted-foreground">Type</label>
        <Select value={distributionType} onValueChange={(v) => onChange('distributionType', v)}>
          <SelectTrigger className="h-9">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            {DistributionTypes.map((t) => (
              <SelectItem key={t} value={t}>{t}</SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>
      <div className="w-8 space-y-1">
        <label className="text-xs text-muted-foreground">#{index + 1}</label>
        <Button type="button" variant="ghost" size="icon" className="h-9 w-9" onClick={onRemove}>
          <Trash2 className="h-3.5 w-3.5" />
        </Button>
      </div>
    </div>
  )
}
