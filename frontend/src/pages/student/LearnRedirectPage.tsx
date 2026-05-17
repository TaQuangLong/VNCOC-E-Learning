import { useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useCourseLessons } from '@/features/lessons/api'
import { useEnrollmentStatus } from '@/features/enrollment/api'

/**
 * Handles /learn/:courseId — redirects to the first lesson.
 */
export default function LearnRedirectPage() {
  const { courseId: courseIdParam } = useParams<{ courseId: string }>()
  const courseId = Number(courseIdParam)
  const navigate = useNavigate()

  const { data: lessons, isLoading, isError } = useCourseLessons(courseId)
  const { data: enrollmentStatus, isLoading: enrollmentLoading } =
    useEnrollmentStatus(courseId)

  // Redirect unenrolled students back to courses
  useEffect(() => {
    if (!enrollmentLoading && enrollmentStatus?.isEnrolled === false) {
      navigate('/courses', { replace: true })
    }
  }, [enrollmentStatus, enrollmentLoading, navigate])

  useEffect(() => {
    if (!isLoading && lessons && lessons.length > 0) {
      navigate(`/learn/${courseId}/lessons/${lessons[0].id}`, { replace: true })
    }
  }, [lessons, isLoading, courseId, navigate])

  if (isLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <span className="text-sm text-muted-foreground">Loading course…</span>
      </div>
    )
  }

  if (isError) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <p className="text-sm text-destructive">Failed to load course.</p>
      </div>
    )
  }

  if (!isLoading && lessons?.length === 0) {
    return (
      <div className="flex min-h-screen flex-col items-center justify-center gap-2">
        <p className="text-sm text-muted-foreground">
          This course has no lessons yet.
        </p>
      </div>
    )
  }

  return null
}
