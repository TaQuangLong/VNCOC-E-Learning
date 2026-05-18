import { useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import {
  useAdminLessonResources,
  useAddResource,
  useDeleteResource,
  useUpdateLesson,
  useLessonDetail,
} from '@/features/lessons/api'
import LessonForm from '@/features/lessons/LessonForm'
import { resourceFormSchema, type LessonFormInput, type ResourceFormInput } from '@/features/lessons/types'
import { Button } from '@/components/ui/button'
import QuizBuilder from '@/features/quiz/QuizBuilder'

export default function EditLessonPage() {
  const { id: idParam } = useParams<{ id: string }>()
  const lessonId = Number(idParam)
  const navigate = useNavigate()

  const {
    data: lesson,
    isLoading,
    isError,
  } = useLessonDetail(lessonId)

  const updateMutation = useUpdateLesson(lessonId, lesson?.courseId ?? 0)

  const handleSubmit = async (data: LessonFormInput) => {
    await updateMutation.mutateAsync(data)
    navigate(`/admin/courses/${lesson?.courseId}/lessons`)
  }

  if (isLoading) {
    return (
      <div className="mx-auto max-w-2xl px-4 py-8">
        <div className="space-y-3">
          {Array.from({ length: 6 }).map((_, i) => (
            <div key={i} className="h-10 animate-pulse rounded-md bg-muted" />
          ))}
        </div>
      </div>
    )
  }

  if (isError || !lesson) {
    return (
      <div className="mx-auto max-w-2xl px-4 py-8">
        <p className="text-sm text-destructive">Lesson not found.</p>
      </div>
    )
  }

  const defaultValues: Partial<LessonFormInput> = {
    title: lesson.title,
    description: lesson.description ?? '',
    contentType: lesson.contentType,
    youTubeUrl: lesson.youTubeUrl ?? '',
    textContent: lesson.textContent ?? '',
    pdfUrl: lesson.pdfUrl ?? '',
    durationSeconds: lesson.durationSeconds,
    orderIndex: lesson.orderIndex,
    isPreview: lesson.isPreview,
  }

  return (
    <div className="mx-auto max-w-2xl px-4 py-8 sm:px-6">
      <div className="mb-6">
        <Link
          to={`/admin/courses/${lesson.courseId}/lessons`}
          className="text-sm text-muted-foreground hover:text-foreground"
        >
          ← Back to lessons
        </Link>
        <h1 className="mt-2 text-2xl font-bold">Edit Lesson</h1>
      </div>

      <LessonForm
        defaultValues={defaultValues}
        onSubmit={handleSubmit}
        submitLabel="Save Changes"
        key={lessonId}
      />

      <div className="mt-10">
        <ResourcesPanel lessonId={lessonId} />
      </div>

      <div className="mt-10">
        <h2 className="mb-4 text-base font-semibold">Quiz</h2>
        <QuizBuilder lessonId={lessonId} />
      </div>
    </div>
  )
}

function ResourcesPanel({ lessonId }: { lessonId: number }) {
  const { data: resources, isLoading } = useAdminLessonResources(lessonId)
  const addMutation = useAddResource(lessonId)
  const deleteMutation = useDeleteResource(lessonId)
  const [showForm, setShowForm] = useState(false)

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<ResourceFormInput>({
    resolver: zodResolver(resourceFormSchema),
    defaultValues: { title: '', url: '' },
  })

  const handleAdd = async (data: ResourceFormInput) => {
    await addMutation.mutateAsync(data)
    reset()
    setShowForm(false)
  }

  const handleDelete = async (resourceId: number) => {
    if (!confirm('Delete this resource?')) return
    try {
      await deleteMutation.mutateAsync(resourceId)
    } catch {
      // handled via mutation state
    }
  }

  return (
    <section aria-labelledby="resources-panel-heading">
      <div className="mb-3 flex items-center justify-between">
        <h2
          id="resources-panel-heading"
          className="text-base font-semibold"
        >
          Resources
        </h2>
        <Button
          variant="outline"
          size="sm"
          onClick={() => setShowForm((v) => !v)}
        >
          {showForm ? 'Cancel' : '+ Add Resource'}
        </Button>
      </div>

      {showForm && (
        <form
          onSubmit={handleSubmit(handleAdd)}
          className="mb-4 space-y-3 rounded-md border border-border p-4"
          noValidate
        >
          <div className="space-y-1">
            <label htmlFor="res-title" className="text-sm font-medium">
              Title <span className="text-destructive">*</span>
            </label>
            <input
              id="res-title"
              type="text"
              className="border-input bg-background w-full rounded-md border px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
              {...register('title')}
            />
            {errors.title && (
              <p className="text-xs text-destructive">{errors.title.message}</p>
            )}
          </div>
          <div className="space-y-1">
            <label htmlFor="res-url" className="text-sm font-medium">
              URL <span className="text-destructive">*</span>
            </label>
            <input
              id="res-url"
              type="url"
              placeholder="https://..."
              className="border-input bg-background w-full rounded-md border px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
              {...register('url')}
            />
            {errors.url && (
              <p className="text-xs text-destructive">{errors.url.message}</p>
            )}
          </div>
          <Button type="submit" size="sm" disabled={isSubmitting}>
            {isSubmitting ? 'Adding…' : 'Add'}
          </Button>
          {addMutation.isError && (
            <p className="text-xs text-destructive">Failed to add resource.</p>
          )}
        </form>
      )}

      {isLoading && (
        <div className="space-y-2">
          {Array.from({ length: 2 }).map((_, i) => (
            <div key={i} className="h-10 animate-pulse rounded-md bg-muted" />
          ))}
        </div>
      )}

      {!isLoading && resources?.length === 0 && (
        <p className="text-sm text-muted-foreground">No resources added yet.</p>
      )}

      {resources && resources.length > 0 && (
        <ul className="space-y-2">
          {resources.map((r) => (
            <li
              key={r.id}
              className="flex items-center gap-3 rounded-md border border-border px-3 py-2"
            >
              <span className="flex-1 truncate text-sm">{r.title}</span>
              <a
                href={r.url}
                target="_blank"
                rel="noopener noreferrer"
                className="text-xs text-muted-foreground underline hover:text-foreground"
              >
                {r.url.length > 40 ? r.url.slice(0, 40) + '…' : r.url}
              </a>
              <button
                type="button"
                onClick={() => handleDelete(r.id)}
                disabled={deleteMutation.isPending}
                className="text-xs text-destructive hover:underline disabled:opacity-50"
              >
                Remove
              </button>
            </li>
          ))}
        </ul>
      )}

      {deleteMutation.isError && (
        <p className="mt-2 text-xs text-destructive">
          Failed to remove resource.
        </p>
      )}
    </section>
  )
}
