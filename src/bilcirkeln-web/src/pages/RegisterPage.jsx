import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { getErrorMessage } from '../api/client'

export default function RegisterPage() {
  const { register } = useAuth()
  const navigate = useNavigate()
  const [fullName, setFullName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')

  async function handleSubmit(e) {
    e.preventDefault()
    setError('')
    try {
      await register(fullName, email, password)
      navigate('/salj')
    } catch (err) {
      setError(getErrorMessage(err))
    }
  }

  return (
    <form className="card form" onSubmit={handleSubmit}>
      <h2>Skapa konto</h2>
      {error && <p className="error">{error}</p>}
      <label>
        Namn
        <input value={fullName} onChange={(e) => setFullName(e.target.value)} required minLength={2} />
      </label>
      <label>
        E-post
        <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
      </label>
      <label>
        Lösenord (minst 8 tecken, en stor bokstav, en liten och en siffra)
        <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} required minLength={8} />
      </label>
      <button type="submit" className="button">Skapa konto</button>
    </form>
  )
}
