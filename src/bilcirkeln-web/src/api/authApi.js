import client from './client'

export async function login(email, password) {
  const response = await client.post('/auth/login', { email, password })
  return response.data
}

export async function register(fullName, email, password) {
  const response = await client.post('/auth/register', { fullName, email, password })
  return response.data
}
