import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type { EnrollCourseResponse, EnrollmentStatus, MyEnrolledCourse } from './types'

export const enrollmentKeys = {
  myEnrolledCourses: () => ['enrollment', 'my-courses'] as const,
  status: (courseId: number) => ['enrollment', 'status', courseId] as const,
}

export function useMyEnrolledCourses() {
  return useQuery({
    queryKey: enrollmentKeys.myEnrolledCourses(),
    queryFn: () =>
      apiClient.get<MyEnrolledCourse[]>('/me/courses').then((r) => r.data),
  })
}

export function useEnrollmentStatus(courseId: number | undefined, enabled = true) {
  return useQuery({
    queryKey: enrollmentKeys.status(courseId ?? 0),
    queryFn: () =>
      apiClient
        .get<EnrollmentStatus>(`/courses/${courseId}/enrollment-status`)
        .then((r) => r.data),
    enabled: !!courseId && enabled,
  })
}

export function useEnrollCourse() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (courseId: number) =>
      apiClient
        .post<EnrollCourseResponse>(`/courses/${courseId}/enroll`)
        .then((r) => r.data),
    onSuccess: (_, courseId) => {
      queryClient.invalidateQueries({ queryKey: enrollmentKeys.myEnrolledCourses() })
      queryClient.invalidateQueries({ queryKey: enrollmentKeys.status(courseId) })
    },
  })
}
