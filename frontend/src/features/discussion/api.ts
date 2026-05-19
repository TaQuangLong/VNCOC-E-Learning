import { useMutation, useQuery, useQueryClient, useInfiniteQuery } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type {
  GetLessonDiscussionsResponse,
  GetDiscussionRepliesResponse,
  CreateDiscussionResponse,
  CreateReplyResponse,
  UpdateDiscussionResponse,
} from './types'

const PAGE_SIZE = 10

export const discussionKeys = {
  lessonDiscussions: (lessonId: number) =>
    ['discussions', 'lesson', lessonId] as const,
  replies: (discussionId: number) =>
    ['discussions', 'replies', discussionId] as const,
}

// GET /api/lessons/{lessonId}/discussions?page=1&pageSize=10
export function useLessonDiscussions(lessonId: number) {
  return useInfiniteQuery({
    queryKey: discussionKeys.lessonDiscussions(lessonId),
    queryFn: ({ pageParam }: { pageParam: number }) =>
      apiClient
        .get<GetLessonDiscussionsResponse>(`/lessons/${lessonId}/discussions`, {
          params: { page: pageParam, pageSize: PAGE_SIZE },
        })
        .then((r) => r.data),
    initialPageParam: 1,
    getNextPageParam: (lastPage: GetLessonDiscussionsResponse) => {
      const loaded =
        (lastPage.page - 1) * lastPage.pageSize + lastPage.items.length
      return loaded < lastPage.totalCount ? lastPage.page + 1 : undefined
    },
    enabled: lessonId > 0,
  })
}

// GET /api/discussions/{discussionId}/replies?page=1&pageSize=50
export function useDiscussionReplies(discussionId: number, enabled: boolean) {
  return useQuery({
    queryKey: discussionKeys.replies(discussionId),
    queryFn: () =>
      apiClient
        .get<GetDiscussionRepliesResponse>(
          `/discussions/${discussionId}/replies`,
          { params: { page: 1, pageSize: 50 } },
        )
        .then((r) => r.data),
    enabled: enabled && discussionId > 0,
  })
}

// POST /api/lessons/{lessonId}/discussions
export function useCreateDiscussion(lessonId: number) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (content: string) =>
      apiClient
        .post<CreateDiscussionResponse>(`/lessons/${lessonId}/discussions`, {
          content,
        })
        .then((r) => r.data),
    onSuccess: () => {
      qc.invalidateQueries({
        queryKey: discussionKeys.lessonDiscussions(lessonId),
      })
    },
  })
}

// POST /api/discussions/{discussionId}/reply
export function useCreateReply(lessonId: number, discussionId: number) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (content: string) =>
      apiClient
        .post<CreateReplyResponse>(`/discussions/${discussionId}/reply`, {
          content,
        })
        .then((r) => r.data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: discussionKeys.replies(discussionId) })
      qc.invalidateQueries({
        queryKey: discussionKeys.lessonDiscussions(lessonId),
      })
    },
  })
}

// PUT /api/discussions/{discussionId}
export function useUpdateDiscussion(lessonId: number, parentDiscussionId?: number) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({
      discussionId,
      content,
    }: {
      discussionId: number
      content: string
    }) =>
      apiClient
        .put<UpdateDiscussionResponse>(`/discussions/${discussionId}`, {
          content,
        })
        .then((r) => r.data),
    onSuccess: () => {
      qc.invalidateQueries({
        queryKey: discussionKeys.lessonDiscussions(lessonId),
      })
      if (parentDiscussionId !== undefined) {
        qc.invalidateQueries({
          queryKey: discussionKeys.replies(parentDiscussionId),
        })
      }
    },
  })
}

// DELETE /api/discussions/{discussionId}
export function useDeleteDiscussion(lessonId: number, parentDiscussionId?: number) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (discussionId: number) =>
      apiClient.delete(`/discussions/${discussionId}`).then((r) => r.data),
    onSuccess: () => {
      qc.invalidateQueries({
        queryKey: discussionKeys.lessonDiscussions(lessonId),
      })
      if (parentDiscussionId !== undefined) {
        qc.invalidateQueries({
          queryKey: discussionKeys.replies(parentDiscussionId),
        })
      }
    },
  })
}

// DELETE /api/admin/discussions/{discussionId}
export function useAdminDeleteDiscussion(
  lessonId: number,
  parentDiscussionId?: number,
) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (discussionId: number) =>
      apiClient
        .delete(`/admin/discussions/${discussionId}`)
        .then((r) => r.data),
    onSuccess: () => {
      qc.invalidateQueries({
        queryKey: discussionKeys.lessonDiscussions(lessonId),
      })
      if (parentDiscussionId !== undefined) {
        qc.invalidateQueries({
          queryKey: discussionKeys.replies(parentDiscussionId),
        })
      }
    },
  })
}
