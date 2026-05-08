import { apiClient } from './atraccionesApi'

/**
 * Cliente API administrativo.
 *
 * Todos los endpoints exigen rol Admin (policy "SoloAdmin" en backend).
 * El backend serializa en snake_case (Program.cs → JsonNamingPolicy.SnakeCaseLower)
 * y envuelve respuestas en `{ status, message, data, pagination? }`.
 *
 * Convención:
 *  - Métodos `listar*` que devuelven listas planas retornan directamente `response.data.data`.
 *  - Métodos `obtener*` retornan `response.data.data`.
 *  - Métodos que pueden necesitar la metainfo de paginación (ej. listas administrativas
 *    grandes) devuelven el envelope completo `{ data, pagination }`.
 */
export const adminApi = {
  // ─── Atracciones ──────────────────────────────────────────────────────────
  listarAtraccionesAdmin: async (params = {}) => {
    const response = await apiClient.get('/admin/atracciones', { params })
    const raw = response.data?.data ?? response.data ?? []
    return Array.isArray(raw) ? raw : []
  },
  /**
   * Igual que `listarAtraccionesAdmin` pero retorna el envelope completo
   * `{ data, pagination }` para que la UI pueda paginar.
   */
  listarAtraccionesAdminConPaginacion: async (params = {}) => {
    const response = await apiClient.get('/admin/atracciones', { params })
    return {
      data: response.data?.data || [],
      pagination: response.data?.pagination || null,
    }
  },
  listarTodasAtraccionesAdmin: async () => {
    const LIMIT = 50
    let page = 1
    let todas = []
    let totalPages = 1

    do {
      const response = await apiClient.get('/admin/atracciones', {
        params: { page, limit: LIMIT },
      })
      const data = response.data?.data || []
      const pagination = response.data?.pagination || {}
      todas = [...todas, ...data]
      totalPages = pagination.total_pages ?? pagination.totalPages ?? 1
      page++
    } while (page <= totalPages)

    return todas
  },
  obtenerAtraccionAdmin: async (guid) => {
    const response = await apiClient.get(`/admin/atracciones/${guid}`)
    return response.data?.data || response.data
  },
  createAtraccion: async (payload) => {
    const response = await apiClient.post('/admin/atracciones', payload)
    return response.data?.data || response.data
  },
  updateAtraccion: async (guid, payload) => {
    const response = await apiClient.put(`/admin/atracciones/${guid}`, payload)
    return response.data?.data || response.data
  },
  desactivarAtraccion: async (guid) => {
    const response = await apiClient.delete(`/admin/atracciones/${guid}`)
    return response.data
  },

  // ─── Destinos ─────────────────────────────────────────────────────────────
  listDestinos: async () => {
    const response = await apiClient.get('/admin/destinos')
    return response.data?.data || []
  },
  createDestino: async (payload) => {
    const response = await apiClient.post('/admin/destinos', payload)
    return response.data?.data || response.data
  },
  updateDestino: async (guid, payload) => {
    const response = await apiClient.put(`/admin/destinos/${guid}`, payload)
    return response.data?.data || response.data
  },
  deleteDestino: async (guid) => {
    const response = await apiClient.delete(`/admin/destinos/${guid}`)
    return response.data
  },

  // ─── Catálogos auxiliares ──────────────────────────────────────────────────
  listarCategorias: async () => {
    const response = await apiClient.get('/admin/categorias')
    return response.data?.data || []
  },
  createCategoria: async (payload) => {
    const response = await apiClient.post('/admin/categorias', payload)
    return response.data?.data || response.data
  },
  updateCategoria: async (guid, payload) => {
    const response = await apiClient.put(`/admin/categorias/${guid}`, payload)
    return response.data?.data || response.data
  },
  deleteCategoria: async (guid) => {
    const response = await apiClient.delete(`/admin/categorias/${guid}`)
    return response.data
  },

  listarIdiomas: async () => {
    const response = await apiClient.get('/admin/idiomas')
    return response.data?.data || []
  },
  createIdioma: async (payload) => {
    const response = await apiClient.post('/admin/idiomas', payload)
    return response.data?.data || response.data
  },
  updateIdioma: async (guid, payload) => {
    const response = await apiClient.put(`/admin/idiomas/${guid}`, payload)
    return response.data?.data || response.data
  },
  deleteIdioma: async (guid) => {
    const response = await apiClient.delete(`/admin/idiomas/${guid}`)
    return response.data
  },

  listarIncluye: async () => {
    const response = await apiClient.get('/admin/incluye')
    return response.data?.data || []
  },
  createIncluye: async (payload) => {
    const response = await apiClient.post('/admin/incluye', payload)
    return response.data?.data || response.data
  },
  updateIncluye: async (guid, payload) => {
    const response = await apiClient.put(`/admin/incluye/${guid}`, payload)
    return response.data?.data || response.data
  },
  deleteIncluye: async (guid) => {
    const response = await apiClient.delete(`/admin/incluye/${guid}`)
    return response.data
  },

  // ─── Tickets ──────────────────────────────────────────────────────────────
  listarTicketsAdmin: async () => {
    const response = await apiClient.get('/admin/tickets')
    return response.data?.data || []
  },
  obtenerTicketAdmin: async (guid) => {
    const response = await apiClient.get(`/admin/tickets/${guid}`)
    return response.data?.data || response.data
  },
  listarTicketsDeAtraccionAdmin: async (atGuid) => {
    if (!atGuid) return []
    const response = await apiClient.get(`/admin/atracciones/${atGuid}/tickets`)
    return response.data?.data || []
  },
  createTicket: async (payload) => {
    const response = await apiClient.post('/admin/tickets', payload)
    return response.data?.data || response.data
  },
  updateTicket: async (guid, payload) => {
    const response = await apiClient.put(`/admin/tickets/${guid}`, payload)
    return response.data?.data || response.data
  },
  eliminarTicket: async (guid) => {
    const response = await apiClient.delete(`/admin/tickets/${guid}`)
    return response.data
  },

  // ─── Horarios ─────────────────────────────────────────────────────────────
  // El listado se sirve en GET /admin/horarios (alias en TicketsController).
  listarHorariosAdmin: async () => {
    const response = await apiClient.get('/admin/horarios')
    return response.data?.data || []
  },
  obtenerHorarioAdmin: async (guid) => {
    const response = await apiClient.get(`/admin/horarios/${guid}`)
    return response.data?.data || response.data
  },
  listarHorariosDeTicket: async (tckGuid) => {
    if (!tckGuid) return []
    const response = await apiClient.get(`/admin/tickets/${tckGuid}/horarios`)
    return response.data?.data || []
  },
  listarHorariosDeAtraccion: async (atGuid) => {
    if (!atGuid) return []
    const response = await apiClient.get(`/admin/atracciones/${atGuid}/horarios`)
    return response.data?.data || []
  },
  createHorario: async (payload) => {
    // body: { tck_guid, fecha, hora_inicio, hora_fin?, cupos_disponibles }
    const response = await apiClient.post('/admin/tickets/horarios', payload)
    return response.data?.data || response.data
  },
  actualizarHorario: async (guid, payload) => {
    // body: { fecha?, hora_inicio?, hora_fin?, cupos_disponibles?, estado? }
    const response = await apiClient.put(`/admin/horarios/${guid}`, payload)
    return response.data?.data || response.data
  },
  eliminarHorario: async (guid) => {
    const response = await apiClient.delete(`/admin/horarios/${guid}`)
    return response.data
  },

  // ─── Reservas ─────────────────────────────────────────────────────────────
  /**
   * Devuelve el envelope crudo `{ data, pagination }` para que el hook
   * pueda calcular `totalPages` desde `pagination.total / pagination.limit`.
   */
  listarReservasAdmin: async (params = {}) => {
    const response = await apiClient.get('/admin/reservas', { params })
    return {
      data: response.data?.data || [],
      pagination: response.data?.pagination || null,
    }
  },
  obtenerReservaAdmin: async (guid) => {
    const response = await apiClient.get(`/admin/reservas/${guid}`)
    return response.data?.data || response.data
  },
  /**
   * Cambia el estado de una reserva. `nuevo_estado` es 'A' | 'I' | 'C'.
   */
  actualizarEstadoReserva: async (guid, nuevoEstado, motivo = '') => {
    const response = await apiClient.put(`/admin/reservas/${guid}/estado`, {
      nuevo_estado: nuevoEstado,
      motivo,
    })
    return response.data
  },
  cancelarReservaAdmin: async (guid, motivo = 'Cancelada desde administración.') => {
    const response = await apiClient.put(`/admin/reservas/${guid}/cancelar`, {
      nuevo_estado: 'C',
      motivo,
    })
    return response.data
  },
  anularReservaAdmin: async (guid, motivo = 'Anulada desde administración.') => {
    // axios DELETE acepta body via { data }
    const response = await apiClient.delete(`/admin/reservas/${guid}`, {
      data: { nuevo_estado: 'I', motivo },
    })
    return response.data
  },

  // ─── Clientes ─────────────────────────────────────────────────────────────
  listarClientesAdmin: async (params = {}) => {
    const response = await apiClient.get('/admin/clientes', { params })
    return response.data?.data || []
  },
  createCliente: async (payload) => {
    const response = await apiClient.post('/admin/clientes', payload)
    return response.data?.data || response.data
  },

  // ─── Usuarios (gestión de cuentas) ────────────────────────────────────────
  listarUsuariosAdmin: async (params = {}) => {
    const response = await apiClient.get('/admin/usuarios', { params })
    return {
      data: response.data?.data || [],
      pagination: response.data?.pagination || null,
    }
  },
  createUsuario: async (payload) => {
    const response = await apiClient.post('/admin/usuarios', payload)
    return response.data?.data || response.data
  },

  // ─── Reseñas ──────────────────────────────────────────────────────────────
  listarReseniasAdmin: async (atraccionGuid) => {
    const response = await apiClient.get('/admin/resenias', {
      params: { atraccionGuid },
    })
    return response.data?.data || []
  },
}
