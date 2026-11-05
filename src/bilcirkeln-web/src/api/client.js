import axios from 'axios'

// One shared axios instance for all API calls.
const client = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? 'http://localhost:5221/api',
})

// Interceptor: runs before every request and adds the JWT token if we have one.
client.interceptors.request.use((config) => {
  const token = localStorage.getItem('token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// If the token has expired the API answers 401. Then we log out and go to the login page.
client.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401 && localStorage.getItem('token')) {
      localStorage.removeItem('token')
      localStorage.removeItem('user')
      window.location.href = '/login'
    }
    return Promise.reject(error)
  },
)

// Turns an axios error into a readable Swedish message.
export function getErrorMessage(error) {
  const data = error.response?.data
  if (data?.message) return data.message
  if (data?.errors) return Object.values(data.errors).flat().join(' ')
  if (error.response?.status === 401) return 'Du måste logga in igen.'
  if (error.response?.status === 403) return 'Du har inte behörighet.'
  return 'Något gick fel. Kontrollera att API:et är igång.'
}

export default client
