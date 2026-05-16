import { Link, useNavigate, useParams } from 'react-router-dom'
import { useAdminCourse, useUpdateCourse } from '@/features/courses/api'
import CourseForm from '@/features/courses/CourseForm'
import type { CourseFormInput } from '@/features/courses/types'

export default function EditCoursePage() {
  const { id } = useParams<{ id: string }>()
  const courseId = Number(id)
  const navigate = useNavigate()

  const isValidId = !Number.isNaN(courseId) && courseId > 0
  const { data: course, isLoading, isError } = useAdminCourse(isValidId ? courseId : 0)
  const mutation = useUpdateCourse(isValidId ? courseId : 0)

  const handleSubmit = async (data: CourseFormInput) => {
    await mutation.mutateAsync(data)
    navigate('/admin/courses')
  }

  if (!isValidId) {
    return (
      <div className="mx-auto max-w-2xl px-4 py-8 sm:px-6">
        <p className="text-muted-foreground">Invalid course ID.</p>
        <Link to="/admin/courses" className="mt-2 block text-sm text-primary hover:underline">
          Back to courses
        </Link>
      </div>
    )
  }

  if (isLoading) {
    return (
      <div className="mx-auto max-w-2xl animate-pulse space-y-3 px-4 py-8 sm:px-6">
        <div className="h-4 w-32 rounded bg-muted" />
        <div className="h-8 w-56 rounded bg-muted" />
        <div className="mt-6 space-y-4">
          {Array.from({ length: 6 }).map((_, i) => (
            <div key={i} className="h-10 rounded bg-muted" />
          ))}
        </div>
      </div>
    )
  }

  if (isError || !course) {
    return (
      <div className="mx-auto max-w-2xl px-4 py-8 sm:px-6">
        <p className="text-muted-foreground">Course not found.</p>
        <Link
          to="/admin/courses"
          className="mt-2 block text-sm text-primary hover:underline"
        >
          Back to courses
        </Link>
      </div>
    )
  }

  const defaultValues: Partial<CourseFormInput> = {
    title: course.title,
    slug: course.slug,
    shortDescription: course.shortDescription ?? '',
    description: course.description ?? '',
    thumbnailUrl: course.thumbnailUrl ?? '',
    category: course.category ?? '',
    level: course.level ?? '',
    language: course.language ?? '',
    authorId: course.authorId,
  }

  return (
    <div className="mx-auto max-w-2xl px-4 py-8 sm:px-6">
      <div className="mb-6">
        <Link
          to="/admin/courses"
          className="text-sm text-muted-foreground hover:text-foreground"
        >
          ← Back to courses
        </Link>
        <h1 className="mt-2 text-2xl font-bold">Edit Course</h1>
        <p className="text-sm text-muted-foreground">{course.title}</p>
      </div>

      <CourseForm
        defaultValues={defaultValues}
        onSubmit={handleSubmit}
        submitLabel="Save Changes"
        isEditMode
      />
    </div>
  )
}
