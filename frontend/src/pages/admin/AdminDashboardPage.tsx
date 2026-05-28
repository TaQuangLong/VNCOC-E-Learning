import { Link } from 'react-router-dom'
import { useAdminOverview } from '@/features/reports/api'
import OverviewCards from '@/features/reports/OverviewCards'
import UserAvatarMenu from '@/components/layout/UserAvatarMenu'

export default function AdminDashboardPage() {
  const { data, isLoading, isError } = useAdminOverview()

  return (
    <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
      <div className="mb-6 flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
        <h1 className="text-2xl font-bold">Admin Dashboard</h1>
        <div className="flex items-center gap-3">
          <div className="flex gap-2 text-sm">
            <Link
              to="/admin/courses"
              className="rounded-md border px-3 py-1.5 hover:bg-muted"
            >
              Manage Courses
            </Link>
          </div>
          <UserAvatarMenu />
        </div>
      </div>

      {isLoading && (
        <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-5">
          {Array.from({ length: 5 }).map((_, i) => (
            <div
              key={i}
              className="h-24 animate-pulse rounded-lg border bg-muted"
            />
          ))}
        </div>
      )}

      {isError && (
        <div className="rounded-md bg-destructive/10 px-4 py-3 text-sm text-destructive">
          Failed to load dashboard data. Please try again.
        </div>
      )}

      {data && <OverviewCards data={data} />}
    </div>
  )
}
