export interface LessonProgressItem {
  lessonId: number
  isCompleted: boolean
  completedAt: string | null
  videoProgressPercent: number
}

export interface CourseProgress {
  courseId: number
  progressPercent: number
  completedLessonsCount: number
  totalLessonsCount: number
  lessons: LessonProgressItem[]
}

export interface MarkLessonCompleteResponse {
  lessonId: number
  courseId: number
  isCompleted: boolean
  progressPercent: number
  completedLessonsCount: number
  totalLessonsCount: number
}

export interface SaveVideoProgressRequest {
  videoProgressPercent: number
  videoWatchedSeconds: number
}
