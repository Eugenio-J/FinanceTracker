import { Card, CardContent } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Separator } from '@/components/ui/separator'
import { formatCurrency, formatDate } from '@/lib/utils'
import { Play } from 'lucide-react'
import type { SalaryCycle } from '@/types/api'

interface SalaryCycleListProps {
  cycles: SalaryCycle[]
  onExecute: (cycleId: string) => void
  isExecuting: boolean
}

function statusVariant(status: string): 'default' | 'secondary' | 'destructive' | 'outline' {
  switch (status) {
    case 'Completed': return 'default'
    case 'Pending': return 'secondary'
    case 'InProgress': return 'outline'
    case 'Failed': return 'destructive'
    default: return 'secondary'
  }
}

export function SalaryCycleList({ cycles, onExecute, isExecuting }: SalaryCycleListProps) {
  if (!cycles.length) {
    return <p className="py-8 text-center text-sm text-muted-foreground">No salary cycles yet.</p>
  }

  return (
    <div className="space-y-4">
      {cycles.map((cycle) => (
        <Card key={cycle.id}>
          <CardContent className="p-4">
            <div className="flex items-start justify-between">
              <div>
                <p className="font-medium">{formatDate(cycle.payDate)}</p>
                <p className="text-sm text-muted-foreground">
                  Net: {formatCurrency(cycle.netSalary)} &middot; Gross: {formatCurrency(cycle.grossSalary)}
                </p>
              </div>
              <div className="flex items-center gap-2">
                <Badge variant={statusVariant(cycle.status)}>{cycle.status}</Badge>
                {cycle.status === 'Pending' && (
                  <Button
                    size="sm"
                    variant="outline"
                    onClick={() => onExecute(cycle.id)}
                    disabled={isExecuting}
                  >
                    <Play className="mr-1 h-3 w-3" />
                    Execute
                  </Button>
                )}
              </div>
            </div>

            {cycle.distributions.length > 0 && (
              <>
                <Separator className="my-3" />
                <div className="space-y-2">
                  {cycle.distributions.map((dist) => (
                    <div key={dist.id} className="flex items-center justify-between text-sm">
                      <div className="flex items-center gap-2">
                        <span className={dist.isExecuted ? '' : 'text-muted-foreground'}>
                          {dist.targetAccountName}
                        </span>
                        <Badge variant="outline" className="text-xs">
                          {dist.distributionType}
                        </Badge>
                      </div>
                      <div className="flex items-center gap-2">
                        <span className="font-medium">{formatCurrency(dist.amount)}</span>
                        {dist.isExecuted && (
                          <span className="text-xs text-green-600">Done</span>
                        )}
                      </div>
                    </div>
                  ))}
                </div>
              </>
            )}
          </CardContent>
        </Card>
      ))}
    </div>
  )
}
