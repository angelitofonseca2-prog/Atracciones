import { apiClient } from './atraccionesApi'

/** GET /api/v1/clientes/perfil — requiere JWT con rol CLIENTE */
export async function obtenerPerfilCliente() {
  const response = await apiClient.get('/clientes/perfil')
  return response.data?.data ?? response.data ?? {}
}
