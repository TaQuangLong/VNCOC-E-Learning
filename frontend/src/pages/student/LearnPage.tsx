import { Link, useNavigate, useParams } from 'react-router-dom'
import { useEffect, useState } from 'react'
import { useCourseLessons, useLessonDetail } from '@/features/lessons/api'
import { useEnrollmentStatus } from '@/features/enrollment/api'
import { useCourseProgress, useMarkLessonComplete } from '@/features/progress/api'
import LessonSidebar from '@/features/lessons/LessonSidebar'
import ResourcesSection from '@/features/lessons/ResourcesSection'
import YouTubePlayer from '@/features/lessons/players/YouTubePlayer'
import TextRenderer from '@/features/lessons/players/TextRenderer'
import PdfLink from '@/features/lessons/players/PdfLink'
import { Button } from '@/components/ui/button'
import { useLessonQuiz } from '@/features/quiz/api'
import QuizPlayer from '@/features/quiz/QuizPlayer'

export default function LearnPage() {
  const { courseId: courseIdParam, lessonId: lessonIdParam } = useParams<{
    courseId: string
    lessonId: string
  }>()
  const courseId = Number(courseIdParam)
  const lessonId = Number(lessonIdParam)
  const navigate = useNavigate()

  const { data: lessons, isLoading: lessonsLoading } =
    useCourseLessons(courseId)
  const {
    data: lesson,
    isLoading: lessonLoading,
    isError: lessonError,
  } = useLessonDetail(lessonId)
  const { data: enrollmentStatus, isLoading: enrollmentLoading } =
    useEnrollmentStatus(courseId)
  const { data: courseProgress } = useCourseProgress(courseId)
  const markComplete = useMarkLessonComplete(courseId)

  // Redirect unenrolled students back to courses
  useEffect(() => {
    if (!enrollmentLoading && enrollmentStatus?.isEnrolled === false) {
      navigate('/courses', { replace: true })
    }
  }, [enrollmentStatus, enrollmentLoading, navigate])

  const currentIndex = lessons?.findIndex((l) => l.id === lessonId) ?? -1
  const prevLesson = currentIndex > 0 ? lessons![currentIndex - 1] : null
  const nextLesson =
    lessons && currentIndex < lessons.length - 1
      ? lessons[currentIndex + 1]
      : null

  const completedLessonIds =
    courseProgress?.lessons.filter((l) => l.isCompleted).map((l) => l.lessonId) ?? []
  const isCurrentLessonCompleted = completedLessonIds.includes(lessonId)

  const [activeTab, setActiveTab] = useState<'content' | 'quiz'>('content')

  return (
    <div className="flex min-h-screen flex-col bg-background">
      {/* Top bar */}
      <header className="flex items-center gap-4 border-b border-border bg-background px-4 py-3 lg:px-6">
        <Link
          to={`/courses`}
          className="flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground"
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
              d="M15.75 19.5 8.25 12l7.5-7.5"
            />
          </svg>
          Courses
        </Link>
        {lesson && (
          <span className="truncate text-sm font-medium text-foreground">
            {lesson.title}
          </span>
        )}
      </header>

      {/* Body: sidebar + content */}
      <div className="flex flex-1 flex-col lg:flex-row">
        {/* Sidebar (mobile drawer + desktop) */}
        {!lessonsLoading && lessons && lessons.length > 0 && (
          <LessonSidebar
            courseId={courseId}
            lessons={lessons}
            activeLessonId={lessonId}
            completedLessonIds={completedLessonIds}
          />
        )}

        {/* Main content */}
        <main className="flex-1 overflow-y-auto px-4 py-6 sm:px-6 lg:px-8">
          {/* Lesson loading skeleton */}
          {lessonLoading && (
            <div className="mx-auto max-w-3xl space-y-4">
              <div className="aspect-video w-full animate-pulse rounded-md bg-muted" />
              <div className="h-6 w-1/2 animate-pulse rounded bg-muted" />
              <div className="h-4 w-3/4 animate-pulse rounded bg-muted" />
              <div className="h-4 w-2/3 animate-pulse rounded bg-muted" />
            </div>
          )}

          {/* Lesson error */}
          {lessonError && (
            <div className="mx-auto max-w-3xl rounded-md bg-destructive/10 px-4 py-3 text-sm text-destructive">
              Failed to load lesson. You may need to enroll in this course.
            </div>
          )}

          {/* Lesson content */}
          {lesson && (
            <div className="mx-auto max-w-3xl">
              <h1 className="mb-4 text-xl font-bold sm:text-2xl">
                {lesson.title}
              </h1>

              {/* Tab bar */}
              <div className="mb-6 flex gap-1 border-b border-border">
                <TabButton
                  label="Content"
                  isActive={activeTab === 'content'}
                  onClick={() => setActiveTab('content')}
                />
                <TabButton
                  label="Quiz"
                  isActive={activeTab === 'quiz'}
                  onClick={() => setActiveTab('quiz')}
                />
              </div>

              {/* Content tab */}
              {activeTab === 'content' && (
                <>
                  {/* Content player */}
                  {lesson.contentType === 'Video' &&
                    (lesson.youTubeUrl ? (
                      <YouTubePlayer url={lesson.youTubeUrl} />
                    ) : (
                      <div className="flex aspect-video items-center justify-center rounded-md bg-muted text-sm text-muted-foreground">
                        Video content not available.
                      </div>
                    ))}
                  {lesson.contentType === 'Text' &&
                    (lesson.textContent ? (
                      <TextRenderer content={lesson.textContent} />
                    ) : (
                      <p className="text-sm text-muted-foreground">Text content not available.</p>
                    ))}
                  {lesson.contentType === 'Pdf' &&
                    (lesson.pdfUrl ? (
                      <PdfLink url={lesson.pdfUrl} title={lesson.title} />
                    ) : (
                      <p className="text-sm text-muted-foreground">PDF content not available.</p>
                    ))}

                  {/* Description */}
                  {lesson.description && (
                    <p className="mt-4 text-sm leading-relaxed text-muted-foreground">
                      {lesson.description}
                    </p>
                  )}

                  {/* Resources */}
                  <ResourcesSection resources={lesson.resources} />

                  {/* Mark as Completed */}
                  <div className="mt-6">
                    {isCurrentLessonCompleted ? (
                      <div className="flex items-center gap-2 rounded-md bg-green-50 px-4 py-2.5 text-sm font-medium text-green-700 dark:bg-green-950/30 dark:text-green-400">
                        <svg
                          className="h-4 w-4 shrink-0"
                          fill="none"
                          viewBox="0 0 24 24"
                          stroke="currentColor"
                          strokeWidth={2.5}
                          aria-hidden="true"
                        >
                          <path strokeLinecap="round" strokeLinejoin="round" d="M5 13l4 4L19 7" />
                        </svg>
                        Lesson completed
                      </div>
                    ) : (
                      <div className="flex flex-col gap-2">
                        <Button
                          onClick={() => markComplete.mutate(lessonId)}
                          disabled={markComplete.isPending}
                        >
                          {markComplete.isPending ? 'Saving…' : 'Mark as Completed'}
                        </Button>
                        {markComplete.isError && (
                          <p className="text-xs text-destructive">
                            Failed to save progress. Please try again.
                          </p>
                        )}
                      </div>
                    )}
                  </div>
                </>
              )}

              {/* Quiz tab */}
              {activeTab === 'quiz' && (
                <QuizTab lessonId={lessonId} courseId={courseId} />
              )}

              {/* Navigation (always visible) */}
              <div className="mt-8 flex items-center justify-between border-t border-border pt-6">
                <div>
                  {prevLesson && (
                    <Button
                      variant="outline"
                      onClick={() =>
                        navigate(
                          `/learn/${courseId}/lessons/${prevLesson.id}`,
                        )
                      }
                    >
                      ← Previous
                    </Button>
                  )}
                </div>
                <div>
                  {nextLesson && (
                    <Button
                      onClick={() =>
                        navigate(
                          `/learn/${courseId}/lessons/${nextLesson.id}`,
                        )
                      }
                    >
                      Next →
                    </Button>
                  )}
                </div>
              </div>
            </div>
          )}

          {/* Empty course */}
          {!lessonsLoading && lessons?.length === 0 && (
            <div className="mx-auto max-w-3xl py-16 text-center text-sm text-muted-foreground">
              This course has no lessons yet.
            </div>
          )}
        </main>
      </div>
    </div>
  )
}

