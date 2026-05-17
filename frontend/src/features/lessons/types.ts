import { z } from 'zod'

// --- Content type (matches backend enum) ---
export type ContentType = 'Video' | 'Text' | 'Pdf'

// --- Lesson list item (public, GET /api/courses/{courseId}/lessons) ---
export interface LessonSummary {
  id: number
  title: string
  description: string | null
  contentType: ContentType
  orderIndex: number
  durationSeconds: number
  isPreview: boolean
}

// --- Resource ---
export interface Resource {
  id: number
  title: string
  url: string
  createdAt?: string
}

// --- Lesson detail (GET /api/lessons/{id}) ---
export interface LessonDetail {
  id: number
  courseId: number
  title: string
  description: string | null
  contentType: ContentType
  youTubeUrl: string | null
  textContent: string | null
  pdfUrl: string | null
  durationSeconds: number
  orderIndex: number
  isPreview: boolean
  createdAt: string
  updatedAt: string
  resources: Resource[]
}

// --- Admin lesson form ---
const YOUTUBE_REGEX =
  /^https?:\/\/(www\.)?(youtube\.com\/watch\?.*v=|youtu\.be\/)[a-zA-Z0-9_-]+/

export const lessonFormSchema = z
  .object({
    title: z.string().min(1, 'Title is required').max(300),
    description: z.string().max(1000).optional(),
    contentType: z.enum(['Video', 'Text', 'Pdf']),
    youTubeUrl: z.string().max(2048).optional(),
    textContent: z.string().max(50000).optional(),
    pdfUrl: z.string().max(2048).optional(),
    durationSeconds: z.number().min(0),
    orderIndex: z.number().min(0),
    isPreview: z.boolean(),
  })
  .superRefine((data, ctx) => {
    if (data.contentType === 'Video') {
      if (!data.youTubeUrl) {
        ctx.addIssue({
          code: z.ZodIssueCode.custom,
          path: ['youTubeUrl'],
          message: 'YouTube URL is required for Video lessons.',
        })
      } else if (!YOUTUBE_REGEX.test(data.youTubeUrl)) {
        ctx.addIssue({
          code: z.ZodIssueCode.custom,
          path: ['youTubeUrl'],
          message: 'Must be a valid YouTube URL.',
        })
      }
    }
    if (data.contentType === 'Text' && !data.textContent) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        path: ['textContent'],
        message: 'Text content is required for Text lessons.',
      })
    }
    if (data.contentType === 'Pdf') {
      if (!data.pdfUrl) {
        ctx.addIssue({
          code: z.ZodIssueCode.custom,
          path: ['pdfUrl'],
          message: 'PDF URL is required for PDF lessons.',
        })
      } else {
        try {
          const parsed = new URL(data.pdfUrl)
          if (parsed.protocol !== 'http:' && parsed.protocol !== 'https:') {
            throw new Error()
          }
        } catch {
          ctx.addIssue({
            code: z.ZodIssueCode.custom,
            path: ['pdfUrl'],
            message: 'Must be a valid HTTP/HTTPS URL.',
          })
        }
      }
    }
  })

export type LessonFormInput = z.infer<typeof lessonFormSchema>

// --- Resource form ---
export const resourceFormSchema = z.object({
  title: z.string().min(1, 'Title is required').max(300),
  url: z
    .string()
    .min(1, 'URL is required')
    .max(2048)
    .refine((val) => {
      try {
        const parsed = new URL(val)
        return parsed.protocol === 'http:' || parsed.protocol === 'https:'
      } catch {
        return false
      }
    }, 'Must be a valid HTTP/HTTPS URL.'),
})

export type ResourceFormInput = z.infer<typeof resourceFormSchema>

// --- Content type label helper ---
export const CONTENT_TYPE_LABELS: Record<ContentType, string> = {
  Video: 'Video (YouTube)',
  Text: 'Text / Markdown',
  Pdf: 'PDF Link',
}
