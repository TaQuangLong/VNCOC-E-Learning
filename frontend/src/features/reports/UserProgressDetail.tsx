import { Link } from 'react-router-dom'
import type { UserCourseProgress } from './types'

interface UserProgressDetailProps {
  courses: UserCourseProgress[]
}

export default function UserProgressDetail({
  courses,
}: UserProgressDetailProps) {
  if (courses.length === 0) {
    return (
      <p className="py-8 text-center text-sm text-muted-foreground">
        This user is not enrolled in any courses.
      </p>
    )
  }

  return (
    <div className="space-y-4">
      {courses.map((c) => (
        <div key={c.courseId} className="rounded-lg border bg-card shadow-sm">
          <div className="flex flex-col gap-1 border-b px-4 py-3 sm:flex-row sm:items-center sm:justify-between">
            <div>
              <Link
                to={`/admin/reports/courses/${c.courseId}/learners`}
                className="font-semibold hover:underline"
              >
                {c.courseTitle}
              </Link>
              <p className="text-xs text-muted-foreground">
                Enrolled {new Date(c.enrolledAt).toLocaleDateString()}
              </p>
            </div>
            <div className="flex items-center gap-3">
              <div className="flex items-center gap-2 text-sm">
                <div className="h-2 w-24 overflow-hidden rounded-full bg-muted">
                  <div
                    className="h-full bg-primary transition-all"
                    style={{ width: `${c.progressPercent}%` }}
                  />
                </div>
                <span>{c.progressPercent}%</span>
              </div>
              {c.isCompleted && (
                <span className="rounded-full bg-green-100 px-2 py-0.5 text-xs font-medium text-green-800">
                  Completed
                </span>
              )}
            </div>
          </div>

          <div className="px-4 py-3 text-sm text-muted-foreground">
            <span>
              {c.completedLessonsCount}/{c.totalLessonsCount} lessons
            </span>
          </div>

          {c.quizResults.length > 0 && (
            <div className="border-t px-4 py-3">
              <p className="mb-2 text-xs font-medium uppercase tracking-wide text-muted-foreground">
                Quiz Results
              </p>
              <div className="overflow-x-auto">
                <table className="w-full min-w-[400px] text-sm">
                  <thead>
                    <tr className="border-b text-left text-muted-foreground">
                      <th className="pb-1 pr-4 font-medium">Quiz</th>
                      <th className="pb-1 pr-4 font-medium">Score</th>
                      <th className="pb-1 pr-4 font-medium">Result</th>
                      <th className="pb-1 font-medium">Date</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y">
                    {c.quizResults.map((q) => (
                      <tr key={`${q.quizId}-${q.submittedAt}`}>
                        <td className="py-1.5 pr-4">{q.quizTitle}</td>
                        <td className="py-1.5 pr-4">
                          {Number(q.score).toFixed(1)}%
                        </td>
                        <td className="py-1.5 pr-4">
                          {q.passed ? (
                            <span className="text-green-700">Passed</span>
                          ) : (
                            <span className="text-destructive">Failed</span>
                          )}
                        </td>
                        <td className="py-1.5 text-muted-foreground">
                          {new Date(q.submittedAt).toLocaleDateString()}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}
        </div>
      ))}
    </div>
  )
}
