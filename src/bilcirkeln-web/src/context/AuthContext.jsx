import { createContext, useContext, useState } from 'react'
import * as authApi from '../api/authApi'

// Context = a way to share data (here: the logged in user) with all components
// without passing it as props through every level.
const AuthContext = createContext(null)

function loadUser() {
  const saved = localStorage.getItem('user')
  return saved ? JSON.parse(saved) : null
}

export function AuthProvider({ children }) {
  const [user, setUser] = useState(loadUser)

  function saveSession(data) {
    localStorage.setItem('token', data.token)
    const userInfo = { email: data.email, fullName: data.fullName, role: data.role }
    localStorage.setItem('user', JSON.stringify(userInfo))
    setUser(userInfo)
    return userInfo
  }

  async function login(email, password) {
    return saveSession(await authApi.login(email, password))
  }

  async function register(fullName, email, password) {
    return saveSession(await authApi.register(fullName, email, password))
  }

  function logout() {
    localStorage.removeItem('token')
    localStorage.removeItem('user')
    setUser(null)
  }

  const value = {
    user,
    isStaff: user?.role === 'Personal',
    isCustomer: user?.role === 'Kund',
    login,
    register,
    logout,
  }

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

// Small helper so components can write: const { user } = useAuth()
// eslint-disable-next-line react-refresh/only-export-components
export function useAuth() {
  return useContext(AuthContext)
}
