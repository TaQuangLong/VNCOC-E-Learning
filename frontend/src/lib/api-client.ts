import axios from 'axios'

export const apiClient = axios.create({
  baseURL: `${import.meta.env.VITE_API_URL}/api`,
  withCredentials: true, // send httpOnly cookie for refresh token
})

// Token getter — set by AuthProvider after login
let accessTokenGetter: (() => string | null) | null = null

export function setAccessTokenGetter(getter: () => string | null) {
  accessTokenGetter = getter
}

// Inject access token into every request
apiClient.interceptors.request.use((config) => {
  const token = accessTokenGetter?.()
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// Singleton refresh promise — prevents multiple concurrent refresh calls
let refreshPromise: Promise<string> | null = null

function doRefresh(): Promise<string> {
  if (!refreshPromise) {
    refreshPromise = axios
      .post<{ accessToken: string }>(
        `${import.meta.env.VITE_API_URL}/api/auth/refresh`,
        {},
        { withCredentials: true }
      )
      .then((r) => r.data.accessToken)
      .finally(() => {
        refreshPromise = null
      })
  }
  return refreshPromise
}

apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config
    // Skip refresh for auth endpoints that don't benefit from a retry
    if (
      originalRequest.url?.includes('/auth/refresh') ||
      originalRequest.url?.includes('/auth/login') ||
      originalRequest.url?.includes('/auth/register')
    ) {
      return Promise.reject(error)
    }
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true
      try {
        const newToken = await doRefresh()
        originalRequest.headers.Authorization = `Bearer ${newToken}`
        return apiClient(originalRequest)
      } catch {
        // Refresh failed — reject so callers (e.g. AuthContext) can handle navigation cleanly
        return Promise.reject(error)
      }
    }
    return Promise.reject(error)
  }
)

