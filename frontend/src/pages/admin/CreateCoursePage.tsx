import { Link, useNavigate } from 'react-router-dom'
import { useCreateCourse } from '@/features/courses/api'
import CourseForm from '@/features/courses/CourseForm'
import type { CourseFormInput } from '@/features/courses/types'

export default function CreateCoursePage() {
  const navigate = useNavigate()
  const mutation = useCreateCourse()

  const handleSubmit = async (data: CourseFormInput) => {
    await mutation.mutateAsync(data)
    navigate('/admin/courses')
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
        <h1 className="mt-2 text-2xl font-bold">New Course</h1>
      </div>

      <CourseForm onSubmit={handleSubmit} submitLabel="Create Course" />
    </div>
  )
}
