import { api } from '@/lib/api'

export const assetSearchService = {
  search: (params) => api().get('/api/investments/search-assets', { params }),
}
