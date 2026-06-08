import { useCallback, useEffect, useState } from 'react'
import {
  listarAtracciones,
  obtenerAtraccion,
  obtenerFiltros,
} from '../../api/atraccionesApi'
import { useGraphqlEnabled } from '../../config/graphqlUrl'
import {
  graphqlListarAtracciones,
  graphqlObtenerAtraccion,
  graphqlObtenerFiltros,
} from '../../graphql/marketplaceApi'

export function useAtracciones(filtrosActivos = {}) {
  const graphqlOn = useGraphqlEnabled()
  const ciudad = filtrosActivos?.ciudad || undefined
  const tipo = filtrosActivos?.tipo || undefined
  const subtipo = filtrosActivos?.subtipo || undefined
  const idioma = filtrosActivos?.idioma || undefined
  const calificacionMin =
    filtrosActivos?.calificacion_min != null && filtrosActivos?.calificacion_min !== ''
      ? Number(filtrosActivos.calificacion_min)
      : undefined
  const disponible =
    typeof filtrosActivos?.disponible === 'boolean'
      ? filtrosActivos.disponible
      : undefined
  const ordenarPor = filtrosActivos?.ordenar_por || undefined
  const page = filtrosActivos?.page || 1
  const limit = filtrosActivos?.limit || 8

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
        Ciudad: ciudad,
        Tipo: tipo,
        Subtipo: subtipo,
        Idioma: idioma,
        CalificacionMin: calificacionMin,
        Disponible: disponible,
        OrdenarPor: ordenarPor,
        Page: page,
        Limit: limit,
      }
      const data = graphqlOn
        ? await graphqlListarAtracciones(params)
        : await listarAtracciones(params)
      setAtracciones(data.data || [])
      const pagination = data.pagination || {}
      setPaginacion({
        page: pagination.page || page,
        limit: pagination.limit || limit,
        total: pagination.total || 0,
        totalPages: pagination.total_pages ?? pagination.totalPages ?? 1,
      })
    } catch (err) {
      setError(err?.response?.data?.message || err?.message || 'No se pudo cargar el catálogo')
    } finally {
      setCargando(false)
    }
  }, [graphqlOn, ciudad, tipo, subtipo, idioma, calificacionMin, disponible, ordenarPor, page, limit])

  useEffect(() => {
    cargarAtracciones()
  }, [cargarAtracciones])

  useEffect(() => {
    const load = graphqlOn
      ? () => graphqlObtenerFiltros().then((raw) => setFiltrosDisponibles(raw ?? {}))
      : () => obtenerFiltros().then((raw) => setFiltrosDisponibles(raw ?? {}))
    load().catch(() => setFiltrosDisponibles({}))
  }, [graphqlOn])

  const cargarDetalle = useCallback(async (guid) => {
    setCargando(true)
    setError('')
    try {
      const data = graphqlOn
        ? await graphqlObtenerAtraccion(guid)
        : await obtenerAtraccion(guid)
      const atraccion = data?.data || null
      setDetalle(atraccion)
      return atraccion
    } catch (err) {
      if (err?.response?.status === 404) {
        setError('Atracción no encontrada')
      } else {
        setError(err?.response?.data?.message || err?.message || 'No se pudo cargar el detalle')
      }
      setDetalle(null)
      throw err
    } finally {
      setCargando(false)
    }
  }, [graphqlOn])

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