// ─── Tab helpers ─────────────────────────────────────────────────────────────────

function TabButton({
  label,
  isActive,
  onClick,
}: {
  label: string
  isActive: boolean
  onClick: () => void
}) {
  return (
    <button
      type="button"
      onClick={onClick}
      className={`px-4 py-2 text-sm font-medium transition-colors ${
        isActive
          ? 'border-b-2 border-primary text-foreground'
          : 'text-muted-foreground hover:text-foreground'
      }`}
    >
      {label}
    </button>
  )
}

function QuizTab({
  lessonId,
  courseId,
}: {
  lessonId: number
  courseId: number
}) {
  const { data: quiz, isLoading, isError } = useLessonQuiz(lessonId)

  if (isLoading) {
    return (
      <div className="space-y-3">
        <div className="h-6 w-1/2 animate-pulse rounded bg-muted" />
        <div className="h-4 w-3/4 animate-pulse rounded bg-muted" />
        <div className="h-4 w-2/3 animate-pulse rounded bg-muted" />
      </div>
    )
  }

  if (isError || !quiz) {
    return (
      <div className="rounded-md border border-dashed border-border py-12 text-center text-sm text-muted-foreground">
        No quiz available for this lesson.
      </div>
    )
  }

  return <QuizPlayer quiz={quiz} courseId={courseId} />
}
