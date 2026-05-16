import { z } from 'zod'

// --- Course status ---
export type CourseStatus = 'Draft' | 'Published' | 'Archived'

// --- Author ---
export interface AuthorSummary {
  id: number
  name: string
  bio: string | null
  avatarUrl: string | null
}

// --- Published course (public) ---
export interface CourseSummary {
  id: number
  title: string
  slug: string
  shortDescription: string | null
  thumbnailUrl: string | null
  category: string | null
  level: string | null
  language: string | null
  authorName: string
  createdAt: string
}

export interface PaginatedResponse<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
}

// --- Course detail (public, by slug) ---
export interface CourseDetail {
  id: number
  title: string
  slug: string
  shortDescription: string | null
  description: string | null
  thumbnailUrl: string | null
  category: string | null
  level: string | null
  language: string | null
  authorId: number
  authorName: string
  authorBio: string | null
  authorAvatarUrl: string | null
  status: CourseStatus
  createdAt: string
  updatedAt: string
}

// --- Admin course list ---
export interface AdminCourseSummary {
  id: number
  title: string
  slug: string
  category: string | null
  level: string | null
  authorName: string
  status: CourseStatus
  createdAt: string
  updatedAt: string
}

// --- Admin course detail ---
export interface AdminCourseDetail {
  id: number
  title: string
  slug: string
  shortDescription: string | null
  description: string | null
  thumbnailUrl: string | null
  category: string | null
  level: string | null
  language: string | null
  authorId: number
  authorName: string
  status: CourseStatus
  createdAt: string
  updatedAt: string
}

// --- Form schema ---
export const courseFormSchema = z.object({
  title: z.string().min(1, 'Title is required').max(200),
  slug: z
    .string()
    .min(1, 'Slug is required')
    .max(200)
    .regex(
      /^[a-z0-9]+(?:-[a-z0-9]+)*$/,
      'Slug must be lowercase kebab-case (e.g. my-course-title)',
    ),
  shortDescription: z.string().max(500).optional(),
  description: z.string().optional(),
  thumbnailUrl: z.string().max(2048).optional(),
  category: z.string().optional(),
  level: z.string().optional(),
  language: z.string().optional(),
  authorId: z.number({ error: 'Author is required' }).min(1, 'Author is required'),
})

export type CourseFormInput = z.infer<typeof courseFormSchema>

// --- Slug utility ---
export function slugify(text: string): string {
  return text
    .toLowerCase()
    .trim()
    .replace(/[^\w\s-]/g, '')
    .replace(/[\s_]+/g, '-')
    .replace(/^-+|-+$/g, '')
}
