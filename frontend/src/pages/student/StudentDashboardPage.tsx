import { Link } from 'react-router-dom'
import { BookOpen, Play, ArrowRight } from 'lucide-react'
import { useMyEnrolledCourses } from '@/features/enrollment/api'
import { usePublishedCourses } from '@/features/courses/api'
import type { MyEnrolledCourse } from '@/features/enrollment/types'
import type { CourseSummary } from '@/features/courses/types'
import UserAvatarMenu from '@/components/layout/UserAvatarMenu'

// ─── Progress Bar ─────────────────────────────────────────────────────────────

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

// ─── Hero: Continue Learning ──────────────────────────────────────────────────

function ContinueLearningHero({ courses }: { courses: MyEnrolledCourse[] }) {
  // Prefer a course with a last accessed lesson; fall back to most recently enrolled
  const resumeCourse =
    courses.find((c) => c.lastAccessedLessonId !== null) ?? courses[0]

  if (!resumeCourse) return null

  const href = resumeCourse.lastAccessedLessonId
    ? `/learn/${resumeCourse.courseId}/lessons/${resumeCourse.lastAccessedLessonId}`
    : `/learn/${resumeCourse.courseId}`

  const label = resumeCourse.lastAccessedLessonId ? 'Continue Learning' : 'Start Learning'

  return (
    <div className="relative overflow-hidden rounded-2xl border border-border bg-card shadow-sm">
      <div className="flex flex-col gap-4 p-6 sm:flex-row sm:items-center sm:gap-8">
        {/* Thumbnail */}
        <div className="shrink-0">
          {resumeCourse.thumbnailUrl ? (
            <img
              src={resumeCourse.thumbnailUrl}
              alt={resumeCourse.title}
              className="h-28 w-48 rounded-lg object-cover sm:h-32 sm:w-56"
            />
          ) : (
            <div className="flex h-28 w-48 items-center justify-center rounded-lg bg-muted sm:h-32 sm:w-56">
              <BookOpen className="h-10 w-10 text-muted-foreground" />
            </div>
          )}
        </div>

        {/* Info */}
        <div className="flex flex-1 flex-col gap-3">
          {resumeCourse.category && (
            <span className="text-xs font-medium uppercase tracking-wide text-primary">
              {resumeCourse.category}
            </span>
          )}
          <h2 className="text-xl font-bold leading-snug">{resumeCourse.title}</h2>
          <div className="space-y-1">
            <ProgressBar percent={resumeCourse.progressPercent} />
            <p className="text-sm text-muted-foreground">
              {resumeCourse.completedLessonsCount} / {resumeCourse.totalLessonsCount} lessons
              &nbsp;&bull;&nbsp;{resumeCourse.progressPercent}% complete
            </p>
          </div>
          <Link
            to={href}
            className="mt-1 inline-flex w-fit items-center gap-2 rounded-lg bg-primary px-5 py-2.5 text-sm font-semibold text-primary-foreground hover:bg-primary/90"
          >
            <Play className="h-4 w-4" />
            {label}
          </Link>
        </div>
      </div>
    </div>
  )
}

// ─── Enrolled Course Card ─────────────────────────────────────────────────────

function EnrolledCourseCard({ course }: { course: MyEnrolledCourse }) {
  const href = course.lastAccessedLessonId
    ? `/learn/${course.courseId}/lessons/${course.lastAccessedLessonId}`
    : `/learn/${course.courseId}`
  const label = course.lastAccessedLessonId ? 'Continue' : 'Start'

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
        <h3 className="line-clamp-2 text-sm font-semibold leading-snug">{course.title}</h3>
        <div className="space-y-1">
          <ProgressBar percent={course.progressPercent} />
          <p className="text-xs text-muted-foreground">
            {course.completedLessonsCount} / {course.totalLessonsCount} lessons
            &nbsp;&bull;&nbsp;{course.progressPercent}%
          </p>
        </div>
        <Link
          to={href}
          className="mt-auto inline-flex h-9 items-center justify-center rounded-lg bg-primary px-4 text-sm font-medium text-primary-foreground hover:bg-primary/90"
        >
          {label}
        </Link>
      </div>
    </div>
  )
}

// ─── Browse Course Card ───────────────────────────────────────────────────────

function BrowseCourseCard({ course }: { course: CourseSummary }) {
  return (
    <Link
      to={`/courses/${course.slug}`}
      className="group flex flex-col overflow-hidden rounded-xl border border-border bg-card shadow-sm transition-shadow hover:shadow-md"
    >
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
      <div className="flex flex-1 flex-col gap-2 p-4">
        {course.category && (
          <span className="text-xs font-medium uppercase tracking-wide text-primary">
            {course.category}
          </span>
        )}
        <h3 className="line-clamp-2 text-sm font-semibold leading-snug">{course.title}</h3>
        {course.shortDescription && (
          <p className="line-clamp-2 text-xs text-muted-foreground">{course.shortDescription}</p>
        )}
        <span className="mt-auto inline-flex items-center gap-1 text-xs font-medium text-primary group-hover:underline">
          View Course <ArrowRight className="h-3 w-3" />
        </span>
      </div>
    </Link>
  )
}

