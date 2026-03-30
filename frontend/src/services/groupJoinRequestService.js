import api from './api'

export const groupJoinRequestService = {
  async getIncomingRequests() {
    return await api.get('/group-join-requests/incoming')
  },

  async acceptRequest(requestId) {
    return await api.post(`/group-join-requests/${requestId}/accept`)
  },

  async rejectRequest(requestId) {
    return await api.post(`/group-join-requests/${requestId}/reject`)
  },
}
