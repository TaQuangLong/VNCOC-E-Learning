import { Link, useNavigate } from 'react-router-dom'
import { useMyEnrolledCourses } from '@/features/enrollment/api'
import type { MyEnrolledCourse } from '@/features/enrollment/types'
import { BookOpen } from 'lucide-react'

function ProgressBar({ percent }: { percent: number }) {
  return (
    <div className="h-1.5 w-full overflow-hidden rounded-full bg-muted">
      <div
        className="h-full rounded-full bg-primary transition-all"
        style={{ width: `${Math.min(100, Math.max(0, percent))}%` }}
      />
    </div>
  )
}

function EnrolledCourseCard({ course }: { course: MyEnrolledCourse }) {
  const learnHref = course.lastAccessedLessonId
    ? `/learn/${course.courseId}/lessons/${course.lastAccessedLessonId}`
    : `/learn/${course.courseId}`

  return (
    <div className="flex flex-col overflow-hidden rounded-xl border border-border bg-card shadow-sm">
      {course.thumbnailUrl ? (
        <img
          src={course.thumbnailUrl}
          alt={course.title}
          className="aspect-video w-full object-cover"
        />
      ) : (
        <div className="flex aspect-video w-full items-center justify-center bg-muted">
          <BookOpen className="h-10 w-10 text-muted-foreground" />
        </div>
      )}

      <div className="flex flex-1 flex-col gap-3 p-4">
        {course.category && (
          <span className="text-xs font-medium uppercase tracking-wide text-primary">
            {course.category}
          </span>
        )}

        <h2 className="line-clamp-2 text-sm font-semibold leading-snug">
          {course.title}
        </h2>

        <div className="space-y-1">
          <ProgressBar percent={course.progressPercent} />
          <p className="text-xs text-muted-foreground">
            {course.completedLessonsCount} / {course.totalLessonsCount} lessons
            &nbsp;&bull;&nbsp;{course.progressPercent}% complete
          </p>
        </div>

        <Link
          to={learnHref}
          className="mt-auto inline-flex h-9 items-center justify-center rounded-lg bg-primary px-4 text-sm font-medium text-primary-foreground hover:bg-primary/90"
        >
          Continue Learning
        </Link>
      </div>
    </div>
  )
}

export default function MyLearningPage() {
  const navigate = useNavigate()
  const { data: courses, isLoading, isError } = useMyEnrolledCourses()

  if (isLoading) {
    return (
      <div className="mx-auto max-w-5xl px-4 py-8 sm:px-6 lg:px-8">
        <h1 className="mb-6 text-2xl font-bold">My Learning</h1>
        <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {Array.from({ length: 3 }).map((_, i) => (
            <div key={i} className="overflow-hidden rounded-xl border border-border">
              <div className="aspect-video w-full animate-pulse bg-muted" />
              <div className="space-y-3 p-4">
                <div className="h-4 w-2/3 animate-pulse rounded bg-muted" />
                <div className="h-3 w-full animate-pulse rounded bg-muted" />
                <div className="h-8 w-full animate-pulse rounded bg-muted" />
              </div>
            </div>
          ))}
        </div>
      </div>
    )
  }

  if (isError) {
    return (
      <div className="mx-auto max-w-5xl px-4 py-20 text-center sm:px-6">
        <p className="text-sm text-destructive">Failed to load your courses.</p>
      </div>
    )
  }

  if (!courses || courses.length === 0) {
    return (
      <div className="mx-auto max-w-5xl px-4 py-8 sm:px-6 lg:px-8">
        <h1 className="mb-6 text-2xl font-bold">My Learning</h1>
        <div className="flex flex-col items-center gap-4 rounded-xl border border-dashed border-border py-20 text-center">
          <BookOpen className="h-12 w-12 text-muted-foreground" />
          <p className="text-muted-foreground">You haven't enrolled in any courses yet.</p>
          <button
            onClick={() => navigate('/courses')}
            className="inline-flex h-9 items-center rounded-lg bg-primary px-4 text-sm font-medium text-primary-foreground hover:bg-primary/90"
          >
            Browse Courses
          </button>
        </div>
      </div>
    )
  }

  return (
    <div className="mx-auto max-w-5xl px-4 py-8 sm:px-6 lg:px-8">
      <h1 className="mb-6 text-2xl font-bold">My Learning</h1>
      <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
        {courses.map((course) => (
          <EnrolledCourseCard key={course.enrollmentId} course={course} />
        ))}
      </div>
    </div>
  )
}
