import api from './api'

export const expenseService = {
  async createExpense(payload) {
    return await api.post('/expenses', payload)
  },

  async updateExpensePayments(expenseId, payload) {
    return await api.put(`/expenses/${expenseId}/payments`, payload)
  },

  async getGroupExpenses(groupId) {
    return await api.get(`/groups/${groupId}/expenses`)
  },
}
