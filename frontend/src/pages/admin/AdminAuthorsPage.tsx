import { useEffect, useRef, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuthors, useDeleteAuthor } from '@/features/courses/api'
import { Button } from '@/components/ui/button'
import UserAvatarMenu from '@/components/layout/UserAvatarMenu'
import type { AxiosError } from 'axios'

type AlertMessage = { type: 'success' | 'error'; text: string }

export default function AdminAuthorsPage() {
  const navigate = useNavigate()
  const [message, setMessage] = useState<AlertMessage | null>(null)
  const [confirmDeleteId, setConfirmDeleteId] = useState<number | null>(null)
  const timeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  useEffect(() => {
    return () => {
      if (timeoutRef.current) clearTimeout(timeoutRef.current)
    }
  }, [])

  const { data: authors, isLoading, isError } = useAuthors()
  const deleteMutation = useDeleteAuthor()

  const notify = (type: 'success' | 'error', text: string) => {
    if (timeoutRef.current) clearTimeout(timeoutRef.current)
    setMessage({ type, text })
    timeoutRef.current = setTimeout(() => setMessage(null), 4000)
  }

  const handleDelete = async (id: number) => {
    try {
      await deleteMutation.mutateAsync(id)
      notify('success', 'Author deleted.')
      setConfirmDeleteId(null)
    } catch (err: unknown) {
      const axiosErr = err as AxiosError<{ error?: string }>
      const status = axiosErr?.response?.status
      if (status === 409) {
        notify('error', 'This author has assigned courses. Reassign all courses before deleting.')
      } else {
        notify('error', axiosErr?.response?.data?.error ?? 'Delete failed.')
      }
      setConfirmDeleteId(null)
    }
  }

  return (
    <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
      {/* Header */}
      <div className="mb-6 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <h1 className="text-2xl font-bold">Authors</h1>
        <div className="flex items-center gap-3">
          <Button onClick={() => navigate('/admin/authors/new')}>
            + New Author
          </Button>
          <UserAvatarMenu />
        </div>
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

      {/* Loading skeletons */}
      {isLoading && (
        <div className="space-y-2">
          {Array.from({ length: 5 }).map((_, i) => (
            <div key={i} className="h-12 animate-pulse rounded-lg border bg-muted" />
          ))}
        </div>
      )}

      {/* Error */}
      {isError && (
        <div className="rounded-md bg-destructive/10 px-4 py-3 text-sm text-destructive">
          Failed to load authors. Please try again.
        </div>
      )}

      {/* Empty state */}
      {!isLoading && !isError && authors && authors.length === 0 && (
        <div className="rounded-lg border border-dashed p-12 text-center">
          <p className="mb-4 text-muted-foreground">No authors yet.</p>
          <Button onClick={() => navigate('/admin/authors/new')}>
            Create Author
          </Button>
        </div>
      )}

      {/* Table */}
      {!isLoading && !isError && authors && authors.length > 0 && (
        <div className="overflow-x-auto rounded-lg border border-border">
          <table className="w-full text-sm">
            <thead className="bg-muted/50">
              <tr>
                <th className="px-4 py-3 text-left font-medium">Name</th>
                <th className="hidden px-4 py-3 text-left font-medium sm:table-cell">Bio</th>
                <th className="px-4 py-3 text-right font-medium">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
              {authors.map((author) => (
                <tr key={author.id} className="hover:bg-muted/30">
                  <td className="px-4 py-3 font-medium">
                    <Link
                      to={`/admin/authors/${author.id}/edit`}
                      className="hover:underline"
                    >
                      {author.name}
                    </Link>
                  </td>
                  <td className="hidden px-4 py-3 text-muted-foreground sm:table-cell">
                    {author.bio
                      ? author.bio.length > 80
                        ? author.bio.slice(0, 80) + '…'
                        : author.bio
                      : '—'}
                  </td>
                  <td className="px-4 py-3">
                    <div className="flex items-center justify-end gap-2">
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => navigate(`/admin/authors/${author.id}/edit`)}
                      >
                        Edit
                      </Button>

                      {confirmDeleteId === author.id ? (
                        <>
                          <span className="text-xs text-muted-foreground">Sure?</span>
                          <Button
                            variant="destructive"
                            size="sm"
                            disabled={deleteMutation.isPending}
                            onClick={() => handleDelete(author.id)}
                          >
                            Yes, delete
                          </Button>
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => setConfirmDeleteId(null)}
                          >
                            Cancel
                          </Button>
                        </>
                      ) : (
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => setConfirmDeleteId(author.id)}
                        >
                          Delete
                        </Button>
                      )}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}
