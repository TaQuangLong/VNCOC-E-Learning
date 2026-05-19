import { z } from 'zod'

export interface DiscussionSummary {
  id: number
  userId: string
  authorName: string
  content: string
  replyCount: number
  createdAt: string
  updatedAt: string
  isDeleted: boolean
}

export interface GetLessonDiscussionsResponse {
  items: DiscussionSummary[]
  totalCount: number
  page: number
  pageSize: number
}

export interface ReplyDto {
  id: number
  userId: string
  authorName: string
  content: string
  createdAt: string
  updatedAt: string
  isDeleted: boolean
}

export interface GetDiscussionRepliesResponse {
  items: ReplyDto[]
  totalCount: number
  page: number
  pageSize: number
}

export interface CreateDiscussionResponse {
  id: number
  content: string
  createdAt: string
}

export interface CreateReplyResponse {
  id: number
  content: string
  createdAt: string
}

export interface UpdateDiscussionResponse {
  id: number
  content: string
  updatedAt: string
}

export const discussionFormSchema = z.object({
  content: z
    .string()
    .min(1, 'Content is required')
    .max(2000, 'Max 2,000 characters'),
})

export type DiscussionFormInput = z.infer<typeof discussionFormSchema>
