import { Link, useParams } from 'react-router-dom'
import {
  ArrowLeft,
  ArrowRight,
  BookOpen,
  CheckCircle2,
  Clock,
  Route,
} from 'lucide-react'

import CatalogNavigation from '@/components/layout/CatalogNavigation'
import { useLearningPathBySlug } from '@/features/learning-paths/api'
import type {
  LearningPathCourseDetail,
  LearningPathProgress,
} from '@/features/learning-paths/types'

function ProgressBar({ percent }: { percent: number }) {
  const normalizedPercent = Math.min(100, Math.max(0, percent))

  return (
    <div
      className="h-2 w-full overflow-hidden rounded-full bg-muted"
      role="progressbar"
      aria-label="Learning path progress"
      aria-valuemin={0}
      aria-valuemax={100}
      aria-valuenow={normalizedPercent}
    >
      <div
        className="h-full rounded-full bg-primary transition-all"
        style={{ width: `${normalizedPercent}%` }}
      />
    </div>
  )
}

function PathProgress({ progress }: { progress: LearningPathProgress }) {
  return (
    <div className="rounded-xl border bg-background/80 p-4 shadow-sm backdrop-blur">
      <div className="mb-3 flex items-end justify-between gap-4">
        <div>
          <p className="text-sm font-medium">Your progress</p>
          <p className="mt-0.5 text-sm text-muted-foreground">
            {progress.completedCoursesCount} of {progress.totalCoursesCount}{' '}
            courses complete
          </p>
        </div>
        <span className="text-lg font-semibold">{progress.progressPercent}%</span>
      </div>
      <ProgressBar percent={progress.progressPercent} />
    </div>
  )
}

function CourseProgressBadge({
  course,
}: {
  course: LearningPathCourseDetail
}) {
  if (course.isCompleted) {
    return (
      <span className="inline-flex items-center gap-1.5 rounded-full bg-green-100 px-2.5 py-1 text-xs font-medium text-green-800">
        <CheckCircle2 className="size-3.5" aria-hidden="true" />
        Completed
      </span>
    )
  }

  if (course.isEnrolled) {
    return (
      <span className="rounded-full bg-primary/10 px-2.5 py-1 text-xs font-medium text-primary">
        Enrolled - {course.progressPercent ?? 0}% complete
      </span>
    )
  }

  return (
    <span className="rounded-full bg-muted px-2.5 py-1 text-xs font-medium text-muted-foreground">
      Enroll to start
    </span>
  )
}

function LearningPathCourseCard({
  course,
  number,
  showProgress,
}: {
  course: LearningPathCourseDetail
  number: number
  showProgress: boolean
}) {
  return (
    <Link
      to={`/courses/${course.slug}`}
      className="group flex flex-col overflow-hidden rounded-xl border bg-card shadow-sm transition-shadow hover:shadow-md sm:flex-row"
    >
      <div className="relative aspect-video overflow-hidden bg-muted sm:aspect-auto sm:w-56 sm:shrink-0">
        {course.thumbnailUrl ? (
          <img
            src={course.thumbnailUrl}
            alt={course.title}
            className="h-full w-full object-cover transition-transform duration-300 group-hover:scale-105"
          />
        ) : (
          <div className="flex h-full min-h-36 items-center justify-center">
            <BookOpen
              className="size-10 text-muted-foreground"
              aria-hidden="true"
            />
          </div>
        )}
        <span className="absolute top-3 left-3 flex size-8 items-center justify-center rounded-full bg-background/90 text-xs font-semibold shadow-sm">
          {number}
        </span>
      </div>

      <div className="flex min-w-0 flex-1 flex-col gap-3 p-5">
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div className="min-w-0">
            <h3 className="font-semibold leading-snug transition-colors group-hover:text-primary">
              {course.title}
            </h3>
            <div className="mt-2 flex flex-wrap gap-3 text-xs text-muted-foreground">
              {course.level && <span>{course.level}</span>}
              <span>
                {course.lessonCount}{' '}
                {course.lessonCount === 1 ? 'lesson' : 'lessons'}
              </span>
            </div>
          </div>
          {showProgress && <CourseProgressBadge course={course} />}
        </div>

        {course.shortDescription && (
          <p className="line-clamp-2 text-sm text-muted-foreground">
            {course.shortDescription}
          </p>
        )}

        <span className="mt-auto inline-flex items-center gap-1 text-sm font-medium text-primary">
          View course
          <ArrowRight
            className="size-4 transition-transform group-hover:translate-x-1"
            aria-hidden="true"
          />
        </span>
      </div>
    </Link>
  )
}

function LearningPathDetailSkeleton() {
  return (
    <div className="min-h-screen animate-pulse">
      <div className="border-b bg-muted/40">
        <div className="mx-auto grid max-w-6xl gap-8 px-4 py-10 sm:px-6 md:grid-cols-2 lg:px-8">
          <div className="space-y-4">
            <div className="h-4 w-32 rounded bg-muted" />
            <div className="h-10 w-4/5 rounded bg-muted" />
            <div className="h-4 w-full rounded bg-muted" />
            <div className="h-4 w-3/4 rounded bg-muted" />
          </div>
          <div className="aspect-video rounded-xl bg-muted" />
        </div>
      </div>
      <div className="mx-auto max-w-4xl space-y-5 px-4 py-10 sm:px-6 lg:px-8">
        <div className="h-7 w-48 rounded bg-muted" />
        {Array.from({ length: 3 }).map((_, index) => (
          <div key={index} className="h-44 rounded-xl bg-muted" />
        ))}
      </div>
    </div>
  )
}

