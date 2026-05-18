import axios from 'axios'

const AUTH_STORAGE_KEY = 'smart-splitter-auth'
const apiBaseUrl = import.meta.env.VITE_API_BASE_URL || '/api'

const api = axios.create({
  baseURL: apiBaseUrl,
  headers: {
    'Content-Type': 'application/json',
  },
})

api.interceptors.request.use((config) => {
  const saved = localStorage.getItem(AUTH_STORAGE_KEY)

  if (saved) {
    try {
      const { token } = JSON.parse(saved)
      if (token) {
        config.headers.Authorization = `Bearer ${token}`
      }
    } catch {
      localStorage.removeItem(AUTH_STORAGE_KEY)
    }
  }

  return config
})

api.interceptors.response.use(
  (response) => {
    const payload = response.data

    if (payload && typeof payload === 'object' && 'success' in payload) {
      if (!payload.success) {
        return Promise.reject(new Error(payload.message || 'The request failed.'))
      }

      return payload.data
    }

    return payload
  },
  (error) => {
    if (error?.response?.status === 401) {
      localStorage.removeItem(AUTH_STORAGE_KEY)
      window.dispatchEvent(new CustomEvent('auth:unauthorized'))
    }

    return Promise.reject(error)
  },
)

export default api
