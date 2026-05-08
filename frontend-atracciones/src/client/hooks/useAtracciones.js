import { useCallback, useEffect, useState } from 'react'
import {
  listarAtracciones,
  obtenerAtraccion,
  obtenerFiltros,
} from '../../api/atraccionesApi'

export function useAtracciones(filtrosActivos = {}) {
  const [atracciones, setAtracciones] = useState([])
  const [paginacion, setPaginacion] = useState({
    page: 1,
    limit: 8,
    total: 0,
    totalPages: 1,
  })
  const [filtrosDisponibles, setFiltrosDisponibles] = useState({})
  const [detalle, setDetalle] = useState(null)
  const [cargando, setCargando] = useState(false)
  const [error, setError] = useState('')

  const cargarAtracciones = useCallback(async () => {
    setCargando(true)
    setError('')
    try {
      const params = {
        Ciudad:          filtrosActivos.ciudad           || undefined,
        Tipo:            filtrosActivos.tipo             || undefined,
        Subtipo:         filtrosActivos.subtipo          || undefined,
        Idioma:          filtrosActivos.idioma           || undefined,
        CalificacionMin: filtrosActivos.calificacion_min != null && filtrosActivos.calificacion_min !== ''
          ? Number(filtrosActivos.calificacion_min)
          : undefined,
        Disponible:
          typeof filtrosActivos.disponible === 'boolean'
            ? filtrosActivos.disponible
            : undefined,
        OrdenarPor:      filtrosActivos.ordenar_por      || undefined,
        Page:            filtrosActivos.page             || 1,
        Limit:           filtrosActivos.limit            || 8,
      }
      const data = await listarAtracciones(params)
      setAtracciones(data.data || [])
      const pagination = data.pagination || {}
      setPaginacion({
        page: pagination.page || filtrosActivos.page || 1,
        limit: pagination.limit || filtrosActivos.limit || 8,
        total: pagination.total || 0,
        // El backend puede devolver total_pages (snake_case) o totalPages (camelCase)
        totalPages: pagination.total_pages ?? pagination.totalPages ?? 1,
      })
    } catch (err) {
      setError(err?.response?.data?.message || 'No se pudo cargar el catálogo')
    } finally {
      setCargando(false)
    }
  }, [filtrosActivos])

  useEffect(() => {
    cargarAtracciones()
  }, [cargarAtracciones])

  // Carga filtros al montar; el endpoint no requiere parámetros.
  useEffect(() => {
    obtenerFiltros()
      .then((raw) => setFiltrosDisponibles(raw ?? {}))
      .catch(() => setFiltrosDisponibles({}))
  }, [])

  const cargarDetalle = useCallback(async (guid) => {
    setCargando(true)
    setError('')
    try {
      const data = await obtenerAtraccion(guid)
      const atraccion = data?.data || null
      setDetalle(atraccion)
      return atraccion
    } catch (err) {
      if (err?.response?.status === 404) {
        setError('Atracción no encontrada')
      } else {
        setError(err?.response?.data?.message || 'No se pudo cargar el detalle')
      }
      setDetalle(null)
      throw err
    } finally {
      setCargando(false)
    }
  }, [])

  const cambiarPagina = (nuevaPagina) => {
    setPaginacion((prev) => ({ ...prev, page: nuevaPagina }))
  }

  return {
    atracciones,
    paginacion,
    filtrosDisponibles,
    detalle,
    cargando,
    error,
    cambiarPagina,
    cargarDetalle,
  }
}
