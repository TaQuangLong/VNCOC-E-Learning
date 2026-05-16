import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'

export default function AppRouter() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Navigate to="/login" replace />} />
        {/* Auth routes */}
        <Route path="/login" element={<div>Login Page (coming soon)</div>} />
        {/* Student routes */}
        <Route path="/dashboard" element={<div>Dashboard (coming soon)</div>} />
        <Route path="/courses" element={<div>Courses (coming soon)</div>} />
        <Route path="/courses/:id" element={<div>Course Detail (coming soon)</div>} />
        {/* Admin routes */}
        <Route path="/admin" element={<div>Admin (coming soon)</div>} />
        {/* Catch-all */}
        <Route path="*" element={<div>404 — Page not found</div>} />
      </Routes>
    </BrowserRouter>
  )
}
