import { Navigate } from 'react-router-dom'
import { useAuth } from '@/hooks/useAuth'

const ADMIN_ROLES = ['Admin', 'SuperAdmin']

interface AdminRouteProps {
  children: React.ReactNode
}

export default function AdminRoute({ children }: AdminRouteProps) {
  const { user, isLoading } = useAuth()

  if (isLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <span className="text-muted-foreground text-sm">Loading...</span>
      </div>
    )
  }

  if (!user) {
    return <Navigate to="/login" replace />
  }

  if (!user.roles.some((r) => ADMIN_ROLES.includes(r))) {
    return <Navigate to="/dashboard" replace />
  }

  return <>{children}</>
}
