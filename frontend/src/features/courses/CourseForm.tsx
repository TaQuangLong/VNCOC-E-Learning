import { useEffect } from 'react'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Button } from '@/components/ui/button'
import { useAuthors } from './api'
import { courseFormSchema, slugify, type CourseFormInput } from './types'

interface Props {
  defaultValues?: Partial<CourseFormInput>
  onSubmit: (data: CourseFormInput) => Promise<void>
  submitLabel: string
  isEditMode?: boolean
}

export default function CourseForm({
  defaultValues,
  onSubmit,
  submitLabel,
  isEditMode = false,
}: Props) {
  const { data: authors, isLoading: authorsLoading } = useAuthors()

  const {
    register,
    handleSubmit,
    watch,
    setValue,
    control,
    formState: { errors, isSubmitting },
    setError,
  } = useForm<CourseFormInput>({
    resolver: zodResolver(courseFormSchema),
    defaultValues: {
      title: '',
      slug: '',
      shortDescription: '',
      description: '',
      thumbnailUrl: '',
      category: '',
      level: '',
      language: '',
      authorId: 0,
      ...defaultValues,
    },
  })

  const titleValue = watch('title')

  // Auto-generate slug from title in create mode only
  useEffect(() => {
    if (!isEditMode && titleValue) {
      setValue('slug', slugify(titleValue), { shouldValidate: false })
    }
  }, [titleValue, isEditMode, setValue])

  const handleFormSubmit = async (data: CourseFormInput) => {
    try {
      await onSubmit(data)
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Something went wrong'
      setError('root', { message })
    }
  }

  return (
    <form onSubmit={handleSubmit(handleFormSubmit)} className="space-y-4">
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

      {/* Slug */}
      <div className="space-y-1">
        <label htmlFor="slug" className="text-sm font-medium">
          Slug <span className="text-destructive">*</span>
        </label>
        <input
          id="slug"
          type="text"
          className="border-input bg-background w-full rounded-md border px-3 py-2 font-mono text-sm focus:outline-none focus:ring-2 focus:ring-ring"
          {...register('slug')}
        />
        {errors.slug && (
          <p className="text-xs text-destructive">{errors.slug.message}</p>
        )}
      </div>

      {/* Author */}
      <div className="space-y-1">
        <label htmlFor="authorId" className="text-sm font-medium">
          Author <span className="text-destructive">*</span>
        </label>
        <Controller
          name="authorId"
          control={control}
          render={({ field }) => (
            <select
              id="authorId"
              className="border-input bg-background w-full rounded-md border px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring disabled:opacity-50"
              disabled={authorsLoading}
              value={field.value}
              onChange={(e) => field.onChange(Number(e.target.value))}
            >
              <option value={0}>Select an author…</option>
              {authors?.map((a) => (
                <option key={a.id} value={a.id}>
                  {a.name}
                </option>
              ))}
            </select>
          )}
        />
        {errors.authorId && (
          <p className="text-xs text-destructive">{errors.authorId.message}</p>
        )}
      </div>

      {/* Short Description */}
      <div className="space-y-1">
        <label htmlFor="shortDescription" className="text-sm font-medium">
          Short Description
        </label>
        <textarea
          id="shortDescription"
          rows={2}
          className="border-input bg-background w-full resize-none rounded-md border px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
          {...register('shortDescription')}
        />
        {errors.shortDescription && (
          <p className="text-xs text-destructive">
            {errors.shortDescription.message}
          </p>
        )}
      </div>

      {/* Description */}
      <div className="space-y-1">
        <label htmlFor="description" className="text-sm font-medium">
          Description
        </label>
        <textarea
          id="description"
          rows={5}
          className="border-input bg-background w-full resize-y rounded-md border px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
          {...register('description')}
        />
      </div>

      {/* Thumbnail URL */}
      <div className="space-y-1">
        <label htmlFor="thumbnailUrl" className="text-sm font-medium">
          Thumbnail URL
        </label>
        <input
          id="thumbnailUrl"
          type="text"
          placeholder="https://example.com/image.jpg"
          className="border-input bg-background w-full rounded-md border px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
          {...register('thumbnailUrl')}
        />
        {errors.thumbnailUrl && (
          <p className="text-xs text-destructive">{errors.thumbnailUrl.message}</p>
        )}
      </div>

      {/* Category / Level / Language */}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
        <div className="space-y-1">
          <label htmlFor="category" className="text-sm font-medium">
            Category
          </label>
          <input
            id="category"
            type="text"
            placeholder="e.g. Bible Study"
            className="border-input bg-background w-full rounded-md border px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
            {...register('category')}
          />
        </div>
        <div className="space-y-1">
          <label htmlFor="level" className="text-sm font-medium">
            Level
          </label>
          <select
            id="level"
            className="border-input bg-background w-full rounded-md border px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
            {...register('level')}
          >
            <option value="">Any level</option>
            <option value="Beginner">Beginner</option>
            <option value="Intermediate">Intermediate</option>
            <option value="Advanced">Advanced</option>
          </select>
        </div>
        <div className="space-y-1">
          <label htmlFor="language" className="text-sm font-medium">
            Language
          </label>
          <input
            id="language"
            type="text"
            placeholder="e.g. English"
            className="border-input bg-background w-full rounded-md border px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
            {...register('language')}
          />
        </div>
      </div>

      {errors.root && (
        <p className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">
          {errors.root.message}
        </p>
      )}

      <Button type="submit" disabled={isSubmitting}>
        {isSubmitting ? 'Saving…' : submitLabel}
      </Button>
    </form>
  )
}