export default function LearningPathDetailPage() {
  const { slug } = useParams<{ slug: string }>()
  const {
    data: learningPath,
    isPending,
    isError,
  } = useLearningPathBySlug(slug ?? '')

  if (isPending) {
    return <LearningPathDetailSkeleton />
  }

  if (isError || !learningPath) {
    return (
      <div className="mx-auto max-w-4xl px-4 py-20 text-center sm:px-6">
        <Route
          className="mx-auto mb-4 size-12 text-muted-foreground"
          aria-hidden="true"
        />
        <h1 className="text-xl font-semibold">Learning path not found</h1>
        <p className="mt-2 text-sm text-muted-foreground">
          This path may be unavailable or no longer published.
        </p>
        <Link
          to="/learning-paths"
          className="mt-5 inline-flex items-center gap-2 text-sm font-medium text-primary hover:underline"
        >
          <ArrowLeft className="size-4" aria-hidden="true" />
          Back to learning paths
        </Link>
      </div>
    )
  }

  const totalCourses = learningPath.sections.reduce(
    (count, section) => count + section.courses.length,
    0,
  )

  return (
    <div className="min-h-screen bg-background">
      <div className="border-b bg-muted/40">
        <div className="mx-auto max-w-6xl px-4 py-8 sm:px-6 lg:px-8">
          <div className="mb-6 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
            <Link
              to="/learning-paths"
              className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground"
            >
              <ArrowLeft className="size-4" aria-hidden="true" />
              Learning paths
            </Link>
            <CatalogNavigation />
          </div>

          <div className="grid gap-8 md:grid-cols-2 md:items-center">
            <div className="space-y-5">
              <div className="space-y-3">
                <span className="text-xs font-medium uppercase tracking-wide text-primary">
                  Guided Learning Path
                </span>
                <h1 className="text-3xl font-bold tracking-tight sm:text-4xl">
                  {learningPath.title}
                </h1>
                {learningPath.shortDescription && (
                  <p className="text-lg text-muted-foreground">
                    {learningPath.shortDescription}
                  </p>
                )}
              </div>

              <div className="flex flex-wrap gap-4 text-sm text-muted-foreground">
                <span className="inline-flex items-center gap-2">
                  <BookOpen className="size-4" aria-hidden="true" />
                  {totalCourses} {totalCourses === 1 ? 'course' : 'courses'}
                </span>
                {learningPath.estimatedDurationLabel && (
                  <span className="inline-flex items-center gap-2">
                    <Clock className="size-4" aria-hidden="true" />
                    {learningPath.estimatedDurationLabel}
                  </span>
                )}
              </div>

              {learningPath.progress && (
                <PathProgress progress={learningPath.progress} />
              )}
            </div>

            <div className="aspect-video overflow-hidden rounded-2xl bg-muted shadow-sm">
              {learningPath.thumbnailUrl ? (
                <img
                  src={learningPath.thumbnailUrl}
                  alt={learningPath.title}
                  className="h-full w-full object-cover"
                />
              ) : (
                <div className="flex h-full items-center justify-center">
                  <Route
                    className="size-16 text-muted-foreground"
                    aria-hidden="true"
                  />
                </div>
              )}
            </div>
          </div>
        </div>
      </div>

      <main className="mx-auto max-w-4xl space-y-10 px-4 py-10 sm:px-6 lg:px-8">
        {learningPath.description && (
          <section>
            <h2 className="mb-3 text-xl font-semibold">About this path</h2>
            <p className="whitespace-pre-wrap text-sm leading-6 text-muted-foreground">
              {learningPath.description}
            </p>
          </section>
        )}

        {learningPath.sections.length === 0 && (
          <div className="rounded-xl border border-dashed p-10 text-center">
            <BookOpen
              className="mx-auto mb-3 size-10 text-muted-foreground"
              aria-hidden="true"
            />
            <h2 className="font-semibold">No courses in this path yet</h2>
            <p className="mt-1 text-sm text-muted-foreground">
              Browse the course catalog while this path is being prepared.
            </p>
            <Link
              to="/courses"
              className="mt-4 inline-flex text-sm font-medium text-primary hover:underline"
            >
              Browse Courses
            </Link>
          </div>
        )}

        {learningPath.sections.map((section, sectionIndex) => (
          <section key={section.id}>
            <div className="mb-4">
              <p className="text-xs font-medium uppercase tracking-wide text-primary">
                Section {sectionIndex + 1}
              </p>
              <h2 className="mt-1 text-xl font-semibold">{section.title}</h2>
              {section.description && (
                <p className="mt-2 text-sm text-muted-foreground">
                  {section.description}
                </p>
              )}
            </div>

            {section.courses.length === 0 ? (
              <div className="rounded-xl border border-dashed p-6 text-center text-sm text-muted-foreground">
                No courses in this section.
              </div>
            ) : (
              <div className="space-y-4">
                {section.courses.map((course, courseIndex) => (
                  <LearningPathCourseCard
                    key={course.id}
                    course={course}
                    number={
                      learningPath.sections
                        .slice(0, sectionIndex)
                        .reduce(
                          (count, previousSection) =>
                            count + previousSection.courses.length,
                          0,
                        ) +
                      courseIndex +
                      1
                    }
                    showProgress={learningPath.progress !== undefined}
                  />
                ))}
              </div>
            )}
          </section>
        ))}
      </main>
    </div>
  )
}
