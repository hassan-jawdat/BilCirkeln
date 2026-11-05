import { Route, Routes } from 'react-router-dom'
import NavBar from './components/NavBar'
import ProtectedRoute from './components/ProtectedRoute'
import HomePage from './pages/HomePage'
import LoginPage from './pages/LoginPage'
import RegisterPage from './pages/RegisterPage'
import SellCarPage from './pages/SellCarPage'
import MySubmissionsPage from './pages/MySubmissionsPage'
import SubmissionDetailsPage from './pages/SubmissionDetailsPage'
import StaffDashboardPage from './pages/StaffDashboardPage'

// All pages (routes) in the app.
export default function App() {
  return (
    <>
      <NavBar />
      <main className="container">
        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/registrera" element={<RegisterPage />} />
          <Route
            path="/salj"
            element={<ProtectedRoute role="Kund"><SellCarPage /></ProtectedRoute>}
          />
          <Route
            path="/mina-bilar"
            element={<ProtectedRoute role="Kund"><MySubmissionsPage /></ProtectedRoute>}
          />
          <Route
            path="/personal"
            element={<ProtectedRoute role="Personal"><StaffDashboardPage /></ProtectedRoute>}
          />
          <Route
            path="/forfragan/:id"
            element={<ProtectedRoute><SubmissionDetailsPage /></ProtectedRoute>}
          />
          <Route path="*" element={<p>Sidan finns inte.</p>} />
        </Routes>
      </main>
    </>
  )
}
