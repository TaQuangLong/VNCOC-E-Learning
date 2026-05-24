import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useNavigate, Link } from 'react-router-dom'
import { useAuth } from '@/hooks/useAuth'
import { loginSchema, type LoginInput } from '@/features/auth/types'
import { Button } from '@/components/ui/button'

export default function LoginPage() {
  const { login } = useAuth()
  const navigate = useNavigate()

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
    setError,
  } = useForm<LoginInput>({
    resolver: zodResolver(loginSchema),
    defaultValues: import.meta.env.DEV
      ? { email: import.meta.env.VITE_DEV_EMAIL ?? '', password: import.meta.env.VITE_DEV_PASSWORD ?? '' }
      : undefined,
  })

  const onSubmit = async (data: LoginInput) => {
    try {
      await login(data)
      navigate('/dashboard')
    } catch (err: unknown) {
      const message =
        err instanceof Error ? err.message : 'Invalid email or password'
      setError('root', { message })
    }
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-background px-4">
      <div className="w-full max-w-sm space-y-6">
        <div className="space-y-2 text-center">
          <h1 className="text-2xl font-bold">Sign In</h1>
          <p className="text-muted-foreground text-sm">ChurchLearn</p>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4" noValidate>
          <div className="space-y-1">
            <label htmlFor="email" className="text-sm font-medium">Email</label>
            <input
              id="email"
              type="email"
              autoComplete="email"
              className="border-input bg-background w-full rounded-md border px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
              {...register('email')}
            />
            {errors.email && (
              <p className="text-destructive text-xs">{errors.email.message}</p>
            )}
          </div>

          <div className="space-y-1">
            <label htmlFor="password" className="text-sm font-medium">Password</label>
            <input
              id="password"
              type="password"
              autoComplete="current-password"
              className="border-input bg-background w-full rounded-md border px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
              {...register('password')}
            />
            {errors.password && (
              <p className="text-destructive text-xs">{errors.password.message}</p>
            )}
          </div>

          {errors.root && (
            <p className="bg-destructive/10 text-destructive rounded-md px-3 py-2 text-sm">
              {errors.root.message}
            </p>
          )}

          <Button type="submit" className="w-full" disabled={isSubmitting}>
            {isSubmitting ? 'Signing in...' : 'Sign In'}
          </Button>
        </form>

        <div className="space-y-2 text-center text-sm">
          <Link to="/forgot-password" className="text-primary hover:underline block">
            Forgot your password?
          </Link>
          <span className="text-muted-foreground">
            Don&apos;t have an account?{' '}
            <Link to="/register" className="text-primary hover:underline">
              Register
            </Link>
          </span>
        </div>
      </div>
    </div>
  )
}
