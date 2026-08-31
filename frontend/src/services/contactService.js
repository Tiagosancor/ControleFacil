import { api } from '@/lib/api'

export const contactService = {
  send: (data) => api().post('/api/contact', data),
}
