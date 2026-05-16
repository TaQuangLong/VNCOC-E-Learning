import { Link } from 'react-router-dom'
import type { CourseSummary } from './types'

interface Props {
  course: CourseSummary
}

export default function CourseCard({ course }: Props) {
  return (
    <Link
      to={`/courses/${course.slug}`}
      className="group block overflow-hidden rounded-xl border border-border bg-card transition-shadow hover:shadow-md"
    >
      <div className="aspect-video w-full overflow-hidden bg-muted">
        {course.thumbnailUrl ? (
          <img
            src={course.thumbnailUrl}
            alt={course.title}
            className="h-full w-full object-cover transition-transform duration-300 group-hover:scale-105"
          />
        ) : (
          <div className="flex h-full items-center justify-center text-sm text-muted-foreground">
            No thumbnail
          </div>
        )}
      </div>
      <div className="space-y-2 p-4">
        {course.category && (
          <span className="text-xs font-medium uppercase tracking-wide text-primary">
            {course.category}
          </span>
        )}
        <h3 className="line-clamp-2 font-semibold text-card-foreground transition-colors group-hover:text-primary">
          {course.title}
        </h3>
        {course.shortDescription && (
          <p className="line-clamp-2 text-sm text-muted-foreground">
            {course.shortDescription}
          </p>
        )}
        <div className="flex items-center gap-2 text-xs text-muted-foreground">
          <span>{course.authorName}</span>
          {course.level && (
            <>
              <span>·</span>
              <span>{course.level}</span>
            </>
          )}
        </div>
      </div>
    </Link>
  )
}
