import { useEffect, useRef, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import {
  useAdminCourses,
  useDeleteCourse,
  usePublishCourse,
  useUnpublishCourse,
} from '@/features/courses/api'
import CourseStatusBadge from '@/features/courses/CourseStatusBadge'
import { Button } from '@/components/ui/button'
import type { CourseStatus } from '@/features/courses/types'

type AlertMessage = { type: 'success' | 'error'; text: string }

export default function AdminCoursesPage() {
  const navigate = useNavigate()
  const [statusFilter, setStatusFilter] = useState('')
  const [titleFilter, setTitleFilter] = useState('')
  const [page, setPage] = useState(1)
  const [message, setMessage] = useState<AlertMessage | null>(null)
  const timeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  useEffect(() => {
    return () => {
      if (timeoutRef.current) clearTimeout(timeoutRef.current)
    }
  }, [])

  const { data, isLoading, isError } = useAdminCourses({
    page,
    pageSize: 20,
    status: statusFilter || undefined,
    title: titleFilter || undefined,
  })

  const deleteMutation = useDeleteCourse()
  const publishMutation = usePublishCourse()
  const unpublishMutation = useUnpublishCourse()

  const totalPages = data ? Math.ceil(data.totalCount / data.pageSize) : 0

  const notify = (type: 'success' | 'error', text: string) => {
    if (timeoutRef.current) clearTimeout(timeoutRef.current)
    setMessage({ type, text })
    timeoutRef.current = setTimeout(() => setMessage(null), 3500)
  }

  const handleAction = async (
    action: () => Promise<unknown>,
    successText: string,
  ) => {
    try {
      await action()
      notify('success', successText)
    } catch (err: unknown) {
      notify('error', err instanceof Error ? err.message : 'Action failed')
    }
  }

  return (
    <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
      {/* Header */}
      <div className="mb-6 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <h1 className="text-2xl font-bold">Courses</h1>
        <Button onClick={() => navigate('/admin/courses/new')}>
          + New Course
        </Button>
      </div>

      {/* Alert */}
      {message && (
        <div
          className={`mb-4 rounded-md px-4 py-2 text-sm ${
            message.type === 'success'
              ? 'bg-green-100 text-green-800'
              : 'bg-destructive/10 text-destructive'
          }`}
        >
          {message.text}
        </div>
      )}

      {/* Filters */}
      <div className="mb-4 flex flex-col gap-3 sm:flex-row">
        <input
          type="search"
          placeholder="Search by title…"
          value={titleFilter}
          onChange={(e) => {
            setTitleFilter(e.target.value)
            setPage(1)
          }}
          className="border-input bg-background flex-1 rounded-md border px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
        />
        <select
          value={statusFilter}
          onChange={(e) => {
            setStatusFilter(e.target.value)
            setPage(1)
          }}
          className="border-input bg-background w-full rounded-md border px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring sm:w-40"
        >
          <option value="">All statuses</option>
          <option value="Draft">Draft</option>
          <option value="Published">Published</option>
          <option value="Archived">Archived</option>
        </select>
      </div>

      {/* Loading */}
      {isLoading && (
        <div className="space-y-2">
          {Array.from({ length: 5 }).map((_, i) => (
            <div key={i} className="h-14 animate-pulse rounded-lg bg-muted" />
          ))}
        </div>
      )}

      {/* Error */}
      {isError && (
        <div className="rounded-md bg-destructive/10 px-4 py-3 text-sm text-destructive">
          Failed to load courses. Please try again.
        </div>
      )}

      {/* Empty */}
      {!isLoading && !isError && data?.items.length === 0 && (
        <div className="py-20 text-center text-muted-foreground">
          No courses found.{' '}
          <Link to="/admin/courses/new" className="text-primary hover:underline">
            Create one.
          </Link>
        </div>
      )}

      {/* Table */}
      {!isLoading && !isError && data && data.items.length > 0 && (
        <div className="overflow-x-auto rounded-lg border border-border">
          <table className="w-full text-sm">
            <thead className="bg-muted/50">
              <tr>
                <th className="px-4 py-3 text-left font-medium">Title</th>
                <th className="hidden px-4 py-3 text-left font-medium sm:table-cell">
                  Author
                </th>
                <th className="hidden px-4 py-3 text-left font-medium md:table-cell">
                  Category
                </th>
                <th className="px-4 py-3 text-left font-medium">Status</th>
                <th className="px-4 py-3 text-right font-medium">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
              {data.items.map((course) => (
                <tr key={course.id} className="hover:bg-muted/30">
                  <td className="px-4 py-3 font-medium">
                    <Link
                      to={`/admin/courses/${course.id}/edit`}
                      className="hover:underline"
                    >
                      {course.title}
                    </Link>
                  </td>
                  <td className="hidden px-4 py-3 text-muted-foreground sm:table-cell">
                    {course.authorName}
                  </td>
                  <td className="hidden px-4 py-3 text-muted-foreground md:table-cell">
                    {course.category ?? '—'}
                  </td>
                  <td className="px-4 py-3">
                    <CourseStatusBadge status={course.status as CourseStatus} />
                  </td>
                  <td className="px-4 py-3">
                    <div className="flex items-center justify-end gap-2">
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() =>
                          navigate(`/admin/courses/${course.id}/edit`)
                        }
                      >
                        Edit
                      </Button>

                      {course.status === 'Draft' && (
                        <Button
                          variant="outline"
                          size="sm"
                          disabled={publishMutation.isPending}
                          onClick={() =>
                            handleAction(
                              () => publishMutation.mutateAsync(course.id),
                              `"${course.title}" published.`,
                            )
                          }
                        >
                          Publish
                        </Button>
                      )}

                      {course.status === 'Published' && (
                        <Button
                          variant="outline"
                          size="sm"
                          disabled={unpublishMutation.isPending}
                          onClick={() =>
                            handleAction(
                              () => unpublishMutation.mutateAsync(course.id),
                              `"${course.title}" unpublished.`,
                            )
                          }
                        >
                          Unpublish
                        </Button>
                      )}

                      <Button
                        variant="destructive"
                        size="sm"
                        disabled={deleteMutation.isPending}
                        onClick={() => {
                          if (confirm(`Delete "${course.title}"?`)) {
                            handleAction(
                              () => deleteMutation.mutateAsync(course.id),
                              `"${course.title}" deleted.`,
                            )
                          }
                        }}
                      >
                        Delete
                      </Button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* Pagination */}
      {totalPages > 1 && (
        <div className="mt-6 flex items-center justify-center gap-3">
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
    </div>
  )
}
