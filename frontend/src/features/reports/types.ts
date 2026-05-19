export interface PopularCourse {
  courseId: number
  title: string
  slug: string
  enrollmentCount: number
}

export interface RecentUser {
  userId: string
  displayName: string
  email: string
  registeredAt: string
}

export interface AdminOverviewResponse {
  totalUsers: number
  totalPublishedCourses: number
  totalActiveEnrollments: number
  totalQuizAttempts: number
  recentRegistrationsLast7Days: number
  mostPopularCourses: PopularCourse[]
  recentRegistrations: RecentUser[]
}

export interface CourseLearner {
  userId: string
  displayName: string
  email: string
  enrolledAt: string
  progressPercent: number
  completedLessonsCount: number
  totalLessonsCount: number
  quizPassedCount: number
  completedAt: string | null
}

export interface CourseLearnersResponse {
  courseId: number
  courseTitle: string
  items: CourseLearner[]
  totalCount: number
  page: number
  pageSize: number
}

export interface UserQuizResult {
  quizId: number
  quizTitle: string
  lessonId: number
  lessonTitle: string
  score: number
  passed: boolean
  submittedAt: string
}

export interface UserCourseProgress {
  courseId: number
  courseTitle: string
  courseSlug: string
  enrolledAt: string
  progressPercent: number
  completedLessonsCount: number
  totalLessonsCount: number
  isCompleted: boolean
  completedAt: string | null
  quizResults: UserQuizResult[]
}

export interface UserProgressResponse {
  userId: string
  displayName: string
  email: string
  courses: UserCourseProgress[]
}
