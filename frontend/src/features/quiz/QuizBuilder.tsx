import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Button } from '@/components/ui/button'
import {
  useLessonQuiz,
  useCreateQuiz,
  useUpdateQuiz,
  useAddQuestion,
  useUpdateQuestion,
  useDeleteQuestion,
} from './api'
import QuestionEditor from './QuestionEditor'
import {
  quizMetaFormSchema,
  type QuizMetaFormInput,
  type QuestionFormInput,
  type Question,
} from './types'

interface Props {
  lessonId: number
}

export default function QuizBuilder({ lessonId }: Props) {
  const { data: quiz, isLoading, isError } = useLessonQuiz(lessonId)

  if (isLoading) {
    return (
      <div className="space-y-2">
        {Array.from({ length: 3 }).map((_, i) => (
          <div key={i} className="h-8 animate-pulse rounded-md bg-muted" />
        ))}
      </div>
    )
  }

  // isError here means 404 (no quiz) or network error — handled inside the panel
  if (isError || !quiz) {
    return <NoQuizPanel lessonId={lessonId} />
  }

  return <QuizPanel lessonId={lessonId} quiz={quiz} />
}

// ─── No quiz yet ────────────────────────────────────────────────────────────────

function NoQuizPanel({ lessonId }: { lessonId: number }) {
  const [isCreating, setIsCreating] = useState(false)

  if (!isCreating) {
    return (
      <div className="rounded-md border border-dashed border-border px-4 py-8 text-center">
        <p className="mb-3 text-sm text-muted-foreground">No quiz for this lesson yet.</p>
        <Button variant="outline" size="sm" onClick={() => setIsCreating(true)}>
          + Create Quiz
        </Button>
      </div>
    )
  }

  return (
    <CreateQuizForm lessonId={lessonId} onCancel={() => setIsCreating(false)} />
  )
}

// ─── Create quiz form ────────────────────────────────────────────────────────────

function CreateQuizForm({
  lessonId,
  onCancel,
}: {
  lessonId: number
  onCancel: () => void
}) {
  const createQuiz = useCreateQuiz(lessonId)
  const [firstQuestion, setFirstQuestion] = useState<QuestionFormInput | null>(null)
  const [showQuestionForm, setShowQuestionForm] = useState(true)

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<QuizMetaFormInput>({
    resolver: zodResolver(quizMetaFormSchema),
    defaultValues: { title: '', description: '', passingScore: 70, isRequired: false },
  })

  const handleCreate = async (meta: QuizMetaFormInput) => {
    if (!firstQuestion) return
    await createQuiz.mutateAsync({
      title: meta.title,
      description: meta.description || null,
      passingScore: meta.passingScore,
      isRequired: meta.isRequired,
      questions: [
        {
          text: firstQuestion.text,
          type: firstQuestion.type,
          orderIndex: 0,
          options: firstQuestion.options.map((o, i) => ({
            text: o.text,
            isCorrect: o.isCorrect,
            orderIndex: i,
          })),
        },
      ],
    })
  }

  return (
    <div className="space-y-6 rounded-md border border-border p-4">
      <h3 className="text-base font-semibold">Create Quiz</h3>

      {/* Quiz meta */}
      <div className="space-y-3">
        <div className="space-y-1">
          <label className="text-sm font-medium">
            Title <span className="text-destructive">*</span>
          </label>
          <input
            {...register('title')}
            className="border-input bg-background w-full rounded-md border px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
            placeholder="e.g. Lesson 1 Quiz"
          />
          {errors.title && (
            <p className="text-xs text-destructive">{errors.title.message}</p>
          )}
        </div>

        <div className="space-y-1">
          <label className="text-sm font-medium">Description</label>
          <input
            {...register('description')}
            className="border-input bg-background w-full rounded-md border px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
            placeholder="Optional description…"
          />
        </div>

        <div className="grid grid-cols-2 gap-3">
          <div className="space-y-1">
            <label className="text-sm font-medium">
              Passing Score (%) <span className="text-destructive">*</span>
            </label>
            <input
              type="number"
              min={0}
              max={100}
              {...register('passingScore', { valueAsNumber: true })}
              className="border-input bg-background w-full rounded-md border px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
            />
            {errors.passingScore && (
              <p className="text-xs text-destructive">{errors.passingScore.message}</p>
            )}
          </div>

          <div className="flex items-end gap-2 pb-2">
            <input
              type="checkbox"
              id="create-is-required"
              {...register('isRequired')}
              className="h-4 w-4 accent-primary"
            />
            <label htmlFor="create-is-required" className="text-sm font-medium">
              Required to complete lesson
            </label>
          </div>
        </div>
      </div>

      {/* First question */}
      <div>
        <p className="mb-2 text-sm font-semibold">First Question</p>
        {firstQuestion ? (
          <div className="flex items-center justify-between rounded-md border border-border px-3 py-2">
            <span className="truncate text-sm">{firstQuestion.text}</span>
            <button
              type="button"
              onClick={() => {
                setFirstQuestion(null)
                setShowQuestionForm(true)
              }}
              className="text-xs text-muted-foreground hover:text-foreground hover:underline"
            >
              Edit
            </button>
          </div>
        ) : showQuestionForm ? (
          <QuestionEditor
            nextOrderIndex={0}
            onSave={async (data) => {
              setFirstQuestion(data)
              setShowQuestionForm(false)
            }}
            onCancel={onCancel}
            saveLabel="Add Question"
          />
        ) : null}
      </div>

      {/* Create button */}
      {firstQuestion && (
        <div className="flex items-center gap-2">
          <Button
            onClick={handleSubmit(handleCreate)}
            disabled={isSubmitting || createQuiz.isPending}
          >
            {createQuiz.isPending ? 'Creating…' : 'Create Quiz'}
          </Button>
          <Button type="button" variant="outline" onClick={onCancel}>
            Cancel
          </Button>
          {createQuiz.isError && (
            <p className="text-xs text-destructive">Failed to create quiz.</p>
          )}
        </div>
      )}
    </div>
  )
}

