import { useEffect, useRef, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useAuthor, useUpdateAuthor } from '@/features/courses/api'
import { Button } from '@/components/ui/button'

const authorSchema = z.object({
  name: z.string().min(1, 'Name is required').max(200, 'Name must be 200 characters or fewer'),
  bio: z.string().max(1000, 'Bio must be 1000 characters or fewer').optional(),
  avatarUrl: z.string().max(2048, 'URL must be 2048 characters or fewer').optional(),
})

type AuthorFormInput = z.infer<typeof authorSchema>

type AlertMessage = { type: 'success' | 'error'; text: string }

export default function EditAuthorPage() {
  const { id } = useParams<{ id: string }>()
  const authorId = Number(id)
  const navigate = useNavigate()
  const mutation = useUpdateAuthor()
  const [message, setMessage] = useState<AlertMessage | null>(null)
  const timeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  const isValidId = !Number.isNaN(authorId) && authorId > 0
  const { data: author, isLoading, isError } = useAuthor(isValidId ? authorId : 0)

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<AuthorFormInput>({
    resolver: zodResolver(authorSchema),
  })

  useEffect(() => {
    if (author) {
      reset({
        name: author.name,
        bio: author.bio ?? '',
        avatarUrl: author.avatarUrl ?? '',
      })
    }
  }, [author, reset])

  useEffect(() => {
    return () => {
      if (timeoutRef.current) clearTimeout(timeoutRef.current)
    }
  }, [])

  const notify = (type: 'success' | 'error', text: string) => {
    if (timeoutRef.current) clearTimeout(timeoutRef.current)
    setMessage({ type, text })
    timeoutRef.current = setTimeout(() => setMessage(null), 4000)
  }

  const onSubmit = async (data: AuthorFormInput) => {
    try {
      await mutation.mutateAsync({
        id: authorId,
        data: {
          name: data.name,
          bio: data.bio || undefined,
          avatarUrl: data.avatarUrl || undefined,
        },
      })
      navigate('/admin/authors')
    } catch {
      notify('error', 'Failed to update author. Please try again.')
    }
  }

  if (!isValidId) {
    return (
      <div className="mx-auto max-w-2xl px-4 py-8 sm:px-6">
        <p className="text-muted-foreground">Invalid author ID.</p>
        <Link to="/admin/authors" className="mt-2 block text-sm text-primary hover:underline">
          Back to authors
        </Link>
      </div>
    )
  }

  if (isLoading) {
    return (
      <div className="mx-auto max-w-2xl animate-pulse space-y-3 px-4 py-8 sm:px-6">
        <div className="h-4 w-32 rounded bg-muted" />
        <div className="h-8 w-56 rounded bg-muted" />
        <div className="mt-6 space-y-4">
          {Array.from({ length: 3 }).map((_, i) => (
            <div key={i} className="h-10 rounded bg-muted" />
          ))}
        </div>
      </div>
    )
  }

  if (isError || !author) {
    return (
      <div className="mx-auto max-w-2xl px-4 py-8 sm:px-6">
        <p className="text-muted-foreground">Author not found.</p>
        <Link to="/admin/authors" className="mt-2 block text-sm text-primary hover:underline">
          Back to authors
        </Link>
      </div>
    )
  }

  return (
    <div className="mx-auto max-w-2xl px-4 py-8 sm:px-6">
      <div className="mb-6">
        <Link
          to="/admin/authors"
          className="text-sm text-muted-foreground hover:text-foreground"
        >
          ← Back to authors
        </Link>
        <h1 className="mt-2 text-2xl font-bold">Edit Author</h1>
      </div>

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

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
        {/* Name */}
        <div>
          <label className="mb-1 block text-sm font-medium">
            Name <span className="text-destructive">*</span>
          </label>
          <input
            {...register('name')}
            className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
          />
          {errors.name && (
            <p className="mt-1 text-xs text-destructive">{errors.name.message}</p>
          )}
        </div>

        {/* Bio */}
        <div>
          <label className="mb-1 block text-sm font-medium">Bio</label>
          <textarea
            {...register('bio')}
            rows={4}
            className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
          />
          {errors.bio && (
            <p className="mt-1 text-xs text-destructive">{errors.bio.message}</p>
          )}
        </div>

        {/* Avatar URL */}
        <div>
          <label className="mb-1 block text-sm font-medium">Avatar URL</label>
          <input
            {...register('avatarUrl')}
            type="url"
            className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
          />
          {errors.avatarUrl && (
            <p className="mt-1 text-xs text-destructive">{errors.avatarUrl.message}</p>
          )}
        </div>

        <div className="flex gap-3 pt-2">
          <Button type="submit" disabled={isSubmitting || mutation.isPending}>
            {isSubmitting || mutation.isPending ? 'Saving…' : 'Save Changes'}
          </Button>
          <Button
            type="button"
            variant="ghost"
            onClick={() => navigate('/admin/authors')}
          >
            Cancel
          </Button>
        </div>
      </form>
    </div>
  )
}
