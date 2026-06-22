import { useEffect, useRef, useState } from 'react'
import { apolloClient, SUBSCRIPTIONS, QUERIES } from '../../graphql/client'
import { useGraphqlEnabled } from '../../config/graphqlUrl'

const POLL_INTERVALO_MS = 2000
const MAX_INTENTOS = 30

/**
 * Espera la confirmación de una reserva identificada por `seguimientoId`.
 *
 * Estrategia:
 *  1. Si GraphQL + WebSocket disponibles → usa subscription (tiempo real).
 *  2. Si falla WS o GraphQL desactivado → polling REST cada 2s.
 *
 * Devuelve { estado, revGuid, revCodigo, error, esperando }.
 */
export function useEsperarEstadoReserva(seguimientoId) {
  const graphqlOn = useGraphqlEnabled()
  const [estado, setEstado] = useState(null)
  const [error, setError] = useState('')
  const [esperando, setEsperando] = useState(false)
  const subRef = useRef(null)
  const timerRef = useRef(null)
  const intentosRef = useRef(0)

  useEffect(() => {
    if (!seguimientoId) return

    setEsperando(true)
    setEstado(null)
    setError('')
    intentosRef.current = 0

    if (graphqlOn) {
      // Intentar subscription WS primero.
      let wsOk = false

      subRef.current = apolloClient
        .subscribe({
          query: SUBSCRIPTIONS.ESTADO_RESERVA,
          variables: { seguimientoId: String(seguimientoId) },
        })
        .subscribe({
          next({ data }) {
            wsOk = true
            const payload = data?.onEstadoReservaActualizado
            if (!payload) return
            if (payload.estado === 'CONFIRMADA' || payload.estado === 'RECHAZADA') {
              setEstado(payload)
              setEsperando(false)
              if (payload.estado === 'RECHAZADA') {
                setError(payload.motivoRechazo || 'La reserva fue rechazada.')
              }
              subRef.current?.unsubscribe()
            }
          },
          error(err) {
            // eslint-disable-next-line no-console
            console.warn('[Subscription] error, activando polling como fallback →', err?.message ?? err)
            if (!wsOk) activarPolling()
          },
        })

      // Si tras 5s no hay respuesta WS, activa polling como seguro.
      const wsTimeout = setTimeout(() => {
        if (!wsOk && esperando) {
          // eslint-disable-next-line no-console
          console.warn('[Subscription] sin respuesta en 5s, activando polling de seguridad')
          activarPolling()
        }
      }, 5000)

      return () => {
        clearTimeout(wsTimeout)
        subRef.current?.unsubscribe()
        clearInterval(timerRef.current)
      }
    } else {
      activarPolling()
      return () => clearInterval(timerRef.current)
    }

    function activarPolling() {
      clearInterval(timerRef.current)
      timerRef.current = setInterval(async () => {
        if (intentosRef.current >= MAX_INTENTOS) {
          clearInterval(timerRef.current)
          setError('Tiempo de espera agotado. La reserva sigue en proceso.')
          setEsperando(false)
          return
        }
        intentosRef.current += 1
        try {
          const { data } = await apolloClient.query({
            query: QUERIES.ESTADO_RESERVA,
            variables: { seguimientoId: String(seguimientoId) },
            fetchPolicy: 'network-only',
          })
          const payload = data?.estadoReserva
          if (!payload) return
          if (payload.estado === 'CONFIRMADA' || payload.estado === 'RECHAZADA') {
            clearInterval(timerRef.current)
            setEstado(payload)
            setEsperando(false)
            if (payload.estado === 'RECHAZADA') {
              setError(payload.motivoRechazo || 'La reserva fue rechazada.')
            }
          }
        } catch {
          // silencioso; se reintentará en el siguiente tick
        }
      }, POLL_INTERVALO_MS)
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [seguimientoId, graphqlOn])

  return { estado, error, esperando }
}
