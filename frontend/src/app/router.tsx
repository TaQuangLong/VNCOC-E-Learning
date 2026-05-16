import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import LoginPage from '@/pages/public/LoginPage'
import RegisterPage from '@/pages/public/RegisterPage'
import ForgotPasswordPage from '@/pages/public/ForgotPasswordPage'
import ResetPasswordPage from '@/pages/public/ResetPasswordPage'
import CoursesPage from '@/pages/public/CoursesPage'
import CourseDetailPage from '@/pages/public/CourseDetailPage'
import AdminCoursesPage from '@/pages/admin/AdminCoursesPage'
import CreateCoursePage from '@/pages/admin/CreateCoursePage'
import EditCoursePage from '@/pages/admin/EditCoursePage'
import ProtectedRoute from '@/components/layout/ProtectedRoute'
import AdminRoute from '@/components/layout/AdminRoute'

export default function AppRouter() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Navigate to="/courses" replace />} />

        {/* Public auth routes */}
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route path="/forgot-password" element={<ForgotPasswordPage />} />
        <Route path="/reset-password" element={<ResetPasswordPage />} />

        {/* Public course routes */}
        <Route path="/courses" element={<CoursesPage />} />
        <Route path="/courses/:slug" element={<CourseDetailPage />} />

        {/* Student routes (protected) */}
        <Route
          path="/dashboard"
          element={
            <ProtectedRoute>
              <div>Dashboard (coming soon)</div>
            </ProtectedRoute>
          }
        />

        {/* Admin — courses */}
        <Route
          path="/admin/courses"
          element={
            <AdminRoute>
              <AdminCoursesPage />
            </AdminRoute>
          }
        />
        <Route
          path="/admin/courses/new"
          element={
            <AdminRoute>
              <CreateCoursePage />
            </AdminRoute>
          }
        />
        <Route
          path="/admin/courses/:id/edit"
          element={
            <AdminRoute>
              <EditCoursePage />
            </AdminRoute>
          }
        />

        {/* Catch-all */}
        <Route path="*" element={<div>404 — Page not found</div>} />
      </Routes>
    </BrowserRouter>
  )
}

