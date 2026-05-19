import { useState } from 'react'
import { Button } from '@/components/ui/button'

const MAX_CHARS = 2000

interface Props {
  onSubmit: (content: string) => Promise<void>
  onCancel?: () => void
  isPending: boolean
  placeholder?: string
  submitLabel?: string
  autoFocus?: boolean
  initialValue?: string
}

export default function DiscussionForm({
  onSubmit,
  onCancel,
  isPending,
  placeholder = 'Write your comment or question…',
  submitLabel = 'Post',
  autoFocus = false,
  initialValue = '',
}: Props) {
  const [content, setContent] = useState(initialValue)
  const [error, setError] = useState<string | null>(null)

  const remaining = MAX_CHARS - content.length
  const isOverLimit = remaining < 0

  const handleSubmit = async () => {
    if (!content.trim()) {
      setError('Content is required.')
      return
    }
    if (isOverLimit) {
      setError(`Content must be at most ${MAX_CHARS} characters.`)
      return
    }
    setError(null)
    await onSubmit(content.trim())
    setContent('')
  }

  return (
    <div className="space-y-2">
      <textarea
        value={content}
        onChange={(e) => setContent(e.target.value)}
        placeholder={placeholder}
        // eslint-disable-next-line jsx-a11y/no-autofocus
        autoFocus={autoFocus}
        rows={3}
        className="w-full resize-none rounded-md border border-input bg-background px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
      />
      <div className="flex items-center justify-between gap-2">
        <span
          className={`text-xs ${isOverLimit ? 'text-destructive' : 'text-muted-foreground'}`}
        >
          {remaining} characters left
        </span>
        <div className="flex gap-2">
          {onCancel && (
            <Button
              type="button"
              variant="outline"
              size="sm"
              onClick={onCancel}
              disabled={isPending}
            >
              Cancel
            </Button>
          )}
          <Button
            type="button"
            size="sm"
            onClick={handleSubmit}
            disabled={isPending || !content.trim() || isOverLimit}
          >
            {isPending ? 'Posting…' : submitLabel}
          </Button>
        </div>
      </div>
      {error && <p className="text-xs text-destructive">{error}</p>}
    </div>
  )
}
