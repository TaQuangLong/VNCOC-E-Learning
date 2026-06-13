import {
  BookOpen,
  LayoutDashboard,
  Route,
  Users,
} from 'lucide-react'
import { Link, NavLink } from 'react-router-dom'

import { cn } from '@/lib/utils'

const adminLinks = [
  {
    to: '/admin/dashboard',
    label: 'Dashboard',
    icon: LayoutDashboard,
  },
  {
    to: '/admin/courses',
    label: 'Courses',
    icon: BookOpen,
  },
  {
    to: '/admin/learning-paths',
    label: 'Learning Paths',
    icon: Route,
  },
  {
    to: '/admin/authors',
    label: 'Authors',
    icon: Users,
  },
]

export default function AdminSidebar() {
  return (
    <aside className="border-b bg-muted/30 lg:min-h-screen lg:border-b-0 lg:border-r">
      <div className="lg:sticky lg:top-0 lg:p-5">
        <Link
          to="/admin/dashboard"
          className="hidden text-lg font-semibold lg:block"
        >
          ChurchLearn Admin
        </Link>

        <nav
          aria-label="Admin navigation"
          className="flex overflow-x-auto px-4 py-3 lg:mt-6 lg:flex-col lg:gap-1 lg:overflow-visible lg:p-0"
        >
          {adminLinks.map(({ to, label, icon: Icon }) => (
            <NavLink
              key={to}
              to={to}
              className={({ isActive }) =>
                cn(
                  'inline-flex min-h-10 shrink-0 items-center gap-2 rounded-md px-3 py-2 text-sm font-medium text-muted-foreground transition-colors hover:bg-muted hover:text-foreground',
                  isActive && 'bg-muted text-foreground',
                )
              }
            >
              <Icon className="size-4" aria-hidden="true" />
              {label}
            </NavLink>
          ))}
        </nav>
      </div>
    </aside>
  )
}
