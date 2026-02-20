import { clsx, type ClassValue } from "clsx"
import { twMerge } from "tailwind-merge"
import { format } from "date-fns"

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

export function formatCurrency(amount: number): string {
  return new Intl.NumberFormat('en-PH', {
    style: 'currency',
    currency: 'PHP',
    minimumFractionDigits: 2,
  }).format(amount)
}

// export function formatDate(date: string | Date): string {
//   return format(new Date(date), 'MMM d, yyyy')
// }


export function formatDate(date: string | Date | null | undefined): string {
  if (!date) return '-'
  const parsedDate = new Date(date)
  return format(parsedDate, 'MMM d, yyyy')
}


export function formatDateTime(date: string | Date): string {
  return format(new Date(date), 'MMM d, yyyy h:mm a')
}
