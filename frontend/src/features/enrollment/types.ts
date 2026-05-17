export interface EnrollmentStatus {
  isEnrolled: boolean
  enrollmentId: number | null
  progressPercent: number | null
  lastAccessedLessonId: number | null
}

export interface MyEnrolledCourse {
  enrollmentId: number
  courseId: number
  title: string
  slug: string
  thumbnailUrl: string | null
  category: string | null
  progressPercent: number
  completedLessonsCount: number
  totalLessonsCount: number
  lastAccessedLessonId: number | null
  enrolledAt: string
}

export interface EnrollCourseResponse {
  enrollmentId: number
  courseId: number
  enrolledAt: string
  totalLessonsCount: number
}
