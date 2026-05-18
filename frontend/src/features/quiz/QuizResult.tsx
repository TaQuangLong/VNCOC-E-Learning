import { Button } from '@/components/ui/button'
import type { SubmitQuizResponse, Quiz } from './types'

interface Props {
  quiz: Quiz
  result: SubmitQuizResponse
  onRetry: () => void
}

export default function QuizResult({ quiz, result, onRetry }: Props) {
  const scorePercent = Math.round(result.score)

  return (
    <div className="space-y-6">
      {/* Score card */}
      <div
        className={`rounded-lg border-2 px-6 py-8 text-center ${
          result.passed
            ? 'border-green-500 bg-green-50 dark:bg-green-950/20'
            : 'border-amber-400 bg-amber-50 dark:bg-amber-950/20'
        }`}
      >
        <p className="mb-1 text-4xl font-bold">
          {scorePercent}%
        </p>
        <p className="text-sm text-muted-foreground">
          {result.correctCount} / {result.totalCount} correct
        </p>
        <p
          className={`mt-3 text-lg font-semibold ${
            result.passed ? 'text-green-700 dark:text-green-400' : 'text-amber-700 dark:text-amber-400'
          }`}
        >
          {result.passed ? '🎉 Great job! You passed!' : 'Keep learning — you\'ve got this!'}
        </p>
        <p className="mt-1 text-xs text-muted-foreground">
          Passing score: {quiz.passingScore}%
        </p>
      </div>

      {/* Per-question breakdown */}
      <div>
        <h3 className="mb-3 text-sm font-semibold">Question Breakdown</h3>
        <ul className="space-y-2">
          {quiz.questions.map((q, index) => {
            const qResult = result.results.find((r) => r.questionId === q.id)
            const isCorrect = qResult?.isCorrect ?? false
            return (
              <li
                key={q.id}
                className={`flex items-start gap-3 rounded-md border px-3 py-2.5 text-sm ${
                  isCorrect
                    ? 'border-green-200 bg-green-50/50 dark:border-green-800 dark:bg-green-950/10'
                    : 'border-red-200 bg-red-50/50 dark:border-red-800 dark:bg-red-950/10'
                }`}
              >
                <span
                  className={`mt-0.5 shrink-0 text-base ${
                    isCorrect ? 'text-green-600 dark:text-green-400' : 'text-red-500 dark:text-red-400'
                  }`}
                  aria-hidden="true"
                >
                  {isCorrect ? '✓' : '✗'}
                </span>
                <span className="flex-1 leading-snug">
                  {index + 1}. {q.text}
                </span>
              </li>
            )
          })}
        </ul>
      </div>

      {/* Retry */}
      <Button variant="outline" onClick={onRetry}>
        Try Again
      </Button>
    </div>
  )
}
