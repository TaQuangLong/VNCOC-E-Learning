import { useQuery } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type {
  AdminOverviewResponse,
  CourseLearnersResponse,
  UserProgressResponse,
} from './types'

export const reportKeys = {
  overview: () => ['reports', 'overview'] as const,
  courseLearners: (courseId: number, page: number) =>
    ['reports', 'course-learners', courseId, page] as const,
  userProgress: (userId: string) =>
    ['reports', 'user-progress', userId] as const,
}

// GET /api/admin/reports/overview
export function useAdminOverview() {
  return useQuery({
    queryKey: reportKeys.overview(),
    queryFn: () =>
      apiClient
        .get<AdminOverviewResponse>('/admin/reports/overview')
        .then((r) => r.data),
  })
}

// GET /api/admin/reports/courses/{courseId}/learners
export function useCourseLearners(
  courseId: number,
  page: number,
  pageSize: number,
) {
  return useQuery({
    queryKey: reportKeys.courseLearners(courseId, page),
    queryFn: () =>
      apiClient
        .get<CourseLearnersResponse>(
          `/admin/reports/courses/${courseId}/learners`,
          { params: { page, pageSize } },
        )
        .then((r) => r.data),
    enabled: courseId > 0,
  })
}

// GET /api/admin/reports/users/{userId}/progress
export function useUserProgress(userId: string) {
  return useQuery({
    queryKey: reportKeys.userProgress(userId),
    queryFn: () =>
      apiClient
        .get<UserProgressResponse>(`/admin/reports/users/${userId}/progress`)
        .then((r) => r.data),
    enabled: !!userId,
  })
}