// ─── Skeleton ─────────────────────────────────────────────────────────────────

function CourseCardSkeleton() {
  return (
    <div className="overflow-hidden rounded-xl border border-border bg-card">
      <div className="aspect-video w-full animate-pulse bg-muted" />
      <div className="space-y-3 p-4">
        <div className="h-3 w-1/3 animate-pulse rounded bg-muted" />
        <div className="h-4 w-2/3 animate-pulse rounded bg-muted" />
        <div className="h-2 w-full animate-pulse rounded bg-muted" />
        <div className="h-8 w-full animate-pulse rounded bg-muted" />
      </div>
    </div>
  )
}

// ─── Main Page ────────────────────────────────────────────────────────────────

export default function StudentDashboardPage() {
  const {
    data: enrolledCourses,
    isLoading: enrolledLoading,
    isError: enrolledError,
  } = useMyEnrolledCourses()

  const {
    data: publishedData,
    isLoading: browseLoading,
    isError: browseError,
  } = usePublishedCourses({ pageSize: 20 })

  const enrolledIds = new Set((enrolledCourses ?? []).map((c) => c.courseId))
  const browseCourses = (publishedData?.items ?? [])
    .filter((c) => !enrolledIds.has(c.id))
    .slice(0, 6)

  return (
    <div className="mx-auto max-w-7xl space-y-10 px-4 py-8 sm:px-6 lg:px-8">

      {/* ── Page header ── */}
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Dashboard</h1>
        <UserAvatarMenu />
      </div>

      {/* ── Hero: Continue Learning ── */}
      {!enrolledLoading && !enrolledError && (enrolledCourses?.length ?? 0) > 0 && (
        <section>
          <h2 className="mb-4 text-lg font-semibold">Continue Learning</h2>
          <ContinueLearningHero courses={enrolledCourses!} />
        </section>
      )}

      {/* ── My Courses ── */}
      <section>
        <div className="mb-4 flex items-center justify-between">
          <h2 className="text-lg font-semibold">My Courses</h2>
          {(enrolledCourses?.length ?? 0) > 0 && (
            <Link to="/my-learning" className="text-sm text-primary hover:underline">
              View all
            </Link>
          )}
        </div>

        {enrolledLoading && (
          <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
            {Array.from({ length: 3 }).map((_, i) => (
              <CourseCardSkeleton key={i} />
            ))}
          </div>
        )}

        {enrolledError && (
          <p className="text-sm text-destructive">Failed to load your courses.</p>
        )}

        {!enrolledLoading && !enrolledError && (enrolledCourses?.length ?? 0) === 0 && (
          <div className="flex flex-col items-center gap-4 rounded-xl border border-dashed border-border py-16 text-center">
            <BookOpen className="h-12 w-12 text-muted-foreground" />
            <p className="text-muted-foreground">You haven't enrolled in any courses yet.</p>
            <Link
              to="/courses"
              className="inline-flex h-9 items-center rounded-lg bg-primary px-4 text-sm font-medium text-primary-foreground hover:bg-primary/90"
            >
              Browse Courses
            </Link>
          </div>
        )}

        {!enrolledLoading && !enrolledError && (enrolledCourses?.length ?? 0) > 0 && (
          <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
            {enrolledCourses!.map((course) => (
              <EnrolledCourseCard key={course.enrollmentId} course={course} />
            ))}
          </div>
        )}
      </section>

      {/* ── Browse All Courses ── */}
      {!browseLoading && !browseError && browseCourses.length > 0 && (
        <section>
          <div className="mb-4 flex items-center justify-between">
            <h2 className="text-lg font-semibold">Browse All Courses</h2>
            <Link to="/courses" className="text-sm text-primary hover:underline">
              See all
            </Link>
          </div>
          <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
            {browseCourses.map((course) => (
              <BrowseCourseCard key={course.id} course={course} />
            ))}
          </div>
        </section>
      )}

      {browseLoading && (
        <section>
          <h2 className="mb-4 text-lg font-semibold">Browse All Courses</h2>
          <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
            {Array.from({ length: 3 }).map((_, i) => (
              <CourseCardSkeleton key={i} />
            ))}
          </div>
        </section>
      )}

      {browseError && (
        <p className="text-sm text-destructive">Failed to load available courses.</p>
      )}
    </div>
  )
}
