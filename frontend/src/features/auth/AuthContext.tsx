import axios from 'axios'
import { createContext, useCallback, useEffect, useRef, useState, type ReactNode } from 'react'
import { authApi } from '@/features/auth/api'
import { setAccessTokenGetter } from '@/lib/api-client'
import type { AuthUser, LoginInput, RegisterInput } from '@/features/auth/types'

interface AuthContextValue {
  user: AuthUser | null
  isLoading: boolean
  login: (data: LoginInput) => Promise<string[]>
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

  // On mount, try to silently restore session via refresh cookie.
  // Use bare axios (not apiClient) to avoid triggering the 401 interceptor.
  useEffect(() => {
    const restore = async () => {
      try {
        const { data } = await axios.post<{ accessToken: string }>(
          `${import.meta.env.VITE_API_URL}/api/auth/refresh`,
          {},
          { withCredentials: true }
        )
        // Update ref directly so the next API call (me) picks up the token immediately,
        // without waiting for the React re-render cycle to run the sync effect.
        tokenRef.current = data.accessToken
        setAccessToken(data.accessToken)
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

  const login = useCallback(async (data: LoginInput): Promise<string[]> => {
    const response = await authApi.login(data)
    tokenRef.current = response.accessToken
    setAccessToken(response.accessToken)
    setUser({
      userId: response.userId,
      email: response.email,
      displayName: response.displayName,
      roles: response.roles,
    })
    return response.roles
  }, [])

  const register = useCallback(async (data: RegisterInput) => {
    const response = await authApi.register(data)
    tokenRef.current = response.accessToken
    setAccessToken(response.accessToken)
    setUser({
      userId: response.userId,
      email: response.email,
      displayName: response.displayName,
      roles: response.roles,
    })
  }, [])

  const logout = useCallback(async () => {
    try {
      await authApi.logout()
    } catch {
      // Server-side logout failure is non-fatal; clean up client state regardless
    } finally {
      setUser(null)
      setAccessToken(null)
    }
  }, [])

  return (
    <AuthContext.Provider value={{ user, isLoading, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  )
}
