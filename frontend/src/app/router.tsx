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
import AdminLessonsPage from '@/pages/admin/AdminLessonsPage'
import CreateLessonPage from '@/pages/admin/CreateLessonPage'
import EditLessonPage from '@/pages/admin/EditLessonPage'
import LearnPage from '@/pages/student/LearnPage'
import LearnRedirectPage from '@/pages/student/LearnRedirectPage'
import MyLearningPage from '@/pages/student/MyLearningPage'
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

        {/* Student — my learning */}
        <Route
          path="/my-learning"
          element={
            <ProtectedRoute>
              <MyLearningPage />
            </ProtectedRoute>
          }
        />

        {/* Student — learning pages */}
        <Route
          path="/learn/:courseId"
          element={
            <ProtectedRoute>
              <LearnRedirectPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/learn/:courseId/lessons/:lessonId"
          element={
            <ProtectedRoute>
              <LearnPage />
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

        {/* Admin — lessons */}
        <Route
          path="/admin/courses/:courseId/lessons"
          element={
            <AdminRoute>
              <AdminLessonsPage />
            </AdminRoute>
          }
        />
        <Route
          path="/admin/courses/:courseId/lessons/new"
          element={
            <AdminRoute>
              <CreateLessonPage />
            </AdminRoute>
          }
        />
        <Route
          path="/admin/lessons/:id/edit"
          element={
            <AdminRoute>
              <EditLessonPage />
            </AdminRoute>
          }
        />

        {/* Catch-all */}
        <Route path="*" element={<div>404 — Page not found</div>} />
      </Routes>
    </BrowserRouter>
  )
}