// ─── Existing quiz panel ─────────────────────────────────────────────────────────

function QuizPanel({
  lessonId,
  quiz,
}: {
  lessonId: number
  quiz: ReturnType<typeof useLessonQuiz>['data'] & object
}) {
  const [editingMeta, setEditingMeta] = useState(false)
  const [addingQuestion, setAddingQuestion] = useState(false)
  const [editingQuestionId, setEditingQuestionId] = useState<number | null>(null)

  const updateQuiz = useUpdateQuiz(lessonId)
  const addQuestion = useAddQuestion(lessonId)
  const updateQuestion = useUpdateQuestion(lessonId)
  const deleteQuestion = useDeleteQuestion(lessonId)

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
    reset,
  } = useForm<QuizMetaFormInput>({
    resolver: zodResolver(quizMetaFormSchema),
    defaultValues: {
      title: quiz!.title,
      description: quiz!.description ?? '',
      passingScore: quiz!.passingScore,
      isRequired: quiz!.isRequired,
    },
  })

  const handleUpdateMeta = async (data: QuizMetaFormInput) => {
    await updateQuiz.mutateAsync({
      quizId: quiz!.id,
      data: {
        title: data.title,
        description: data.description || null,
        passingScore: data.passingScore,
        isRequired: data.isRequired,
      },
    })
    setEditingMeta(false)
  }

  const handleAddQuestion = async (data: QuestionFormInput) => {
    await addQuestion.mutateAsync({
      quizId: quiz!.id,
      data: {
        text: data.text,
        type: data.type,
        orderIndex: data.orderIndex,
        options: data.options.map((o, i) => ({
          text: o.text,
          isCorrect: o.isCorrect,
          orderIndex: i,
        })),
      },
    })
    setAddingQuestion(false)
  }

  const handleUpdateQuestion = async (
    questionId: number,
    data: QuestionFormInput,
  ) => {
    await updateQuestion.mutateAsync({
      questionId,
      data: {
        text: data.text,
        type: data.type,
        orderIndex: data.orderIndex,
        options: data.options.map((o, i) => ({
          text: o.text,
          isCorrect: o.isCorrect,
          orderIndex: i,
        })),
      },
    })
    setEditingQuestionId(null)
  }

  const handleDeleteQuestion = async (questionId: number) => {
    if (!confirm('Delete this question?')) return
    try {
      await deleteQuestion.mutateAsync(questionId)
    } catch {
      // error shown via mutation state
    }
  }

  return (
    <div className="space-y-6">
      {/* Quiz meta */}
      <div className="rounded-md border border-border p-4">
        <div className="mb-3 flex items-center justify-between">
          <h3 className="text-base font-semibold">Quiz Settings</h3>
          {!editingMeta && (
            <Button
              variant="outline"
              size="sm"
              onClick={() => {
                reset({
                  title: quiz!.title,
                  description: quiz!.description ?? '',
                  passingScore: quiz!.passingScore,
                  isRequired: quiz!.isRequired,
                })
                setEditingMeta(true)
              }}
            >
              Edit
            </Button>
          )}
        </div>

        {editingMeta ? (
          <form onSubmit={handleSubmit(handleUpdateMeta)} noValidate className="space-y-3">
            <div className="space-y-1">
              <label className="text-sm font-medium">
                Title <span className="text-destructive">*</span>
              </label>
              <input
                {...register('title')}
                className="border-input bg-background w-full rounded-md border px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
              />
              {errors.title && (
                <p className="text-xs text-destructive">{errors.title.message}</p>
              )}
            </div>

            <div className="space-y-1">
              <label className="text-sm font-medium">Description</label>
              <input
                {...register('description')}
                className="border-input bg-background w-full rounded-md border px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
              />
            </div>

            <div className="grid grid-cols-2 gap-3">
              <div className="space-y-1">
                <label className="text-sm font-medium">Passing Score (%)</label>
                <input
                  type="number"
                  min={0}
                  max={100}
                  {...register('passingScore', { valueAsNumber: true })}
                  className="border-input bg-background w-full rounded-md border px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
                />
                {errors.passingScore && (
                  <p className="text-xs text-destructive">{errors.passingScore.message}</p>
                )}
              </div>

              <div className="flex items-end gap-2 pb-2">
                <input
                  type="checkbox"
                  id="edit-is-required"
                  {...register('isRequired')}
                  className="h-4 w-4 accent-primary"
                />
                <label htmlFor="edit-is-required" className="text-sm font-medium">
                  Required to complete lesson
                </label>
              </div>
            </div>

            <div className="flex items-center gap-2">
              <Button type="submit" size="sm" disabled={isSubmitting || updateQuiz.isPending}>
                {updateQuiz.isPending ? 'Saving…' : 'Save'}
              </Button>
              <Button
                type="button"
                variant="outline"
                size="sm"
                onClick={() => setEditingMeta(false)}
              >
                Cancel
              </Button>
              {updateQuiz.isError && (
                <p className="text-xs text-destructive">Failed to save.</p>
              )}
            </div>
          </form>
        ) : (
          <dl className="grid grid-cols-2 gap-2 text-sm sm:grid-cols-4">
            <div>
              <dt className="text-muted-foreground">Title</dt>
              <dd className="font-medium">{quiz!.title}</dd>
            </div>
            <div>
              <dt className="text-muted-foreground">Passing Score</dt>
              <dd className="font-medium">{quiz!.passingScore}%</dd>
            </div>
            <div>
              <dt className="text-muted-foreground">Required</dt>
              <dd className="font-medium">{quiz!.isRequired ? 'Yes' : 'No'}</dd>
            </div>
            <div>
              <dt className="text-muted-foreground">Questions</dt>
              <dd className="font-medium">{quiz!.questions.length}</dd>
            </div>
          </dl>
        )}
      </div>

      {/* Questions list */}
      <div>
        <div className="mb-3 flex items-center justify-between">
          <h3 className="text-base font-semibold">Questions</h3>
          {!addingQuestion && (
            <Button
              variant="outline"
              size="sm"
              onClick={() => setAddingQuestion(true)}
            >
              + Add Question
            </Button>
          )}
        </div>

        {quiz!.questions.length === 0 && !addingQuestion && (
          <p className="text-sm text-muted-foreground">No questions yet. Add one above.</p>
        )}

        <ul className="space-y-3">
          {quiz!.questions.map((q, index) => (
            <li key={q.id}>
              {editingQuestionId === q.id ? (
                <QuestionEditor
                  initialValues={buildInitialValues(q)}
                  onSave={(data) => handleUpdateQuestion(q.id, data)}
                  onCancel={() => setEditingQuestionId(null)}
                  saveLabel="Update Question"
                  isSaving={updateQuestion.isPending}
                  saveError={updateQuestion.isError}
                />
              ) : (
                <div className="rounded-md border border-border px-3 py-3">
                  <div className="flex items-start justify-between gap-2">
                    <div className="flex-1 space-y-1">
                      <p className="text-sm font-medium">
                        {index + 1}. {q.text}
                      </p>
                      <p className="text-xs text-muted-foreground">
                        {humanizeType(q.type)} · {q.options.length} options
                      </p>
                    </div>
                    <div className="flex shrink-0 gap-2">
                      <button
                        type="button"
                        onClick={() => setEditingQuestionId(q.id)}
                        className="text-xs text-muted-foreground hover:text-foreground hover:underline"
                      >
                        Edit
                      </button>
                      <button
                        type="button"
                        onClick={() => handleDeleteQuestion(q.id)}
                        disabled={deleteQuestion.isPending}
                        className="text-xs text-destructive hover:underline disabled:opacity-50"
                      >
                        Delete
                      </button>
                    </div>
                  </div>
                </div>
              )}
            </li>
          ))}
        </ul>

        {addingQuestion && (
          <div className="mt-3">
            <QuestionEditor
              nextOrderIndex={quiz!.questions.length}
              onSave={handleAddQuestion}
              onCancel={() => setAddingQuestion(false)}
              saveLabel="Add Question"
              isSaving={addQuestion.isPending}
              saveError={addQuestion.isError}
            />
          </div>
        )}

        {deleteQuestion.isError && (
          <p className="mt-2 text-xs text-destructive">Failed to delete question.</p>
        )}
      </div>
    </div>
  )
}

// ─── Helpers ─────────────────────────────────────────────────────────────────────

function humanizeType(type: string): string {
  switch (type) {
    case 'SingleChoice': return 'Single Choice'
    case 'MultipleChoice': return 'Multiple Choice'
    case 'TrueFalse': return 'True / False'
    default: return type
  }
}

function buildInitialValues(q: Question): QuestionFormInput {
  return {
    text: q.text,
    type: q.type,
    orderIndex: q.orderIndex,
    // The GET quiz API doesn't expose IsCorrect — options come without it.
    // We pre-fill with isCorrect: false; admin must re-mark correct answers when editing.
    options: q.options.map((o) => ({
      text: o.text,
      isCorrect: false,
      orderIndex: o.orderIndex,
    })),
  }
}
