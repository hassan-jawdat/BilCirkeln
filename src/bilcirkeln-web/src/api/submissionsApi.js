import client from './client'

export async function createSubmission(data) {
  const response = await client.post('/submissions', data)
  return response.data
}

export async function getMySubmissions() {
  const response = await client.get('/submissions/mine')
  return response.data
}

// status is optional, e.g. "Pending". Empty string = all.
export async function getAllSubmissions(status) {
  const response = await client.get('/submissions', { params: status ? { status } : {} })
  return response.data
}

export async function getSubmission(id) {
  const response = await client.get(`/submissions/${id}`)
  return response.data
}

export async function makeOffer(id, amount, message) {
  const response = await client.post(`/submissions/${id}/offers`, { amount, message })
  return response.data
}

export async function respondToOffer(id, accept) {
  const response = await client.post(`/submissions/${id}/respond`, { accept })
  return response.data
}

export async function rejectSubmission(id) {
  const response = await client.post(`/submissions/${id}/reject`)
  return response.data
}
