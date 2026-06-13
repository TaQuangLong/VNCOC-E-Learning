import { CheckCircle2, X, XCircle } from 'lucide-react'

import { cn } from '@/lib/utils'

export interface ToastMessage {
  type: 'success' | 'error'
  text: string
}

interface ToastProps {
  message: ToastMessage | null
  onDismiss: () => void
}

export function Toast({ message, onDismiss }: ToastProps) {
  if (!message) return null

  const Icon = message.type === 'success' ? CheckCircle2 : XCircle

  return (
    <div
      role={message.type === 'error' ? 'alert' : 'status'}
      className={cn(
        'fixed right-4 bottom-4 z-[100] flex w-[calc(100%-2rem)] max-w-sm items-start gap-3 rounded-lg border bg-background p-4 text-sm shadow-lg',
        message.type === 'success'
          ? 'border-green-200 text-green-800'
          : 'border-destructive/30 text-destructive',
      )}
    >
      <Icon className="mt-0.5 size-4 shrink-0" aria-hidden="true" />
      <span className="flex-1">{message.text}</span>
      <button
        type="button"
        onClick={onDismiss}
        className="rounded-sm text-muted-foreground hover:text-foreground focus:outline-none focus:ring-2 focus:ring-ring"
        aria-label="Dismiss notification"
      >
        <X className="size-4" />
      </button>
    </div>
  )
}
