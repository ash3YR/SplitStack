import api from './api'

export const authService = {
  async login(payload) {
    return await api.post('/auth/login', payload)
  },

  async register(payload) {
    return await api.post('/auth/register', payload)
  },
}
