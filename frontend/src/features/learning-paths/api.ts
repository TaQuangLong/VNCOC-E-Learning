import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useAuth } from '@/hooks/useAuth'
import { apiClient } from '@/lib/api-client'
import {
  adminLearningPathDetailSchema,
  adminLearningPathsResponseSchema,
  learningPathDetailSchema,
  learningPathMutationResponseSchema,
  learningPathStatusResponseSchema,
  learningPathsResponseSchema,
} from './types'
import type {
  LearningPathFormData,
  LearningPathStatus,
} from './types'

export interface LearningPathsParams {
  page?: number
  pageSize?: number
}

export interface AdminLearningPathsParams extends LearningPathsParams {
  status?: LearningPathStatus
}

export const learningPathKeys = {
  all: ['learning-paths'] as const,
  publicList: (params: LearningPathsParams) =>
    ['learning-paths', 'public', 'list', params] as const,
  publicDetail: (slug: string, userId: string | null) =>
    ['learning-paths', 'public', 'detail', slug, userId] as const,
  adminList: (params: AdminLearningPathsParams) =>
    ['learning-paths', 'admin', 'list', params] as const,
  adminDetail: (id: number) =>
    ['learning-paths', 'admin', 'detail', id] as const,
}

// --- Public ---

export function useLearningPaths(params: LearningPathsParams = {}) {
  return useQuery({
    queryKey: learningPathKeys.publicList(params),
    queryFn: () =>
      apiClient
        .get('/learning-paths', { params })
        .then((response) => learningPathsResponseSchema.parse(response.data)),
  })
}

export function useLearningPathBySlug(slug: string) {
  const { user, isLoading: isAuthLoading } = useAuth()

  return useQuery({
    queryKey: learningPathKeys.publicDetail(slug, user?.userId ?? null),
    queryFn: () =>
      apiClient
        .get(`/learning-paths/${slug}`)
        .then((response) => learningPathDetailSchema.parse(response.data)),
    enabled: slug.length > 0 && !isAuthLoading,
  })
}

// --- Admin ---

export function useAdminLearningPaths(
  params: AdminLearningPathsParams = {},
) {
  return useQuery({
    queryKey: learningPathKeys.adminList(params),
    queryFn: () =>
      apiClient
        .get('/admin/learning-paths', { params })
        .then((response) =>
          adminLearningPathsResponseSchema.parse(response.data),
        ),
  })
}

export function useAdminLearningPath(id: number) {
  return useQuery({
    queryKey: learningPathKeys.adminDetail(id),
    queryFn: () =>
      apiClient
        .get(`/admin/learning-paths/${id}`)
        .then((response) => adminLearningPathDetailSchema.parse(response.data)),
    enabled: id > 0,
  })
}

// --- Mutations ---

export function useCreateLearningPath() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: LearningPathFormData) =>
      apiClient
        .post('/admin/learning-paths', data)
        .then((response) =>
          learningPathMutationResponseSchema.parse(response.data),
        ),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: learningPathKeys.all })
    },
  })
}

export function useUpdateLearningPath(id: number) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: LearningPathFormData) =>
      apiClient
        .put(`/admin/learning-paths/${id}`, data)
        .then((response) =>
          learningPathMutationResponseSchema.parse(response.data),
        ),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: learningPathKeys.all })
    },
  })
}

export function usePublishLearningPath() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: number) =>
      apiClient
        .post(`/admin/learning-paths/${id}/publish`)
        .then((response) =>
          learningPathStatusResponseSchema.parse(response.data),
        ),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: learningPathKeys.all })
    },
  })
}

export function useUnpublishLearningPath() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: number) =>
      apiClient
        .post(`/admin/learning-paths/${id}/unpublish`)
        .then((response) =>
          learningPathStatusResponseSchema.parse(response.data),
        ),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: learningPathKeys.all })
    },
  })
}

export function useArchiveLearningPath() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: number) =>
      apiClient
        .delete(`/admin/learning-paths/${id}`)
        .then(() => undefined),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: learningPathKeys.all })
    },
  })
}
