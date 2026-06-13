import { NavLink } from 'react-router-dom'

import UserAvatarMenu from '@/components/layout/UserAvatarMenu'
import { cn } from '@/lib/utils'

const catalogLinks = [
  { to: '/courses', label: 'Courses' },
  { to: '/learning-paths', label: 'Learning Paths' },
]

export default function CatalogNavigation() {
  return (
    <div className="flex items-center gap-2 sm:gap-3">
      <nav aria-label="Catalog navigation" className="flex items-center gap-1">
        {catalogLinks.map(({ to, label }) => (
          <NavLink
            key={to}
            to={to}
            className={({ isActive }) =>
              cn(
                'rounded-md px-3 py-2 text-sm font-medium text-muted-foreground transition-colors hover:bg-muted hover:text-foreground',
                isActive && 'bg-muted text-foreground',
              )
            }
          >
            {label}
          </NavLink>
        ))}
      </nav>
      <UserAvatarMenu />
    </div>
  )
}
