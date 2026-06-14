import { useCallback, useEffect, useRef, useState } from 'react'
import { obtenerHorariosDisponibles } from '../../api/atraccionesApi'
import { graphqlObtenerHorarios } from '../../graphql/marketplaceApi'
import { useGraphqlEnabled } from '../../config/graphqlUrl'

const POLL_INTERVAL_MS = 30_000

/**
 * Devuelve la lista de horarios disponibles y la refresca automáticamente
 * cada POLL_INTERVAL_MS mientras la página esté visible.
 * Si GraphQL no está disponible, cae automáticamente a REST.
 */
export function useHorariosConPolling(atGuid) {
  const graphqlOn = useGraphqlEnabled()
  const [horarios, setHorarios] = useState([])
  const [cargando, setCargando] = useState(false)
  const [error, setError] = useState('')
  const timerRef = useRef(null)

  const cargar = useCallback(
    async (silent = false) => {
      if (!atGuid) return
      if (!silent) setCargando(true)
      setError('')
      try {
        let raw
        if (graphqlOn) {
          try {
            raw = await graphqlObtenerHorarios(atGuid, true)
          } catch {
            // GraphQL no disponible → fallback REST
            const data = await obtenerHorariosDisponibles(atGuid)
            raw = data?.data ?? data ?? []
          }
        } else {
          const data = await obtenerHorariosDisponibles(atGuid)
          raw = data?.data ?? data ?? []
        }

        const data = (Array.isArray(raw) ? raw : []).map((h) => ({
          ...h,
          hor_cupos_disponibles: h.hor_cupos_disponibles ?? h.cupos_disponibles ?? h.cupos,
          fecha_fin: h.fecha_fin ?? h.fecha,
        }))
        setHorarios(data)
      } catch {
        if (!silent) setError('No se pudieron cargar los horarios.')
      } finally {
        if (!silent) setCargando(false)
      }
    },
    [atGuid, graphqlOn],
  )

  useEffect(() => {
    cargar(false)

    const startPolling = () => {
      timerRef.current = setInterval(() => {
        if (!document.hidden) cargar(true)
      }, POLL_INTERVAL_MS)
    }

    startPolling()
    return () => clearInterval(timerRef.current)
  }, [cargar])

  useEffect(() => {
    const onVisible = () => {
      if (!document.hidden) cargar(true)
    }
    document.addEventListener('visibilitychange', onVisible)
    return () => document.removeEventListener('visibilitychange', onVisible)
  }, [cargar])

  return { horarios, cargando, error, refrescar: () => cargar(false) }
}
