import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useSearchParams, useNavigate } from 'react-router-dom'
import { useState } from 'react'
import { authApi } from '@/features/auth/api'
import { resetPasswordSchema, type ResetPasswordInput } from '@/features/auth/types'
import { Button } from '@/components/ui/button'

export default function ResetPasswordPage() {
  const [searchParams] = useSearchParams()
  const navigate = useNavigate()
  const [success, setSuccess] = useState(false)

  const email = searchParams.get('email') ?? ''
  const token = searchParams.get('token') ?? ''

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
    setError,
  } = useForm<ResetPasswordInput>({
    resolver: zodResolver(resetPasswordSchema),
    defaultValues: { email, token },
  })

  const onSubmit = async (data: ResetPasswordInput) => {
    try {
      await authApi.resetPassword(data)
      setSuccess(true)
      setTimeout(() => navigate('/login'), 3000)
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Reset failed. The link may have expired.'
      setError('root', { message })
    }
  }

  if (success) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-background px-4">
        <div className="w-full max-w-sm space-y-4 text-center">
          <h1 className="text-2xl font-bold">Password Reset</h1>
          <p className="text-muted-foreground text-sm">
            Your password has been reset. Redirecting to sign in...
          </p>
        </div>
      </div>
    )
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-background px-4">
      <div className="w-full max-w-sm space-y-6">
        <div className="space-y-2 text-center">
          <h1 className="text-2xl font-bold">Reset Password</h1>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4" noValidate>
          <input type="hidden" {...register('email')} />
          <input type="hidden" {...register('token')} />

          <div className="space-y-1">
            <label htmlFor="newPassword" className="text-sm font-medium">New Password</label>
            <input
              id="newPassword"
              type="password"
              autoComplete="new-password"
              className="border-input bg-background w-full rounded-md border px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
              {...register('newPassword')}
            />
            {errors.newPassword && (
              <p className="text-destructive text-xs">{errors.newPassword.message}</p>
            )}
          </div>

          {errors.root && (
            <p className="bg-destructive/10 text-destructive rounded-md px-3 py-2 text-sm">
              {errors.root.message}
            </p>
          )}

          <Button type="submit" className="w-full" disabled={isSubmitting}>
            {isSubmitting ? 'Resetting...' : 'Reset Password'}
          </Button>
        </form>
      </div>
    </div>
  )
}
