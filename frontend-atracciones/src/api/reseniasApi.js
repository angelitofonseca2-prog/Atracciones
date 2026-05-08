import { apiClient } from './atraccionesApi'

/**
 * GET /api/v1/resenias?atraccionGuid={guid}
 * Devuelve reseñas de una atracción (sin autenticación requerida).
 */
export const listarResenias = async (params = {}) => {
  const response = await apiClient.get('/resenias', { params })
  return response.data
}

export const crearResenia = async (body) => {
  // body: { rev_guid, rating, comentario }
  const response = await apiClient.post('/resenias', body)
  return response.data
}

export const editarResenia = async (guid, body) => {
  const response = await apiClient.put(`/resenias/${guid}`, body)
  return response.data
}

export const eliminarResenia = async (guid) => {
  const response = await apiClient.delete(`/resenias/${guid}`)
  return response.data
}
