import { z } from 'zod'
import type { CourseStatus } from '@/features/courses/types'

export const learningPathStatusSchema = z.enum([
  'Draft',
  'Published',
  'Archived',
])

export type LearningPathStatus = z.infer<typeof learningPathStatusSchema>

const courseStatusSchema: z.ZodType<CourseStatus> = z.enum([
  'Draft',
  'Published',
  'Archived',
])

const nullableStringSchema = z.string().nullable()

// --- Public DTOs ---

export interface LearningPathSummary {
  id: number
  title: string
  slug: string
  shortDescription: string | null
  thumbnailUrl: string | null
  estimatedDurationLabel: string | null
  courseCount: number
}

export const learningPathSummarySchema: z.ZodType<LearningPathSummary> = z.object({
  id: z.number().int().positive(),
  title: z.string(),
  slug: z.string(),
  shortDescription: nullableStringSchema,
  thumbnailUrl: nullableStringSchema,
  estimatedDurationLabel: nullableStringSchema,
  courseCount: z.number().int().nonnegative(),
})

export interface LearningPathsResponse {
  items: LearningPathSummary[]
  totalCount: number
  page: number
  pageSize: number
}

export const learningPathsResponseSchema: z.ZodType<LearningPathsResponse> =
  z.object({
    items: z.array(learningPathSummarySchema),
    totalCount: z.number().int().nonnegative(),
    page: z.number().int().positive(),
    pageSize: z.number().int().positive(),
  })

export interface LearningPathProgress {
  completedCoursesCount: number
  totalCoursesCount: number
  progressPercent: number
}

export const learningPathProgressSchema: z.ZodType<LearningPathProgress> =
  z.object({
    completedCoursesCount: z.number().int().nonnegative(),
    totalCoursesCount: z.number().int().nonnegative(),
    progressPercent: z.number().int().min(0).max(100),
  })

export interface LearningPathCourseDetail {
  id: number
  title: string
  slug: string
  shortDescription: string | null
  thumbnailUrl: string | null
  level: string | null
  lessonCount: number
  isEnrolled?: boolean
  progressPercent?: number
  isCompleted?: boolean
}

export const learningPathCourseDetailSchema: z.ZodType<LearningPathCourseDetail> =
  z.object({
    id: z.number().int().positive(),
    title: z.string(),
    slug: z.string(),
    shortDescription: nullableStringSchema,
    thumbnailUrl: nullableStringSchema,
    level: nullableStringSchema,
    lessonCount: z.number().int().nonnegative(),
    isEnrolled: z.boolean().optional(),
    progressPercent: z.number().int().min(0).max(100).optional(),
    isCompleted: z.boolean().optional(),
  })

export interface LearningPathSectionDetail {
  id: number
  title: string
  description: string | null
  courses: LearningPathCourseDetail[]
}

export const learningPathSectionDetailSchema: z.ZodType<LearningPathSectionDetail> =
  z.object({
    id: z.number().int().positive(),
    title: z.string(),
    description: nullableStringSchema,
    courses: z.array(learningPathCourseDetailSchema),
  })

export interface LearningPathDetail {
  id: number
  title: string
  slug: string
  shortDescription: string | null
  description: string | null
  thumbnailUrl: string | null
  estimatedDurationLabel: string | null
  sections: LearningPathSectionDetail[]
  progress?: LearningPathProgress
}

export const learningPathDetailSchema: z.ZodType<LearningPathDetail> = z.object({
  id: z.number().int().positive(),
  title: z.string(),
  slug: z.string(),
  shortDescription: nullableStringSchema,
  description: nullableStringSchema,
  thumbnailUrl: nullableStringSchema,
  estimatedDurationLabel: nullableStringSchema,
  sections: z.array(learningPathSectionDetailSchema),
  progress: learningPathProgressSchema.optional(),
})

// --- Admin DTOs ---

export interface AdminLearningPathSummary {
  id: number
  title: string
  slug: string
  status: LearningPathStatus
  sectionCount: number
  courseCount: number
  createdAt: string
  updatedAt: string
}

export const adminLearningPathSummarySchema: z.ZodType<AdminLearningPathSummary> =
  z.object({
    id: z.number().int().positive(),
    title: z.string(),
    slug: z.string(),
    status: learningPathStatusSchema,
    sectionCount: z.number().int().nonnegative(),
    courseCount: z.number().int().nonnegative(),
    createdAt: z.string(),
    updatedAt: z.string(),
  })

export interface AdminLearningPathsResponse {
  items: AdminLearningPathSummary[]
  totalCount: number
  page: number
  pageSize: number
}

export const adminLearningPathsResponseSchema: z.ZodType<AdminLearningPathsResponse> =
  z.object({
    items: z.array(adminLearningPathSummarySchema),
    totalCount: z.number().int().nonnegative(),
    page: z.number().int().positive(),
    pageSize: z.number().int().positive(),
  })

export interface AdminLearningPathCourseDetail {
  courseId: number
  title: string
  slug: string
  status: CourseStatus
  orderIndex: number
}

export const adminLearningPathCourseDetailSchema: z.ZodType<AdminLearningPathCourseDetail> =
  z.object({
    courseId: z.number().int().positive(),
    title: z.string(),
    slug: z.string(),
    status: courseStatusSchema,
    orderIndex: z.number().int().nonnegative(),
  })

