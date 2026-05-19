import { useState } from 'react'
import { useParams, Link } from 'react-router-dom'
import { useCourseLearners } from '@/features/reports/api'
import CourseLearnersTable from '@/features/reports/CourseLearnersTable'
import { Button } from '@/components/ui/button'

const PAGE_SIZE = 20

export default function CourseLearnersPage() {
  const { courseId } = useParams<{ courseId: string }>()
  const [page, setPage] = useState(1)
  const id = Number(courseId)

  const { data, isLoading, isError } = useCourseLearners(id, page, PAGE_SIZE)

  const totalPages = data ? Math.ceil(data.totalCount / data.pageSize) : 0

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
          {data ? `${data.courseTitle} — Learners` : 'Course Learners'}
        </h1>
      </div>

      {isLoading && (
        <div className="space-y-3">
          {Array.from({ length: 8 }).map((_, i) => (
            <div key={i} className="h-12 animate-pulse rounded-md bg-muted" />
          ))}
        </div>
      )}

      {isError && (
        <div className="rounded-md bg-destructive/10 px-4 py-3 text-sm text-destructive">
          Failed to load learners. Please try again.
        </div>
      )}

      {data && (
        <>
          <p className="mb-4 text-sm text-muted-foreground">
            {data.totalCount} learner{data.totalCount !== 1 ? 's' : ''} enrolled
          </p>
          <CourseLearnersTable learners={data.items} />

          {totalPages > 1 && (
            <div className="mt-6 flex items-center justify-between">
              <Button
                variant="outline"
                size="sm"
                disabled={page <= 1}
                onClick={() => setPage((p) => p - 1)}
              >
                Previous
              </Button>
              <span className="text-sm text-muted-foreground">
                Page {page} of {totalPages}
              </span>
              <Button
                variant="outline"
                size="sm"
                disabled={page >= totalPages}
                onClick={() => setPage((p) => p + 1)}
              >
                Next
              </Button>
            </div>
          )}
        </>
      )}
    </div>
  )
}
