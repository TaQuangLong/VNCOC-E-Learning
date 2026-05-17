import { Link, useNavigate, useParams } from 'react-router-dom'
import { useEffect } from 'react'
import { useCourseLessons, useLessonDetail } from '@/features/lessons/api'
import { useEnrollmentStatus } from '@/features/enrollment/api'
import LessonSidebar from '@/features/lessons/LessonSidebar'
import ResourcesSection from '@/features/lessons/ResourcesSection'
import YouTubePlayer from '@/features/lessons/players/YouTubePlayer'
import TextRenderer from '@/features/lessons/players/TextRenderer'
import PdfLink from '@/features/lessons/players/PdfLink'
import { Button } from '@/components/ui/button'

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

              {/* Navigation */}
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
