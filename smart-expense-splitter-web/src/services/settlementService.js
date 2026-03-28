import api from './api'

export const settlementService = {
  async getGroupSettlements(groupId) {
    return await api.get(`/groups/${groupId}/settlements`)
  },
}
