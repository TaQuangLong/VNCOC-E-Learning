import { createContext, useCallback, useEffect, useRef, useState, type ReactNode } from 'react'
import { authApi } from '@/features/auth/api'
import { setAccessTokenGetter } from '@/lib/api-client'
import type { AuthUser, LoginInput, RegisterInput } from '@/features/auth/types'

interface AuthContextValue {
  user: AuthUser | null
  accessToken: string | null
  isLoading: boolean
  login: (data: LoginInput) => Promise<void>
  register: (data: RegisterInput) => Promise<void>
  logout: () => Promise<void>
}

export const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null)
  const [accessToken, setAccessToken] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const tokenRef = useRef<string | null>(null)

  // Keep ref in sync so getter always returns latest value
  useEffect(() => {
    tokenRef.current = accessToken
  }, [accessToken])

  // Register getter with api-client once
  useEffect(() => {
    setAccessTokenGetter(() => tokenRef.current)
  }, [])

  // On mount, try to silently restore session via refresh cookie
  useEffect(() => {
    const restore = async () => {
      try {
        const { accessToken: newToken } = await authApi.refresh()
        setAccessToken(newToken)
        // Fetch user info with the new token — api-client interceptor will set it
        const me = await authApi.me()
        setUser(me)
      } catch {
        // No valid session — stay logged out
      } finally {
        setIsLoading(false)
      }
    }
    restore()
  }, [])

  const login = useCallback(async (data: LoginInput) => {
    const response = await authApi.login(data)
    setAccessToken(response.accessToken)
    setUser({
      userId: '',
      email: response.email,
      displayName: response.displayName,
      roles: response.roles,
    })
  }, [])

  const register = useCallback(async (data: RegisterInput) => {
    const response = await authApi.register(data)
    setAccessToken(response.accessToken)
    setUser({
      userId: '',
      email: response.email,
      displayName: response.displayName,
      roles: response.roles,
    })
  }, [])

  const logout = useCallback(async () => {
    try {
      await authApi.logout()
    } finally {
      setUser(null)
      setAccessToken(null)
    }
  }, [])

  return (
    <AuthContext.Provider value={{ user, accessToken, isLoading, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  )
}
