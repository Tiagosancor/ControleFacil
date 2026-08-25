import { api } from '@/lib/api'

export const bankService = {
  search: (params) => api().get('/api/banks', { params }),
}
