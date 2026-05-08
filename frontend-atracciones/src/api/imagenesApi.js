import { apiClient } from './atraccionesApi'

/**
 * Cliente del recurso `/admin/imagenes` (rol Admin).
 *
 * Contrato (Swagger):
 *  - POST  /admin/imagenes        body: { url, descripcion? }
 *      → 201 ApiItemResponse<{ img_guid, url, descripcion?, estado, fecha_ingreso }>
 *  - GET   /admin/imagenes        → 200 ApiItemResponse<ImagenResponse[]>
 *  - PUT   /admin/imagenes/{guid} body: { url?, descripcion? }
 *  - DELETE /admin/imagenes/{guid} → 204
 *
 * Flujo "URL → img_guid" usado por FormularioAtraccion:
 *  1) Usuario ingresa una URL nueva.
 *  2) Frontend hace POST a /admin/imagenes con { url, descripcion? }.
 *  3) Backend devuelve `img_guid`; éste se agrega al array `imagen_guids`
 *     enviado al crear/actualizar la atracción.
 */
export const imagenesApi = {
  listar: async () => {
    const response = await apiClient.get('/admin/imagenes')
    return response.data?.data || []
  },
  crear: async ({ url, descripcion }) => {
    const payload = { url }
    if (descripcion && descripcion.trim()) payload.descripcion = descripcion.trim()
    const response = await apiClient.post('/admin/imagenes', payload)
    return response.data?.data || response.data
  },
  actualizar: async (guid, body) => {
    const response = await apiClient.put(`/admin/imagenes/${guid}`, body)
    return response.data?.data || response.data
  },
  eliminar: async (guid) => {
    const response = await apiClient.delete(`/admin/imagenes/${guid}`)
    return response.data
  },
}
