import { Navigate } from 'react-router'
import { useAuthStore } from '@/stores/auth-store'

export function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const token = useAuthStore((s) => s.token)
  const isInitialized = useAuthStore((s) => s.isInitialized)

  if (!isInitialized) {
    return null
  }

  if (!token) {
    return <Navigate to="/login" replace />
  }

  return <>{children}</>
}
