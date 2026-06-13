import { Link, useNavigate } from 'react-router-dom'

import LearningPathForm from '@/features/learning-paths/LearningPathForm'
import { useCreateLearningPath } from '@/features/learning-paths/api'
import type { LearningPathFormData } from '@/features/learning-paths/types'

export default function CreateLearningPathPage() {
  const navigate = useNavigate()
  const createMutation = useCreateLearningPath()

  const handleSubmit = async (data: LearningPathFormData) => {
    await createMutation.mutateAsync(data)
    navigate('/admin/learning-paths', {
      state: {
        toast: {
          type: 'success',
          text: `"${data.title}" created.`,
        },
      },
    })
  }

  return (
    <div className="mx-auto max-w-4xl px-4 py-8 sm:px-6">
      <div className="mb-6">
        <Link
          to="/admin/learning-paths"
          className="text-sm text-muted-foreground hover:text-foreground"
        >
          Back to learning paths
        </Link>
        <h1 className="mt-2 text-2xl font-bold">New Learning Path</h1>
      </div>

      <LearningPathForm
        onSubmit={handleSubmit}
        submitLabel="Create Learning Path"
      />
    </div>
  )
}
