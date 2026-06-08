import { useCallback, useEffect, useRef, useState } from 'react'
import { obtenerHorariosDisponibles } from '../../api/atraccionesApi'
import { graphqlObtenerHorarios } from '../../graphql/marketplaceApi'
import { useGraphqlEnabled } from '../../config/graphqlUrl'

const POLL_INTERVAL_MS = 30_000 // Refrescar cupos cada 30 segundos

/**
 * Devuelve la lista de horarios disponibles y la refresca automáticamente
 * cada POLL_INTERVAL_MS mientras la página esté visible.
 * Garantiza que el usuario vea cupos actualizados aunque Booking externo
 * haya consumido plazas mientras estaba en la pantalla de selección.
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
        const raw = graphqlOn
          ? await graphqlObtenerHorarios(atGuid, true)
          : await obtenerHorariosDisponibles(atGuid)
        // Normalizar campos: la API REST devuelve `cupos` y sin `fecha_fin`;
        // el resto del código usa `hor_cupos_disponibles` y `fecha_fin`.
        const data = (Array.isArray(raw) ? raw : []).map((h) => ({
          ...h,
          hor_cupos_disponibles: h.hor_cupos_disponibles ?? h.cupos_disponibles ?? h.cupos,
          fecha_fin: h.fecha_fin ?? h.fecha, // si no hay rango, inicio = fin
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

    // Polling silencioso — solo actualiza si la pestaña está activa
    const startPolling = () => {
      timerRef.current = setInterval(() => {
        if (!document.hidden) cargar(true)
      }, POLL_INTERVAL_MS)
    }

    startPolling()
    return () => clearInterval(timerRef.current)
  }, [cargar])

  // Refrescar también cuando el usuario vuelve a la pestaña
  useEffect(() => {
    const onVisible = () => {
      if (!document.hidden) cargar(true)
    }
    document.addEventListener('visibilitychange', onVisible)
    return () => document.removeEventListener('visibilitychange', onVisible)
  }, [cargar])

  return { horarios, cargando, error, refrescar: () => cargar(false) }
}
