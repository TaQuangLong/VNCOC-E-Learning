import { Link, useNavigate, useParams } from 'react-router-dom'
import {
  useCourseLessons,
  useDeleteLesson,
  useReorderLessons,
} from '@/features/lessons/api'
import { Button } from '@/components/ui/button'
import type { LessonSummary } from '@/features/lessons/types'

export default function AdminLessonsPage() {
  const { courseId: courseIdParam } = useParams<{ courseId: string }>()
  const courseId = Number(courseIdParam)
  const navigate = useNavigate()

  const { data: lessons, isLoading, isError } = useCourseLessons(courseId)
  const deleteMutation = useDeleteLesson(courseId)
  const reorderMutation = useReorderLessons(courseId)

  const handleDelete = async (lesson: LessonSummary) => {
    if (!confirm(`Delete lesson "${lesson.title}"? This cannot be undone.`))
      return
    try {
      await deleteMutation.mutateAsync(lesson.id)
    } catch {
      // handled via mutation state
    }
  }

  const handleMove = async (index: number, direction: 'up' | 'down') => {
    if (!lessons) return
    const ids = lessons.map((l) => l.id)
    const swapIndex = direction === 'up' ? index - 1 : index + 1
    if (swapIndex < 0 || swapIndex >= ids.length) return
    const newIds = [...ids]
    ;[newIds[index], newIds[swapIndex]] = [newIds[swapIndex], newIds[index]]
    try {
      await reorderMutation.mutateAsync(newIds)
    } catch {
      // handled via mutation state
    }
  }

  return (
    <div className="mx-auto max-w-4xl px-4 py-8 sm:px-6 lg:px-8">
      {/* Header */}
      <div className="mb-6 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <Link
            to="/admin/courses"
            className="text-sm text-muted-foreground hover:text-foreground"
          >
            ← Back to courses
          </Link>
          <h1 className="mt-1 text-2xl font-bold">Lessons</h1>
        </div>
        <Button
          onClick={() =>
            navigate(`/admin/courses/${courseId}/lessons/new`)
          }
        >
          + New Lesson
        </Button>
      </div>

      {/* Loading */}
      {isLoading && (
        <div className="space-y-2">
          {Array.from({ length: 4 }).map((_, i) => (
            <div
              key={i}
              className="h-14 animate-pulse rounded-md bg-muted"
            />
          ))}
        </div>
      )}

      {/* Error */}
      {isError && (
        <div className="rounded-md bg-destructive/10 px-4 py-3 text-sm text-destructive">
          Failed to load lessons.
        </div>
      )}

      {/* Empty */}
      {!isLoading && !isError && lessons?.length === 0 && (
        <div className="rounded-md border border-dashed border-border py-12 text-center text-sm text-muted-foreground">
          No lessons yet. Create the first one.
        </div>
      )}

      {/* Lesson list */}
      {lessons && lessons.length > 0 && (
        <ul className="space-y-2">
          {lessons.map((lesson, index) => (
            <li
              key={lesson.id}
              className="flex items-center gap-3 rounded-md border border-border bg-background px-4 py-3"
            >
              {/* Reorder */}
              <div className="flex flex-col">
                <button
                  type="button"
                  aria-label="Move up"
                  disabled={index === 0 || reorderMutation.isPending}
                  onClick={() => handleMove(index, 'up')}
                  className="rounded p-0.5 text-muted-foreground hover:bg-muted disabled:opacity-30"
                >
                  <svg
                    className="h-4 w-4"
                    fill="none"
                    viewBox="0 0 24 24"
                    stroke="currentColor"
                    strokeWidth={2}
                    aria-hidden="true"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      d="m4.5 15.75 7.5-7.5 7.5 7.5"
                    />
                  </svg>
                </button>
                <button
                  type="button"
                  aria-label="Move down"
                  disabled={
                    index === lessons.length - 1 || reorderMutation.isPending
                  }
                  onClick={() => handleMove(index, 'down')}
                  className="rounded p-0.5 text-muted-foreground hover:bg-muted disabled:opacity-30"
                >
                  <svg
                    className="h-4 w-4"
                    fill="none"
                    viewBox="0 0 24 24"
                    stroke="currentColor"
                    strokeWidth={2}
                    aria-hidden="true"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      d="m19.5 8.25-7.5 7.5-7.5-7.5"
                    />
                  </svg>
                </button>
              </div>

              {/* Order badge */}
              <span className="flex h-7 w-7 shrink-0 items-center justify-center rounded-full bg-muted text-xs font-medium text-muted-foreground">
                {lesson.orderIndex + 1}
              </span>

              {/* Info */}
              <div className="flex-1 min-w-0">
                <p className="truncate text-sm font-medium">{lesson.title}</p>
                <p className="text-xs text-muted-foreground">
                  {lesson.contentType}
                  {lesson.isPreview && (
                    <span className="ml-2 rounded bg-green-100 px-1.5 py-0.5 text-xs font-medium text-green-700">
                      Preview
                    </span>
                  )}
                </p>
              </div>

              {/* Actions */}
              <div className="flex items-center gap-2 shrink-0">
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() =>
                    navigate(`/admin/lessons/${lesson.id}/edit`)
                  }
                >
                  Edit
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => handleDelete(lesson)}
                  disabled={deleteMutation.isPending}
                  className="text-destructive hover:text-destructive"
                >
                  Delete
                </Button>
              </div>
            </li>
          ))}
        </ul>
      )}

      {/* Mutation errors */}
      {(deleteMutation.isError || reorderMutation.isError) && (
        <div className="mt-4 rounded-md bg-destructive/10 px-4 py-2 text-sm text-destructive">
          Action failed. Please try again.
        </div>
      )}
    </div>
  )
}
