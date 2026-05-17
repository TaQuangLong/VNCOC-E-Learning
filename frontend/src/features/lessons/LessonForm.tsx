import { useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Button } from '@/components/ui/button'
import { lessonFormSchema, CONTENT_TYPE_LABELS, type LessonFormInput } from './types'

interface LessonFormProps {
  defaultValues?: Partial<LessonFormInput>
  onSubmit: (data: LessonFormInput) => Promise<void>
  submitLabel: string
}

export default function LessonForm({
  defaultValues,
  onSubmit,
  submitLabel,
}: LessonFormProps) {
  const {
    register,
    handleSubmit,
    watch,
    setValue,
    formState: { errors, isSubmitting },
    setError,
  } = useForm<LessonFormInput>({
    resolver: zodResolver(lessonFormSchema),
    defaultValues: {
      title: '',
      description: '',
      contentType: 'Video',
      youTubeUrl: '',
      textContent: '',
      pdfUrl: '',
      durationSeconds: 0,
      orderIndex: 0,
      isPreview: false,
      ...defaultValues,
    },
  })

  const contentType = watch('contentType')

  // Clear irrelevant fields when content type changes
  useEffect(() => {
    if (contentType !== 'Video') setValue('youTubeUrl', '')
    if (contentType !== 'Text') setValue('textContent', '')
    if (contentType !== 'Pdf') setValue('pdfUrl', '')
  }, [contentType, setValue])

  const handleFormSubmit = async (data: LessonFormInput) => {
    // Normalize empty optional strings to undefined
    const normalized: LessonFormInput = {
      ...data,
      description: data.description || undefined,
      youTubeUrl: data.youTubeUrl || undefined,
      textContent: data.textContent || undefined,
      pdfUrl: data.pdfUrl || undefined,
    }
    try {
      await onSubmit(normalized)
    } catch (err: unknown) {
      const message =
        err instanceof Error ? err.message : 'Something went wrong'
      setError('root', { message })
    }
  }

  return (
    <form
      onSubmit={handleSubmit(handleFormSubmit)}
      className="space-y-4"
      noValidate
    >
      {/* Title */}
      <div className="space-y-1">
        <label htmlFor="title" className="text-sm font-medium">
          Title <span className="text-destructive">*</span>
        </label>
        <input
          id="title"
          type="text"
          className="border-input bg-background w-full rounded-md border px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
          {...register('title')}
        />
        {errors.title && (
          <p className="text-xs text-destructive">{errors.title.message}</p>
        )}
      </div>

      {/* Description */}
      <div className="space-y-1">
        <label htmlFor="description" className="text-sm font-medium">
          Description
        </label>
        <textarea
          id="description"
          rows={3}
          className="border-input bg-background w-full rounded-md border px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
          {...register('description')}
        />
        {errors.description && (
          <p className="text-xs text-destructive">
            {errors.description.message}
          </p>
        )}
      </div>

      {/* Content type */}
      <div className="space-y-1">
        <label htmlFor="contentType" className="text-sm font-medium">
          Content Type <span className="text-destructive">*</span>
        </label>
        <select
          id="contentType"
          className="border-input bg-background w-full rounded-md border px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
          {...register('contentType')}
        >
          {(Object.entries(CONTENT_TYPE_LABELS) as [string, string][]).map(
            ([value, label]) => (
              <option key={value} value={value}>
                {label}
              </option>
            ),
          )}
        </select>
        {errors.contentType && (
          <p className="text-xs text-destructive">
            {errors.contentType.message}
          </p>
        )}
      </div>

      {/* YouTube URL — visible when Video */}
      {contentType === 'Video' && (
        <div className="space-y-1">
          <label htmlFor="youTubeUrl" className="text-sm font-medium">
            YouTube URL <span className="text-destructive">*</span>
          </label>
          <input
            id="youTubeUrl"
            type="url"
            placeholder="https://www.youtube.com/watch?v=..."
            className="border-input bg-background w-full rounded-md border px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
            {...register('youTubeUrl')}
          />
          {errors.youTubeUrl && (
            <p className="text-xs text-destructive">
              {errors.youTubeUrl.message}
            </p>
          )}
        </div>
      )}

      {/* Text content — visible when Text */}
      {contentType === 'Text' && (
        <div className="space-y-1">
          <label htmlFor="textContent" className="text-sm font-medium">
            Text Content <span className="text-destructive">*</span>
          </label>
          <textarea
            id="textContent"
            rows={12}
            placeholder="Enter lesson content (plain text or markdown)…"
            className="border-input bg-background w-full rounded-md border px-3 py-2 font-mono text-sm focus:outline-none focus:ring-2 focus:ring-ring"
            {...register('textContent')}
          />
          {errors.textContent && (
            <p className="text-xs text-destructive">
              {errors.textContent.message}
            </p>
          )}
        </div>
      )}

      {/* PDF URL — visible when Pdf */}
      {contentType === 'Pdf' && (
        <div className="space-y-1">
          <label htmlFor="pdfUrl" className="text-sm font-medium">
            PDF URL <span className="text-destructive">*</span>
          </label>
          <input
            id="pdfUrl"
            type="url"
            placeholder="https://example.com/document.pdf"
            className="border-input bg-background w-full rounded-md border px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
            {...register('pdfUrl')}
          />
          {errors.pdfUrl && (
            <p className="text-xs text-destructive">{errors.pdfUrl.message}</p>
          )}
        </div>
      )}

      {/* Duration */}
      <div className="grid grid-cols-2 gap-4">
        <div className="space-y-1">
          <label htmlFor="durationSeconds" className="text-sm font-medium">
            Duration (seconds)
          </label>
          <input
            id="durationSeconds"
            type="number"
            min={0}
            className="border-input bg-background w-full rounded-md border px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
            {...register('durationSeconds', { valueAsNumber: true })}
          />
          {errors.durationSeconds && (
            <p className="text-xs text-destructive">
              {errors.durationSeconds.message}
            </p>
          )}
        </div>
        <div className="space-y-1">
          <label htmlFor="orderIndex" className="text-sm font-medium">
            Order
          </label>
          <input
            id="orderIndex"
            type="number"
            min={0}
            className="border-input bg-background w-full rounded-md border px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
            {...register('orderIndex', { valueAsNumber: true })}
          />
          {errors.orderIndex && (
            <p className="text-xs text-destructive">
              {errors.orderIndex.message}
            </p>
          )}
        </div>
      </div>

      {/* Preview toggle */}
      <div className="flex items-center gap-3">
        <input
          id="isPreview"
          type="checkbox"
          className="h-4 w-4 rounded border-input accent-primary"
          {...register('isPreview')}
        />
        <label htmlFor="isPreview" className="text-sm font-medium">
          Free preview (accessible without enrollment)
        </label>
      </div>

      {/* Root error */}
      {errors.root && (
        <p className="text-sm text-destructive">{errors.root.message}</p>
      )}

      <Button type="submit" disabled={isSubmitting} className="w-full sm:w-auto">
        {isSubmitting ? 'Saving…' : submitLabel}
      </Button>
    </form>
  )
}
