import { Navigate } from 'react-router-dom'
import { useAuth } from '@/hooks/useAuth'
import AdminSidebar from '@/components/layout/AdminSidebar'

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

  return (
    <div className="min-h-screen bg-background lg:grid lg:grid-cols-[240px_minmax(0,1fr)]">
      <AdminSidebar />
      <main className="min-w-0">{children}</main>
    </div>
  )
}
