import { useParams, Link } from 'react-router-dom'
import { useUserProgress } from '@/features/reports/api'
import UserProgressDetail from '@/features/reports/UserProgressDetail'

export default function UserProgressPage() {
  const { userId } = useParams<{ userId: string }>()
  const { data, isLoading, isError } = useUserProgress(userId ?? '')

  return (
    <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
      <div className="mb-6 flex items-center gap-3">
        <Link
          to="/admin/dashboard"
          className="text-sm text-muted-foreground hover:underline"
        >
          ← Dashboard
        </Link>
        <h1 className="text-2xl font-bold">
          {data ? `${data.displayName} — Progress` : 'User Progress'}
        </h1>
      </div>

      {isLoading && (
        <div className="space-y-4">
          {Array.from({ length: 3 }).map((_, i) => (
            <div
              key={i}
              className="h-28 animate-pulse rounded-lg border bg-muted"
            />
          ))}
        </div>
      )}

      {isError && (
        <div className="rounded-md bg-destructive/10 px-4 py-3 text-sm text-destructive">
          Failed to load user progress. Please try again.
        </div>
      )}

      {data && (
        <>
          <div className="mb-4 text-sm text-muted-foreground">
            <span className="font-medium text-foreground">{data.email}</span>
            {' · '}
            {data.courses.length} course{data.courses.length !== 1 ? 's' : ''}
          </div>
          <UserProgressDetail courses={data.courses} />
        </>
      )}
    </div>
  )
}
