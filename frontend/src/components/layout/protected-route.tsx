import { Navigate } from 'react-router'
import { useAuthStore } from '@/stores/auth-store'
import { Skeleton } from '../ui/skeleton'

export function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const token = useAuthStore((s) => s.token)
  const hasHydrated = useAuthStore((s) => s._hasHydrated)
  console.log('hasHydrated:', hasHydrated, 'token:', token)

  if (!hasHydrated) {
    return null
  }

  if (!token) {
    return <Navigate to="/login" replace />
  }

  return <>{children}</>
}