export interface AdminLearningPathSectionDetail {
  id: number
  title: string
  description: string | null
  orderIndex: number
  courses: AdminLearningPathCourseDetail[]
}

export const adminLearningPathSectionDetailSchema: z.ZodType<AdminLearningPathSectionDetail> =
  z.object({
    id: z.number().int().positive(),
    title: z.string(),
    description: nullableStringSchema,
    orderIndex: z.number().int().nonnegative(),
    courses: z.array(adminLearningPathCourseDetailSchema),
  })

export interface AdminLearningPathDetail {
  id: number
  title: string
  slug: string
  shortDescription: string | null
  description: string | null
  thumbnailUrl: string | null
  estimatedDurationLabel: string | null
  status: LearningPathStatus
  createdAt: string
  updatedAt: string
  sections: AdminLearningPathSectionDetail[]
}

export const adminLearningPathDetailSchema: z.ZodType<AdminLearningPathDetail> =
  z.object({
    id: z.number().int().positive(),
    title: z.string(),
    slug: z.string(),
    shortDescription: nullableStringSchema,
    description: nullableStringSchema,
    thumbnailUrl: nullableStringSchema,
    estimatedDurationLabel: nullableStringSchema,
    status: learningPathStatusSchema,
    createdAt: z.string(),
    updatedAt: z.string(),
    sections: z.array(adminLearningPathSectionDetailSchema),
  })

export interface LearningPathMutationResponse {
  id: number
  title: string
  slug: string
  status: LearningPathStatus
}

export const learningPathMutationResponseSchema: z.ZodType<LearningPathMutationResponse> =
  z.object({
    id: z.number().int().positive(),
    title: z.string(),
    slug: z.string(),
    status: learningPathStatusSchema,
  })

export interface LearningPathStatusResponse {
  id: number
  status: LearningPathStatus
}

export const learningPathStatusResponseSchema: z.ZodType<LearningPathStatusResponse> =
  z.object({
    id: z.number().int().positive(),
    status: learningPathStatusSchema,
  })

// --- Create/edit form ---

const optionalTextSchema = (maxLength?: number) => {
  const schema = maxLength ? z.string().max(maxLength) : z.string()
  return schema
    .transform((value) => value.trim())
    .transform((value) => (value.length > 0 ? value : undefined))
    .optional()
}

const optionalHttpUrlSchema = z
  .string()
  .max(2048)
  .refine((value) => {
    if (value.trim().length === 0) return true

    try {
      const url = new URL(value)
      return url.protocol === 'http:' || url.protocol === 'https:'
    } catch {
      return false
    }
  }, 'Thumbnail URL must be a valid HTTP/HTTPS URL.')
  .transform((value) => value.trim())
  .transform((value) => (value.length > 0 ? value : undefined))
  .optional()

export const learningPathCourseInputSchema = z.object({
  courseId: z.number().int().positive('Select a course'),
  orderIndex: z.number().int().nonnegative(),
})

export const learningPathSectionInputSchema = z.object({
  title: z.string().trim().min(1, 'Section title is required').max(200),
  description: optionalTextSchema(),
  orderIndex: z.number().int().nonnegative(),
  courses: z
    .array(learningPathCourseInputSchema)
    .min(1, 'Each section must contain at least one course'),
})

export const learningPathFormSchema = z
  .object({
    title: z.string().trim().min(1, 'Title is required').max(200),
    slug: z
      .string()
      .trim()
      .min(1, 'Slug is required')
      .max(200)
      .regex(
        /^[a-z0-9]+(?:-[a-z0-9]+)*$/,
        'Slug must be lowercase kebab-case (e.g. foundations-of-faith)',
      ),
    shortDescription: optionalTextSchema(500),
    description: optionalTextSchema(),
    thumbnailUrl: optionalHttpUrlSchema,
    estimatedDurationLabel: optionalTextSchema(),
    sections: z
      .array(learningPathSectionInputSchema)
      .min(1, 'A learning path must contain at least one section'),
  })
  .superRefine((data, ctx) => {
    const sectionOrderIndices = data.sections.map((section) => section.orderIndex)
    if (new Set(sectionOrderIndices).size !== sectionOrderIndices.length) {
      ctx.addIssue({
        code: 'custom',
        path: ['sections'],
        message: 'Section order indices must be unique',
      })
    }

    const courseIds = data.sections.flatMap((section) =>
      section.courses.map((course) => course.courseId),
    )
    if (new Set(courseIds).size !== courseIds.length) {
      ctx.addIssue({
        code: 'custom',
        path: ['sections'],
        message: 'A course cannot appear more than once in a learning path',
      })
    }

    data.sections.forEach((section, sectionIndex) => {
      const courseOrderIndices = section.courses.map(
        (course) => course.orderIndex,
      )
      if (new Set(courseOrderIndices).size !== courseOrderIndices.length) {
        ctx.addIssue({
          code: 'custom',
          path: ['sections', sectionIndex, 'courses'],
          message: 'Course order indices must be unique within a section',
        })
      }
    })
  })

export type LearningPathFormInput = z.input<typeof learningPathFormSchema>
export type LearningPathFormData = z.output<typeof learningPathFormSchema>
