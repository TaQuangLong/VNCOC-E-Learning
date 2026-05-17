import { Link, useNavigate, useParams } from 'react-router-dom'
import { useCreateLesson } from '@/features/lessons/api'
import LessonForm from '@/features/lessons/LessonForm'
import type { LessonFormInput } from '@/features/lessons/types'

export default function CreateLessonPage() {
  const { courseId: courseIdParam } = useParams<{ courseId: string }>()
  const courseId = Number(courseIdParam)
  const navigate = useNavigate()
  const mutation = useCreateLesson(courseId)

  const handleSubmit = async (data: LessonFormInput) => {
    await mutation.mutateAsync(data)
    navigate(`/admin/courses/${courseId}/lessons`)
  }

  return (
    <div className="mx-auto max-w-2xl px-4 py-8 sm:px-6">
      <div className="mb-6">
        <Link
          to={`/admin/courses/${courseId}/lessons`}
          className="text-sm text-muted-foreground hover:text-foreground"
        >
          ← Back to lessons
        </Link>
        <h1 className="mt-2 text-2xl font-bold">New Lesson</h1>
      </div>

      <LessonForm onSubmit={handleSubmit} submitLabel="Create Lesson" />
    </div>
  )
}
