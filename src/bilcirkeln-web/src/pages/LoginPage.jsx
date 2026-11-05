import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { getErrorMessage } from '../api/client'

export default function LoginPage() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')

  async function handleSubmit(e) {
    e.preventDefault() // Stop the browser from reloading the page.
    setError('')
    try {
      const user = await login(email, password)
      navigate(user.role === 'Personal' ? '/personal' : '/mina-bilar')
    } catch (err) {
      setError(getErrorMessage(err))
    }
  }

  return (
    <form className="card form" onSubmit={handleSubmit}>
      <h2>Logga in</h2>
      {error && <p className="error">{error}</p>}
      <label>
        E-post
        <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
      </label>
      <label>
        Lösenord
        <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} required />
      </label>
      <button type="submit" className="button">Logga in</button>
      <p>Inget konto? <Link to="/registrera">Skapa ett här</Link></p>
    </form>
  )
}
