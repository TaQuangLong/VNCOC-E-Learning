import type { MyQuizAttemptsResponse } from './types'

interface Props {
  data: MyQuizAttemptsResponse
}

export default function AttemptList({ data }: Props) {
  if (data.attempts.length === 0) {
    return (
      <p className="text-sm text-muted-foreground">No previous attempts.</p>
    )
  }

  return (
    <div className="space-y-2">
      {data.bestScore !== null && (
        <p className="text-sm text-muted-foreground">
          Best score: <span className="font-medium">{Math.round(data.bestScore)}%</span>
        </p>
      )}
      <ul className="divide-y divide-border rounded-md border border-border">
        {data.attempts.map((attempt, index) => (
          <li
            key={attempt.id}
            className="flex items-center justify-between px-4 py-2.5 text-sm"
          >
            <span className="text-muted-foreground">
              Attempt {data.attempts.length - index}
              {' · '}
              {new Date(attempt.submittedAt).toLocaleDateString(undefined, {
                month: 'short',
                day: 'numeric',
                year: 'numeric',
              })}
            </span>
            <span
              className={`font-medium ${
                attempt.passed
                  ? 'text-green-600 dark:text-green-400'
                  : 'text-amber-600 dark:text-amber-400'
              }`}
            >
              {Math.round(attempt.score)}% · {attempt.passed ? 'Passed' : 'Failed'}
            </span>
          </li>
        ))}
      </ul>
    </div>
  )
}
