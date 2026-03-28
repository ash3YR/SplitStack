import api from './api'

export const groupService = {
  async getGroups() {
    return await api.get('/groups')
  },

  async createGroup(payload) {
    return await api.post('/groups', payload)
  },

  async getGroupById(groupId) {
    return await api.get(`/groups/${groupId}`)
  },

  async getGroupJoinRequests(groupId) {
    return await api.get(`/groups/${groupId}/join-requests`)
  },

  async addMember(groupId, payload) {
    return await api.post(`/groups/${groupId}/join-requests`, payload)
  },

  async removeMember(groupId, memberUserId) {
    return await api.delete(`/groups/${groupId}/members/${memberUserId}`)
  },
}
