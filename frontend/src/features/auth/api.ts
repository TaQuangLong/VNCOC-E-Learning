import { apiClient } from '@/lib/api-client'
import type {
  ForgotPasswordInput,
  LoginInput,
  LoginResponse,
  RegisterInput,
  RegisterResponse,
  ResetPasswordInput,
  AuthUser,
} from './types'

export const authApi = {
  login: (data: LoginInput) =>
    apiClient.post<LoginResponse>('/auth/login', data).then((r) => r.data),

  register: (data: RegisterInput) =>
    apiClient.post<RegisterResponse>('/auth/register', data).then((r) => r.data),

  logout: () => apiClient.post('/auth/logout').then((r) => r.data),

  me: () => apiClient.get<AuthUser>('/auth/me').then((r) => r.data),

  forgotPassword: (data: ForgotPasswordInput) =>
    apiClient.post('/auth/forgot-password', data).then((r) => r.data),

  resetPassword: (data: ResetPasswordInput) =>
    apiClient.post('/auth/reset-password', data).then((r) => r.data),
}
