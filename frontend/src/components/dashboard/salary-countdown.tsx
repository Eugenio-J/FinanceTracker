import { Card, CardContent } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { formatDate } from '@/lib/utils'
import type { SalaryCountdown as SalaryCountdownType } from '@/types/api'

interface SalaryCountdownProps {
  countdown: SalaryCountdownType | undefined
  isLoading: boolean
}

export function SalaryCountdown({ countdown, isLoading }: SalaryCountdownProps) {
  if (isLoading) {
    return (
      <Card>
        <CardContent className="pt-6">
          <Skeleton className="h-20 w-full" />
        </CardContent>
      </Card>
    )
  }

  if (!countdown) {
    return (
      <Card>
        <CardContent className="pt-6">
          <p className="text-sm text-muted-foreground">No upcoming payday set</p>
        </CardContent>
      </Card>
    )
  }

  const percentage = Math.max(0, Math.min(100, ((30 - countdown.daysUntilPayday) / 30) * 100))

  return (
    <Card>
      <CardContent className="pt-6">
        <p className="text-sm text-muted-foreground">Next Payday</p>
        <div className="mt-2 flex items-center gap-4">
          <div className="relative flex h-20 w-20 items-center justify-center">
            <svg className="h-20 w-20 -rotate-90" viewBox="0 0 36 36">
              <path
                className="text-muted"
                d="M18 2.0845a15.9155 15.9155 0 0 1 0 31.831 15.9155 15.9155 0 0 1 0-31.831"
                fill="none"
                stroke="currentColor"
                strokeWidth="3"
              />
              <path
                className="text-primary"
                d="M18 2.0845a15.9155 15.9155 0 0 1 0 31.831 15.9155 15.9155 0 0 1 0-31.831"
                fill="none"
                stroke="currentColor"
                strokeWidth="3"
                strokeDasharray={`${percentage}, 100`}
              />
            </svg>
            <span className="absolute text-xl font-bold">{countdown.daysUntilPayday}</span>
          </div>
          <div>
            <p className="text-lg font-semibold">
              {countdown.daysUntilPayday === 0
                ? 'Payday!'
                : countdown.daysUntilPayday === 1
                  ? '1 day left'
                  : `${countdown.daysUntilPayday} days left`}
            </p>
            <p className="text-sm text-muted-foreground">
              {formatDate(countdown.nextPayDate)}
            </p>
          </div>
        </div>
      </CardContent>
    </Card>
  )
}
