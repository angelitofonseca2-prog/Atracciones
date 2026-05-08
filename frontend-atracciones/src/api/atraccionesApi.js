import axios from 'axios'

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
})

apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    const status = error?.response?.status

    if (status === 401) {
      // Solo limpiar sesión y redirigir si el token ya existía (no en un intento de login)
      const hayToken = Boolean(localStorage.getItem('token'))
      localStorage.removeItem('token')
      localStorage.removeItem('usuario')
      if (hayToken && window.location.pathname !== '/login') {
        window.location.href = '/login'
      }
    } else if (status === 403) {
      // Notificar sin redirigir — cada componente maneja sus propios 403
      // (ej: registro puede obtener 403 en /admin/clientes con token CLIENTE)
      window.dispatchEvent(
        new CustomEvent('app:toast', {
          detail: {
            id: Date.now(),
            message: 'No tienes permisos para esta acción',
            type: 'error',
          },
        }),
      )
    } else if (status >= 500) {
      window.dispatchEvent(
        new CustomEvent('app:toast', {
          detail: {
            id: Date.now(),
            message: 'Error del servidor. Intenta nuevamente.',
            type: 'error',
          },
        }),
      )
    }
    // 409 y otros se manejan en cada hook
    return Promise.reject(error)
  },
)

export const listarAtracciones = async (params = {}) => {
  const response = await apiClient.get('/atracciones', { params })
  return response.data
}

export const obtenerFiltros = async () => {
  const response = await apiClient.get('/atracciones/filtros')
  return response.data?.data ?? response.data ?? {}
}

export const obtenerAtraccion = async (guid) => {
  const response = await apiClient.get(`/atracciones/${guid}`)
  return response.data
}

export const obtenerTicketsAtraccion = async (guid) => {
  const response = await apiClient.get(`/atracciones/${guid}/tickets`)
  return response.data.data
}

export const obtenerHorariosDisponibles = async (guid) => {
  const response = await apiClient.get(`/atracciones/${guid}/horarios-disponibles`)
  return response.data.data
}
