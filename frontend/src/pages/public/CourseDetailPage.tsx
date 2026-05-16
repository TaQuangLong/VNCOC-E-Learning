import { Link, useParams } from 'react-router-dom'
import { useCourseBySlug } from '@/features/courses/api'

export default function CourseDetailPage() {
  const { slug } = useParams<{ slug: string }>()
  const { data: course, isLoading, isError } = useCourseBySlug(slug ?? '')

  if (isLoading) {
    return (
      <div className="mx-auto max-w-4xl animate-pulse space-y-4 px-4 py-8 sm:px-6 lg:px-8">
        <div className="h-64 w-full rounded-xl bg-muted" />
        <div className="h-8 w-2/3 rounded bg-muted" />
        <div className="h-4 w-full rounded bg-muted" />
        <div className="h-4 w-5/6 rounded bg-muted" />
      </div>
    )
  }

  if (isError || !course) {
    return (
      <div className="mx-auto max-w-4xl px-4 py-20 text-center sm:px-6">
        <p className="mb-4 text-muted-foreground">Course not found.</p>
        <Link
          to="/courses"
          className="text-sm text-primary hover:underline"
        >
          ← Back to catalog
        </Link>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-background">
      {/* Hero */}
      <div className="border-b border-border bg-muted/50">
        <div className="mx-auto max-w-4xl px-4 py-10 sm:px-6 lg:px-8">
          <div className="grid gap-8 md:grid-cols-2 md:items-start">
            <div className="space-y-4">
              {course.category && (
                <span className="text-xs font-medium uppercase tracking-wide text-primary">
                  {course.category}
                </span>
              )}
              <h1 className="text-2xl font-bold sm:text-3xl">{course.title}</h1>
              {course.shortDescription && (
                <p className="text-muted-foreground">{course.shortDescription}</p>
              )}
              <div className="flex flex-wrap gap-2 text-sm">
                {course.level && (
                  <span className="rounded-full border border-border px-2.5 py-0.5 text-muted-foreground">
                    {course.level}
                  </span>
                )}
                {course.language && (
                  <span className="rounded-full border border-border px-2.5 py-0.5 text-muted-foreground">
                    {course.language}
                  </span>
                )}
              </div>
              {/* Enroll button — Sprint 7 */}
              <button
                disabled
                className="mt-2 inline-flex h-9 cursor-not-allowed items-center rounded-lg bg-primary px-4 text-sm font-medium text-primary-foreground opacity-50"
              >
                Enroll — Coming Soon
              </button>
            </div>

            {course.thumbnailUrl ? (
              <img
                src={course.thumbnailUrl}
                alt={course.title}
                className="w-full rounded-xl object-cover"
              />
            ) : (
              <div className="flex aspect-video w-full items-center justify-center rounded-xl bg-muted text-sm text-muted-foreground">
                No thumbnail
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Body */}
      <div className="mx-auto max-w-4xl space-y-8 px-4 py-8 sm:px-6 lg:px-8">
        {/* Description */}
        {course.description && (
          <section>
            <h2 className="mb-3 text-lg font-semibold">About This Course</h2>
            <p className="whitespace-pre-wrap text-sm text-muted-foreground">
              {course.description}
            </p>
          </section>
        )}

        {/* Instructor */}
        <section>
          <h2 className="mb-3 text-lg font-semibold">Instructor</h2>
          <div className="flex items-start gap-4 rounded-xl border border-border p-4">
            {course.authorAvatarUrl ? (
              <img
                src={course.authorAvatarUrl}
                alt={course.authorName}
                className="h-12 w-12 rounded-full object-cover"
              />
            ) : (
              <div className="flex h-12 w-12 shrink-0 items-center justify-center rounded-full bg-muted font-medium text-muted-foreground">
                {course.authorName.charAt(0).toUpperCase()}
              </div>
            )}
            <div>
              <p className="font-medium">{course.authorName}</p>
              {course.authorBio && (
                <p className="mt-1 text-sm text-muted-foreground">
                  {course.authorBio}
                </p>
              )}
            </div>
          </div>
        </section>

        {/* Lessons placeholder */}
        <section>
          <h2 className="mb-3 text-lg font-semibold">Lessons</h2>
          <div className="rounded-xl border border-border p-8 text-center text-sm text-muted-foreground">
            Lessons will be available in a future sprint.
          </div>
        </section>

        <div>
          <Link to="/courses" className="text-sm text-primary hover:underline">
            ← Back to catalog
          </Link>
        </div>
      </div>
    </div>
  )
}
