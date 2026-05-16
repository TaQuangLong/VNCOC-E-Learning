import type { CourseStatus } from './types'

const statusStyles: Record<CourseStatus, string> = {
  Draft: 'bg-muted text-muted-foreground',
  Published: 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400',
  Archived: 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400',
}

interface Props {
  status: CourseStatus
}

export default function CourseStatusBadge({ status }: Props) {
  return (
    <span
      className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${statusStyles[status]}`}
    >
      {status}
    </span>
  )
}
