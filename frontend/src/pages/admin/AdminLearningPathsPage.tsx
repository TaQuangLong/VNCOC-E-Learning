import { useEffect, useRef, useState } from 'react'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { AxiosError } from 'axios'

import UserAvatarMenu from '@/components/layout/UserAvatarMenu'
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from '@/components/ui/alert-dialog'
import { Button } from '@/components/ui/button'
import { Toast, type ToastMessage } from '@/components/ui/toast'
import {
  useAdminLearningPaths,
  useArchiveLearningPath,
  usePublishLearningPath,
  useUnpublishLearningPath,
} from '@/features/learning-paths/api'
import type { LearningPathStatus } from '@/features/learning-paths/types'
import { cn } from '@/lib/utils'

interface LocationState {
  toast?: ToastMessage
}

const statusStyles: Record<LearningPathStatus, string> = {
  Draft: 'bg-amber-100 text-amber-800',
  Published: 'bg-green-100 text-green-800',
  Archived: 'bg-muted text-muted-foreground',
}

function getErrorMessage(error: unknown, fallback: string) {
  if (error instanceof AxiosError) {
    const data = error.response?.data as
      | { error?: string; message?: string }
      | undefined
    return data?.error ?? data?.message ?? fallback
  }

  return error instanceof Error ? error.message : fallback
}

