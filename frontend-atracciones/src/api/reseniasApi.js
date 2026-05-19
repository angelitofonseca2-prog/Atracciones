import { apiClient } from './atraccionesApi'

/**
 * GET /api/v2/atracciones/{atGuid}/resenias
 */
export const listarResenias = async (atGuid, params = {}) => {
  const response = await apiClient.get(`/atracciones/${atGuid}/resenias`, { params })
  return response.data
}

/**
 * POST /api/v2/atracciones/{atGuid}/resenias
 * body: { rev_guid, rating, comentario }
 */
export const crearResenia = async (atGuid, body) => {
  const response = await apiClient.post(`/atracciones/${atGuid}/resenias`, body)
  return response.data
}
