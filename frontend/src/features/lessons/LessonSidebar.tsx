import { useState } from 'react'
import { Link } from 'react-router-dom'
import type { LessonSummary } from './types'

interface LessonSidebarProps {
  courseId: number
  lessons: LessonSummary[]
  activeLessonId: number
}

function formatDuration(seconds: number): string {
  if (seconds <= 0) return ''
  const m = Math.floor(seconds / 60)
  const s = seconds % 60
  return s > 0 ? `${m}m ${s}s` : `${m}m`
}

export default function LessonSidebar({
  courseId,
  lessons,
  activeLessonId,
}: LessonSidebarProps) {
  const [drawerOpen, setDrawerOpen] = useState(false)

  const sidebarContent = (
    <nav aria-label="Lesson list">
      <ul className="space-y-1 p-2">
        {lessons.map((lesson) => {
          const isActive = lesson.id === activeLessonId
          return (
            <li key={lesson.id}>
              <Link
                to={`/learn/${courseId}/lessons/${lesson.id}`}
                onClick={() => setDrawerOpen(false)}
                className={`flex items-start gap-3 rounded-md px-3 py-2.5 text-sm transition-colors ${
                  isActive
                    ? 'bg-primary text-primary-foreground'
                    : 'text-foreground hover:bg-muted'
                }`}
              >
                {/* Placeholder completion circle */}
                <span
                  className={`mt-0.5 flex h-5 w-5 shrink-0 items-center justify-center rounded-full border-2 text-xs ${
                    isActive
                      ? 'border-primary-foreground/60 text-primary-foreground/60'
                      : 'border-muted-foreground/40 text-muted-foreground/40'
                  }`}
                  aria-hidden="true"
                >
                  {lesson.orderIndex + 1}
                </span>
                <span className="flex-1 leading-snug">
                  <span className="block font-medium">{lesson.title}</span>
                  {lesson.durationSeconds > 0 && (
                    <span
                      className={`block text-xs ${isActive ? 'text-primary-foreground/70' : 'text-muted-foreground'}`}
                    >
                      {formatDuration(lesson.durationSeconds)}
                    </span>
                  )}
                </span>
                {lesson.isPreview && !isActive && (
                  <span className="mt-0.5 rounded bg-green-100 px-1.5 py-0.5 text-xs font-medium text-green-700">
                    Free
                  </span>
                )}
              </Link>
            </li>
          )
        })}
      </ul>
    </nav>
  )

  return (
    <>
      {/* Mobile toggle button */}
      <div className="flex items-center gap-2 border-b border-border px-4 py-2 lg:hidden">
        <button
          type="button"
          onClick={() => setDrawerOpen((o) => !o)}
          className="flex items-center gap-2 text-sm font-medium text-foreground"
          aria-expanded={drawerOpen}
          aria-controls="lesson-drawer"
        >
          <svg
            className="h-4 w-4"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
            strokeWidth={2}
            aria-hidden="true"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              d="M4 6h16M4 12h16M4 18h16"
            />
          </svg>
          {drawerOpen ? 'Hide lessons' : 'Show lessons'}
        </button>
      </div>

      {/* Mobile drawer */}
      {drawerOpen && (
        <div
          id="lesson-drawer"
          className="border-b border-border bg-background lg:hidden"
        >
          {sidebarContent}
        </div>
      )}

      {/* Desktop sidebar */}
      <aside className="hidden w-72 shrink-0 overflow-y-auto border-r border-border lg:block">
        <div className="sticky top-0 border-b border-border bg-background px-4 py-3">
          <p className="text-sm font-semibold text-foreground">Lessons</p>
        </div>
        {sidebarContent}
      </aside>
    </>
  )
}
