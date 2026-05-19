import { Link } from 'react-router-dom'
import type { CourseLearner } from './types'

interface CourseLearnersTableProps {
  learners: CourseLearner[]
}

export default function CourseLearnersTable({
  learners,
}: CourseLearnersTableProps) {
  if (learners.length === 0) {
    return (
      <p className="py-8 text-center text-sm text-muted-foreground">
        No learners enrolled in this course yet.
      </p>
    )
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full min-w-[640px] text-sm">
        <thead>
          <tr className="border-b text-left text-muted-foreground">
            <th className="pb-2 pr-4 font-medium">Learner</th>
            <th className="pb-2 pr-4 font-medium">Enrolled</th>
            <th className="pb-2 pr-4 font-medium">Progress</th>
            <th className="pb-2 pr-4 font-medium">Lessons</th>
            <th className="pb-2 pr-4 font-medium">Quizzes Passed</th>
            <th className="pb-2 font-medium">Completed</th>
          </tr>
        </thead>
        <tbody className="divide-y">
          {learners.map((l) => (
            <tr key={l.userId} className="hover:bg-muted/40">
              <td className="py-3 pr-4">
                <Link
                  to={`/admin/reports/users/${l.userId}/progress`}
                  className="font-medium hover:underline"
                >
                  {l.displayName}
                </Link>
                <p className="text-xs text-muted-foreground">{l.email}</p>
              </td>
              <td className="py-3 pr-4 text-muted-foreground">
                {new Date(l.enrolledAt).toLocaleDateString()}
              </td>
              <td className="py-3 pr-4">
                <div className="flex items-center gap-2">
                  <div className="h-2 w-20 overflow-hidden rounded-full bg-muted">
                    <div
                      className="h-full bg-primary transition-all"
                      style={{ width: `${l.progressPercent}%` }}
                    />
                  </div>
                  <span>{l.progressPercent}%</span>
                </div>
              </td>
              <td className="py-3 pr-4 text-muted-foreground">
                {l.completedLessonsCount}/{l.totalLessonsCount}
              </td>
              <td className="py-3 pr-4 text-muted-foreground">
                {l.quizPassedCount}
              </td>
              <td className="py-3">
                {l.completedAt ? (
                  <span className="rounded-full bg-green-100 px-2 py-0.5 text-xs font-medium text-green-800">
                    ✓ {new Date(l.completedAt).toLocaleDateString()}
                  </span>
                ) : (
                  <span className="text-muted-foreground">—</span>
                )}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
