import { Link, useNavigate, useParams } from 'react-router-dom'

import LearningPathForm, {
  type LearningPathCourseLabel,
} from '@/features/learning-paths/LearningPathForm'
import {
  useAdminLearningPath,
  useUpdateLearningPath,
} from '@/features/learning-paths/api'
import type {
  LearningPathFormData,
  LearningPathFormInput,
} from '@/features/learning-paths/types'

export default function EditLearningPathPage() {
  const { id } = useParams<{ id: string }>()
  const learningPathId = Number(id)
  const isValidId =
    Number.isInteger(learningPathId) && learningPathId > 0
  const navigate = useNavigate()

  const {
    data: learningPath,
    isLoading,
    isError,
  } = useAdminLearningPath(isValidId ? learningPathId : 0)
  const updateMutation = useUpdateLearningPath(
    isValidId ? learningPathId : 0,
  )

  const handleSubmit = async (data: LearningPathFormData) => {
    await updateMutation.mutateAsync(data)
    navigate('/admin/learning-paths', {
      state: {
        toast: {
          type: 'success',
          text: `"${data.title}" updated.`,
        },
      },
    })
  }

  if (!isValidId) {
    return (
      <div className="mx-auto max-w-4xl px-4 py-8 sm:px-6">
        <p className="text-muted-foreground">Invalid learning path ID.</p>
        <Link
          to="/admin/learning-paths"
          className="mt-2 inline-block text-sm text-primary hover:underline"
        >
          Back to learning paths
        </Link>
      </div>
    )
  }

  if (isLoading) {
    return (
      <div
        className="mx-auto max-w-4xl animate-pulse space-y-4 px-4 py-8 sm:px-6"
        aria-label="Loading learning path"
      >
        <div className="h-4 w-36 rounded bg-muted" />
        <div className="h-8 w-64 rounded bg-muted" />
        {Array.from({ length: 6 }).map((_, index) => (
          <div key={index} className="h-12 rounded bg-muted" />
        ))}
      </div>
    )
  }

  if (isError || !learningPath) {
    return (
      <div className="mx-auto max-w-4xl px-4 py-8 sm:px-6">
        <p className="rounded-md bg-destructive/10 px-4 py-3 text-sm text-destructive">
          Learning path could not be loaded.
        </p>
        <Link
          to="/admin/learning-paths"
          className="mt-3 inline-block text-sm text-primary hover:underline"
        >
          Back to learning paths
        </Link>
      </div>
    )
  }

  if (learningPath.status === 'Archived') {
    return (
      <div className="mx-auto max-w-4xl px-4 py-8 sm:px-6">
        <Link
          to="/admin/learning-paths"
          className="text-sm text-muted-foreground hover:text-foreground"
        >
          Back to learning paths
        </Link>
        <div className="mt-6 rounded-lg border border-dashed p-8">
          <h1 className="text-xl font-semibold">{learningPath.title}</h1>
          <p className="mt-2 text-sm text-muted-foreground">
            Archived learning paths cannot be edited.
          </p>
        </div>
      </div>
    )
  }

  const sortedSections = [...learningPath.sections].sort(
    (left, right) => left.orderIndex - right.orderIndex,
  )
  const defaultValues: Partial<LearningPathFormInput> = {
    title: learningPath.title,
    slug: learningPath.slug,
    shortDescription: learningPath.shortDescription ?? '',
    description: learningPath.description ?? '',
    thumbnailUrl: learningPath.thumbnailUrl ?? '',
    estimatedDurationLabel: learningPath.estimatedDurationLabel ?? '',
    sections: sortedSections.map((section, sectionIndex) => ({
      title: section.title,
      description: section.description ?? '',
      orderIndex: sectionIndex,
      courses: [...section.courses]
        .sort((left, right) => left.orderIndex - right.orderIndex)
        .map((course, courseIndex) => ({
          courseId: course.courseId,
          orderIndex: courseIndex,
        })),
    })),
  }
  const courseLabels = learningPath.sections
    .flatMap((section) => section.courses)
    .reduce<Record<number, LearningPathCourseLabel>>((labels, course) => {
      labels[course.courseId] = {
        title: course.title,
        status: course.status,
      }
      return labels
    }, {})

  return (
    <div className="mx-auto max-w-4xl px-4 py-8 sm:px-6">
      <div className="mb-6">
        <Link
          to="/admin/learning-paths"
          className="text-sm text-muted-foreground hover:text-foreground"
        >
          Back to learning paths
        </Link>
        <h1 className="mt-2 text-2xl font-bold">Edit Learning Path</h1>
        <p className="text-sm text-muted-foreground">{learningPath.title}</p>
      </div>

      <LearningPathForm
        defaultValues={defaultValues}
        courseLabels={courseLabels}
        onSubmit={handleSubmit}
        submitLabel="Save Changes"
        isEditMode
      />
    </div>
  )
}
