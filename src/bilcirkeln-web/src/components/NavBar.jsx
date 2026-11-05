import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

export default function NavBar() {
  const { user, isStaff, isCustomer, logout } = useAuth()
  const navigate = useNavigate()

  function handleLogout() {
    logout()
    navigate('/')
  }

  return (
    <header className="navbar">
      <Link to="/" className="brand">BilCirkeln</Link>
      <nav>
        {isCustomer && (
          <>
            <Link to="/salj">Sälj din bil</Link>
            <Link to="/mina-bilar">Mina bilar</Link>
          </>
        )}
        {isStaff && <Link to="/personal">Förfrågningar</Link>}
        {user ? (
          <>
            <span className="user-name">{user.fullName}</span>
            <button className="link-button" onClick={handleLogout}>Logga ut</button>
          </>
        ) : (
          <>
            <Link to="/login">Logga in</Link>
            <Link to="/registrera">Skapa konto</Link>
          </>
        )}
      </nav>
    </header>
  )
}
