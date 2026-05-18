import { useState } from 'react'
import { Button } from '@/components/ui/button'
import { useSubmitQuiz, useMyQuizAttempts } from './api'
import QuizResult from './QuizResult'
import AttemptList from './AttemptList'
import type { Quiz, SubmitQuizResponse } from './types'

interface Props {
  quiz: Quiz
  courseId: number
}

export default function QuizPlayer({ quiz, courseId }: Props) {
  const [answers, setAnswers] = useState<Map<number, number[]>>(new Map())
  const [result, setResult] = useState<SubmitQuizResponse | null>(null)
  const submitQuiz = useSubmitQuiz(courseId)
  const { data: attemptsData, isLoading: attemptsLoading } = useMyQuizAttempts(quiz.id)

  const allAnswered = quiz.questions.every(
    (q) => (answers.get(q.id)?.length ?? 0) > 0,
  )

  const handleSingleSelect = (questionId: number, optionId: number) => {
    setAnswers((prev) => new Map(prev).set(questionId, [optionId]))
  }

  const handleMultiToggle = (questionId: number, optionId: number) => {
    setAnswers((prev) => {
      const next = new Map(prev)
      const current = next.get(questionId) ?? []
      if (current.includes(optionId)) {
        next.set(questionId, current.filter((id) => id !== optionId))
      } else {
        next.set(questionId, [...current, optionId])
      }
      return next
    })
  }

  const handleSubmit = async () => {
    const submitData = quiz.questions.map((q) => ({
      questionId: q.id,
      selectedOptionIds: answers.get(q.id) ?? [],
    }))
    const response = await submitQuiz.mutateAsync({
      quizId: quiz.id,
      data: { answers: submitData },
    })
    setResult(response)
  }

  const handleRetry = () => {
    setAnswers(new Map())
    setResult(null)
  }

  return (
    <div className="space-y-8">
      {/* Quiz header */}
      <div>
        <h2 className="text-lg font-bold">{quiz.title}</h2>
        {quiz.description && (
          <p className="mt-1 text-sm text-muted-foreground">{quiz.description}</p>
        )}
        <p className="mt-1 text-xs text-muted-foreground">
          {quiz.questions.length} question{quiz.questions.length !== 1 ? 's' : ''} · Passing score:{' '}
          {quiz.passingScore}%
        </p>
      </div>

      {result ? (
        <QuizResult quiz={quiz} result={result} onRetry={handleRetry} />
      ) : (
        <>
          {/* Questions */}
          <ol className="space-y-6">
            {quiz.questions.map((q, index) => {
              const selected = answers.get(q.id) ?? []

              return (
                <li key={q.id} className="space-y-3">
                  <p className="text-sm font-medium leading-snug">
                    {index + 1}. {q.text}
                  </p>

                  {/* TrueFalse — two-button toggle */}
                  {q.type === 'TrueFalse' && (
                    <div className="flex gap-2">
                      {q.options.map((opt) => {
                        const isSelected = selected.includes(opt.id)
                        return (
                          <button
                            key={opt.id}
                            type="button"
                            onClick={() => handleSingleSelect(q.id, opt.id)}
                            className={`rounded-md border px-5 py-2 text-sm font-medium transition-colors ${
                              isSelected
                                ? 'border-primary bg-primary text-primary-foreground'
                                : 'border-border bg-background hover:bg-muted'
                            }`}
                          >
                            {opt.text}
                          </button>
                        )
                      })}
                    </div>
                  )}

                  {/* SingleChoice — radio group */}
                  {q.type === 'SingleChoice' && (
                    <ul className="space-y-2">
                      {q.options.map((opt) => {
                        const isSelected = selected.includes(opt.id)
                        return (
                          <li key={opt.id}>
                            <label
                              className={`flex cursor-pointer items-center gap-3 rounded-md border px-4 py-2.5 text-sm transition-colors ${
                                isSelected
                                  ? 'border-primary bg-primary/5'
                                  : 'border-border hover:bg-muted'
                              }`}
                            >
                              <input
                                type="radio"
                                name={`q-${q.id}`}
                                value={opt.id}
                                checked={isSelected}
                                onChange={() => handleSingleSelect(q.id, opt.id)}
                                className="h-4 w-4 shrink-0 accent-primary"
                              />
                              {opt.text}
                            </label>
                          </li>
                        )
                      })}
                    </ul>
                  )}

                  {/* MultipleChoice — checkbox group */}
                  {q.type === 'MultipleChoice' && (
                    <ul className="space-y-2">
                      {q.options.map((opt) => {
                        const isSelected = selected.includes(opt.id)
                        return (
                          <li key={opt.id}>
                            <label
                              className={`flex cursor-pointer items-center gap-3 rounded-md border px-4 py-2.5 text-sm transition-colors ${
                                isSelected
                                  ? 'border-primary bg-primary/5'
                                  : 'border-border hover:bg-muted'
                              }`}
                            >
                              <input
                                type="checkbox"
                                checked={isSelected}
                                onChange={() => handleMultiToggle(q.id, opt.id)}
                                className="h-4 w-4 shrink-0 accent-primary"
                              />
                              {opt.text}
                            </label>
                          </li>
                        )
                      })}
                    </ul>
                  )}
                </li>
              )
            })}
          </ol>

          {/* Submit */}
          <div className="space-y-2">
            <Button
              onClick={handleSubmit}
              disabled={!allAnswered || submitQuiz.isPending}
            >
              {submitQuiz.isPending ? 'Submitting…' : 'Submit Quiz'}
            </Button>
            {!allAnswered && (
              <p className="text-xs text-muted-foreground">
                Answer all questions to submit.
              </p>
            )}
            {submitQuiz.isError && (
              <p className="text-xs text-destructive">
                Failed to submit quiz. Please try again.
              </p>
            )}
          </div>
        </>
      )}

      {/* Previous attempts */}
      <div>
        <h3 className="mb-3 text-sm font-semibold">Previous Attempts</h3>
        {attemptsLoading ? (
          <div className="h-8 w-full animate-pulse rounded-md bg-muted" />
        ) : attemptsData ? (
          <AttemptList data={attemptsData} />
        ) : null}
      </div>
    </div>
  )
}
