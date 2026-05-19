import { Link } from 'react-router-dom'
import type { AdminOverviewResponse } from './types'

interface StatCardProps {
  label: string
  value: number
}

function StatCard({ label, value }: StatCardProps) {
  return (
    <div className="rounded-lg border bg-card p-5 shadow-sm">
      <p className="text-sm text-muted-foreground">{label}</p>
      <p className="mt-1 text-3xl font-bold">{value.toLocaleString()}</p>
    </div>
  )
}

interface OverviewCardsProps {
  data: AdminOverviewResponse
}

export default function OverviewCards({ data }: OverviewCardsProps) {
  return (
    <div className="space-y-6">
      {/* Stat cards */}
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-5">
        <StatCard label="Total Users" value={data.totalUsers} />
        <StatCard
          label="Published Courses"
          value={data.totalPublishedCourses}
        />
        <StatCard
          label="Active Enrollments"
          value={data.totalActiveEnrollments}
        />
        <StatCard label="Quiz Attempts" value={data.totalQuizAttempts} />
        <StatCard
          label="New Users (7 days)"
          value={data.recentRegistrationsLast7Days}
        />
      </div>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        {/* Popular courses */}
        <div className="rounded-lg border bg-card shadow-sm">
          <div className="border-b px-4 py-3">
            <h2 className="font-semibold">Most Popular Courses</h2>
          </div>
          {data.mostPopularCourses.length === 0 ? (
            <p className="px-4 py-6 text-sm text-muted-foreground">
              No enrollments yet.
            </p>
          ) : (
            <ul className="divide-y">
              {data.mostPopularCourses.map((c) => (
                <li
                  key={c.courseId}
                  className="flex items-center justify-between px-4 py-3 text-sm"
                >
                  <Link
                    to={`/admin/reports/courses/${c.courseId}/learners`}
                    className="font-medium hover:underline"
                  >
                    {c.title}
                  </Link>
                  <span className="text-muted-foreground">
                    {c.enrollmentCount} enrolled
                  </span>
                </li>
              ))}
            </ul>
          )}
        </div>

        {/* Recent registrations */}
        <div className="rounded-lg border bg-card shadow-sm">
          <div className="border-b px-4 py-3">
            <h2 className="font-semibold">Recent Registrations</h2>
          </div>
          {data.recentRegistrations.length === 0 ? (
            <p className="px-4 py-6 text-sm text-muted-foreground">
              No recent registrations.
            </p>
          ) : (
            <ul className="divide-y">
              {data.recentRegistrations.map((u) => (
                <li key={u.userId} className="px-4 py-3 text-sm">
                  <div className="flex items-center justify-between">
                    <Link
                      to={`/admin/reports/users/${u.userId}/progress`}
                      className="font-medium hover:underline"
                    >
                      {u.displayName}
                    </Link>
                    <span className="text-xs text-muted-foreground">
                      {new Date(u.registeredAt).toLocaleDateString()}
                    </span>
                  </div>
                  <p className="text-muted-foreground">{u.email}</p>
                </li>
              ))}
            </ul>
          )}
        </div>
      </div>
    </div>
  )
}
