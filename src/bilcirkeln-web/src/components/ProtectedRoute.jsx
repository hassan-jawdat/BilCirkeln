import { Navigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

// Only shows the page if the user is logged in (and has the right role).
// Note: this only hides pages in the UI. The real security is [Authorize] in the API.
export default function ProtectedRoute({ role, children }) {
  const { user } = useAuth()

  if (!user) {
    return <Navigate to="/login" replace />
  }

  if (role && user.role !== role) {
    return <Navigate to="/" replace />
  }

  return children
}