export default function AdminLearningPathsPage() {
  const navigate = useNavigate()
  const location = useLocation()
  const [page, setPage] = useState(1)
  const [status, setStatus] = useState<LearningPathStatus | ''>('')
  const [toast, setToast] = useState<ToastMessage | null>(
    (location.state as LocationState | null)?.toast ?? null,
  )
  const toastTimeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  const { data, isLoading, isError } = useAdminLearningPaths({
    page,
    pageSize: 20,
    status: status || undefined,
  })
  const publishMutation = usePublishLearningPath()
  const unpublishMutation = useUnpublishLearningPath()
  const archiveMutation = useArchiveLearningPath()

  const totalPages = data ? Math.ceil(data.totalCount / data.pageSize) : 0

  useEffect(() => {
    const redirectedToast = (location.state as LocationState | null)?.toast
    if (redirectedToast) {
      navigate(location.pathname, { replace: true, state: null })
    }
  }, [location.pathname, location.state, navigate])

  useEffect(() => {
    if (!toast) return

    if (toastTimeoutRef.current) clearTimeout(toastTimeoutRef.current)
    toastTimeoutRef.current = setTimeout(() => setToast(null), 4000)

    return () => {
      if (toastTimeoutRef.current) clearTimeout(toastTimeoutRef.current)
    }
  }, [toast])

  const notify = (message: ToastMessage) => {
    setToast(message)
  }

  const handleAction = async (
    action: () => Promise<unknown>,
    successText: string,
  ) => {
    try {
      await action()
      notify({ type: 'success', text: successText })
    } catch (error: unknown) {
      notify({
        type: 'error',
        text: getErrorMessage(error, 'The action could not be completed.'),
      })
    }
  }

  return (
    <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
      <div className="mb-6 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold">Learning Paths</h1>
          <p className="mt-1 text-sm text-muted-foreground">
            Build and publish guided course journeys.
          </p>
        </div>
        <div className="flex items-center gap-3">
          <Button onClick={() => navigate('/admin/learning-paths/new')}>
            New Learning Path
          </Button>
          <UserAvatarMenu />
        </div>
      </div>

      <div className="mb-4 flex justify-end">
        <label className="flex items-center gap-2 text-sm">
          <span className="text-muted-foreground">Status</span>
          <select
            value={status}
            onChange={(event) => {
              setStatus(event.target.value as LearningPathStatus | '')
              setPage(1)
            }}
            className="border-input bg-background rounded-md border px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
          >
            <option value="">All</option>
            <option value="Draft">Draft</option>
            <option value="Published">Published</option>
            <option value="Archived">Archived</option>
          </select>
        </label>
      </div>

      {isLoading && (
        <div className="space-y-2" aria-label="Loading learning paths">
          {Array.from({ length: 5 }).map((_, index) => (
            <div
              key={index}
              className="h-14 animate-pulse rounded-lg bg-muted"
            />
          ))}
        </div>
      )}

      {isError && (
        <div className="rounded-md bg-destructive/10 px-4 py-3 text-sm text-destructive">
          Failed to load learning paths. Please try again.
        </div>
      )}

      {!isLoading && !isError && data?.items.length === 0 && (
        <div className="rounded-lg border border-dashed p-12 text-center">
          <h2 className="font-semibold">No learning paths found</h2>
          <p className="mt-1 text-sm text-muted-foreground">
            {status
              ? `There are no ${status.toLowerCase()} learning paths.`
              : 'Create a path to organize published courses into a journey.'}
          </p>
          {!status && (
            <Button
              className="mt-4"
              onClick={() => navigate('/admin/learning-paths/new')}
            >
              Create Learning Path
            </Button>
          )}
        </div>
      )}

      {!isLoading && !isError && data && data.items.length > 0 && (
        <div className="overflow-x-auto rounded-lg border">
          <table className="w-full text-sm">
            <thead className="bg-muted/50">
              <tr>
                <th className="px-4 py-3 text-left font-medium">Title</th>
                <th className="hidden px-4 py-3 text-left font-medium sm:table-cell">
                  Slug
                </th>
                <th className="px-4 py-3 text-left font-medium">Status</th>
                <th className="hidden px-4 py-3 text-left font-medium md:table-cell">
                  Courses
                </th>
                <th className="px-4 py-3 text-right font-medium">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {data.items.map((path) => (
                <tr key={path.id} className="hover:bg-muted/30">
                  <td className="px-4 py-3 font-medium">
                    {path.status === 'Archived' ? (
                      path.title
                    ) : (
                      <Link
                        to={`/admin/learning-paths/${path.id}/edit`}
                        className="hover:underline"
                      >
                        {path.title}
                      </Link>
                    )}
                  </td>
                  <td className="hidden px-4 py-3 font-mono text-xs text-muted-foreground sm:table-cell">
                    {path.slug}
                  </td>
                  <td className="px-4 py-3">
                    <span
                      className={cn(
                        'inline-flex rounded-full px-2 py-1 text-xs font-medium',
                        statusStyles[path.status],
                      )}
                    >
                      {path.status}
                    </span>
                  </td>
                  <td className="hidden px-4 py-3 text-muted-foreground md:table-cell">
                    {path.courseCount}
                  </td>
                  <td className="px-4 py-3">
                    <div className="flex items-center justify-end gap-2">
                      {path.status !== 'Archived' && (
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() =>
                            navigate(
                              `/admin/learning-paths/${path.id}/edit`,
                            )
                          }
                        >
                          Edit
                        </Button>
                      )}

                      {path.status === 'Draft' && (
                        <Button
                          variant="outline"
                          size="sm"
                          disabled={publishMutation.isPending}
                          onClick={() =>
                            handleAction(
                              () => publishMutation.mutateAsync(path.id),
                              `"${path.title}" published.`,
                            )
                          }
                        >
                          Publish
                        </Button>
                      )}

                      {path.status === 'Published' && (
                        <Button
                          variant="outline"
                          size="sm"
                          disabled={unpublishMutation.isPending}
                          onClick={() =>
                            handleAction(
                              () => unpublishMutation.mutateAsync(path.id),
                              `"${path.title}" unpublished.`,
                            )
                          }
                        >
                          Unpublish
                        </Button>
                      )}

                      {path.status !== 'Archived' && (
                        <AlertDialog>
                          <AlertDialogTrigger
                            render={
                              <Button
                                variant="destructive"
                                size="sm"
                                disabled={archiveMutation.isPending}
                              />
                            }
                          >
                            Archive
                          </AlertDialogTrigger>
                          <AlertDialogContent>
                            <AlertDialogHeader>
                              <AlertDialogTitle>
                                Archive learning path?
                              </AlertDialogTitle>
                              <AlertDialogDescription>
                                "{path.title}" will be hidden from the public
                                catalog and cannot be edited or republished.
                              </AlertDialogDescription>
                            </AlertDialogHeader>
                            <AlertDialogFooter>
                              <AlertDialogCancel>Cancel</AlertDialogCancel>
                              <AlertDialogAction
                                disabled={archiveMutation.isPending}
                                onClick={() =>
                                  handleAction(
                                    () =>
                                      archiveMutation.mutateAsync(path.id),
                                    `"${path.title}" archived.`,
                                  )
                                }
                              >
                                Archive
                              </AlertDialogAction>
                            </AlertDialogFooter>
                          </AlertDialogContent>
                        </AlertDialog>
                      )}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {totalPages > 1 && (
        <div className="mt-6 flex items-center justify-center gap-3">
          <Button
            variant="outline"
            size="sm"
            disabled={page <= 1}
            onClick={() => setPage((currentPage) => currentPage - 1)}
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
            onClick={() => setPage((currentPage) => currentPage + 1)}
          >
            Next
          </Button>
        </div>
      )}

      <Toast message={toast} onDismiss={() => setToast(null)} />
    </div>
  )
}
