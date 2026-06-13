import { useEffect, useRef, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useCreateAuthor } from '@/features/courses/api'
import { Button } from '@/components/ui/button'

const authorSchema = z.object({
  name: z.string().min(1, 'Name is required').max(200, 'Name must be 200 characters or fewer'),
  bio: z.string().max(1000, 'Bio must be 1000 characters or fewer').optional(),
  avatarUrl: z.string().max(2048, 'URL must be 2048 characters or fewer').optional(),
})

type AuthorFormInput = z.infer<typeof authorSchema>

type AlertMessage = { type: 'success' | 'error'; text: string }

export default function CreateAuthorPage() {
  const navigate = useNavigate()
  const mutation = useCreateAuthor()
  const [message, setMessage] = useState<AlertMessage | null>(null)
  const timeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  useEffect(() => {
    return () => {
      if (timeoutRef.current) clearTimeout(timeoutRef.current)
    }
  }, [])

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<AuthorFormInput>({
    resolver: zodResolver(authorSchema),
  })

  const notify = (type: 'success' | 'error', text: string) => {
    if (timeoutRef.current) clearTimeout(timeoutRef.current)
    setMessage({ type, text })
    timeoutRef.current = setTimeout(() => setMessage(null), 4000)
  }

  const onSubmit = async (data: AuthorFormInput) => {
    try {
      await mutation.mutateAsync({
        name: data.name,
        bio: data.bio || undefined,
        avatarUrl: data.avatarUrl || undefined,
      })
      navigate('/admin/authors')
    } catch {
      notify('error', 'Failed to create author. Please try again.')
    }
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
        <h1 className="mt-2 text-2xl font-bold">New Author</h1>
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
            placeholder="e.g. John Smith"
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
            placeholder="Short biography (optional)"
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
            placeholder="https://example.com/avatar.jpg (optional)"
          />
          {errors.avatarUrl && (
            <p className="mt-1 text-xs text-destructive">{errors.avatarUrl.message}</p>
          )}
        </div>

        <div className="flex gap-3 pt-2">
          <Button type="submit" disabled={isSubmitting || mutation.isPending}>
            {isSubmitting || mutation.isPending ? 'Creating…' : 'Create Author'}
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
