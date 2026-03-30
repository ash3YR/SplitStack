import api from './api'

export const balanceService = {
  async getGroupBalances(groupId) {
    return await api.get(`/groups/${groupId}/balances`)
  },
}
