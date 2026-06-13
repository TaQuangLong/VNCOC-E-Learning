import { useState } from 'react'
import { Link } from 'react-router-dom'
import { ArrowRight, BookOpen, Clock, Route } from 'lucide-react'

import CatalogNavigation from '@/components/layout/CatalogNavigation'
import { Button } from '@/components/ui/button'
import { useLearningPaths } from '@/features/learning-paths/api'
import type { LearningPathSummary } from '@/features/learning-paths/types'

function LearningPathCard({ learningPath }: { learningPath: LearningPathSummary }) {
  return (
    <Link
      to={`/learning-paths/${learningPath.slug}`}
      className="group flex h-full flex-col overflow-hidden rounded-xl border bg-card shadow-sm transition-all hover:-translate-y-0.5 hover:shadow-md"
    >
      <div className="aspect-[16/9] overflow-hidden bg-muted">
        {learningPath.thumbnailUrl ? (
          <img
            src={learningPath.thumbnailUrl}
            alt={learningPath.title}
            className="h-full w-full object-cover transition-transform duration-300 group-hover:scale-105"
          />
        ) : (
          <div className="flex h-full items-center justify-center">
            <Route className="size-12 text-muted-foreground" aria-hidden="true" />
          </div>
        )}
      </div>

      <div className="flex flex-1 flex-col gap-3 p-5">
        <h2 className="line-clamp-2 text-lg font-semibold leading-snug transition-colors group-hover:text-primary">
          {learningPath.title}
        </h2>
        {learningPath.shortDescription && (
          <p className="line-clamp-3 text-sm text-muted-foreground">
            {learningPath.shortDescription}
          </p>
        )}

        <div className="mt-auto flex flex-wrap items-center gap-x-4 gap-y-2 border-t pt-4 text-xs text-muted-foreground">
          <span className="inline-flex items-center gap-1.5">
            <BookOpen className="size-3.5" aria-hidden="true" />
            {learningPath.courseCount}{' '}
            {learningPath.courseCount === 1 ? 'course' : 'courses'}
          </span>
          {learningPath.estimatedDurationLabel && (
            <span className="inline-flex items-center gap-1.5">
              <Clock className="size-3.5" aria-hidden="true" />
              {learningPath.estimatedDurationLabel}
            </span>
          )}
        </div>

        <span className="inline-flex items-center gap-1 text-sm font-medium text-primary">
          Explore path
          <ArrowRight
            className="size-4 transition-transform group-hover:translate-x-1"
            aria-hidden="true"
          />
        </span>
      </div>
    </Link>
  )
}

function LearningPathCardSkeleton() {
  return (
    <div className="animate-pulse overflow-hidden rounded-xl border bg-card">
      <div className="aspect-[16/9] bg-muted" />
      <div className="space-y-3 p-5">
        <div className="h-5 w-3/4 rounded bg-muted" />
        <div className="h-3 w-full rounded bg-muted" />
        <div className="h-3 w-5/6 rounded bg-muted" />
        <div className="h-px bg-muted" />
        <div className="h-3 w-1/2 rounded bg-muted" />
      </div>
    </div>
  )
}

export default function LearningPathsPage() {
  const [page, setPage] = useState(1)
  const { data, isLoading, isError } = useLearningPaths({
    page,
    pageSize: 12,
  })

  const totalPages = data ? Math.ceil(data.totalCount / data.pageSize) : 0

  return (
    <div className="min-h-screen bg-background">
      <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
        <div className="mb-8 flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
          <div className="max-w-2xl space-y-2">
            <h1 className="text-3xl font-bold">Learning Paths</h1>
            <p className="text-muted-foreground">
              Follow curated course journeys designed to help you grow step by
              step.
            </p>
          </div>
          <CatalogNavigation />
        </div>

        {isLoading && (
          <div
            className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3"
            aria-label="Loading learning paths"
          >
            {Array.from({ length: 6 }).map((_, index) => (
              <LearningPathCardSkeleton key={index} />
            ))}
          </div>
        )}

        {isError && (
          <div className="rounded-md bg-destructive/10 px-4 py-3 text-sm text-destructive">
            Failed to load learning paths. Please try again.
          </div>
        )}

        {!isLoading && !isError && data?.items.length === 0 && (
          <div className="flex flex-col items-center gap-4 rounded-xl border border-dashed py-20 text-center">
            <Route className="size-12 text-muted-foreground" aria-hidden="true" />
            <div>
              <h2 className="font-semibold">No learning paths yet</h2>
              <p className="mt-1 text-sm text-muted-foreground">
                Published learning paths will appear here.
              </p>
            </div>
            <Button render={<Link to="/courses" />}>Browse Courses</Button>
          </div>
        )}

        {!isLoading && !isError && data && data.items.length > 0 && (
          <>
            <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
              {data.items.map((learningPath) => (
                <LearningPathCard
                  key={learningPath.id}
                  learningPath={learningPath}
                />
              ))}
            </div>

            {totalPages > 1 && (
              <div className="mt-8 flex items-center justify-center gap-3">
                <Button
                  variant="outline"
                  size="sm"
                  disabled={page <= 1}
                  onClick={() => setPage((currentPage) => currentPage - 1)}
                >
                  Previous
                </Button>
                <span className="text-sm text-muted-foreground">
                  Page {page} of {totalPages}
                </span>
                <Button
                  variant="outline"
                  size="sm"
                  disabled={page >= totalPages}
                  onClick={() => setPage((currentPage) => currentPage + 1)}
                >
                  Next
                </Button>
              </div>
            )}
          </>
        )}
      </div>
    </div>
  )
}
