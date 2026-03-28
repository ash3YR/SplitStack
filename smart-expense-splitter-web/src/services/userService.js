import api from './api'

export const userService = {
  async searchUsers(query) {
    return await api.get('/users/search', {
      params: { query },
    })
  },
}
