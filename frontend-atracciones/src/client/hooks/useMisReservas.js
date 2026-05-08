import { useCallback, useRef, useState } from 'react'
import { listarMisReservas, obtenerReserva } from '../../api/reservasApi'

export function useMisReservas() {
  const [reservas, setReservas] = useState([])
  const [cargando, setCargando] = useState(false)
  const [error, setError] = useState('')
  // Guardamos la lista completa para poder filtrar localmente sin re-fetch
  const todasRef = useRef([])

  const cargarReservas = useCallback(async () => {
    setCargando(true)
    setError('')
    try {
      const data = await listarMisReservas()
      const lista = data?.data || (Array.isArray(data) ? data : [])
      todasRef.current = lista
      setReservas(lista)
      return data
    } catch (err) {
      setError(err?.response?.data?.message || 'No se pudieron cargar las reservas')
      throw err
    } finally {
      setCargando(false)
    }
  }, [])

  /**
   * Busca primero en la lista ya cargada comparando rev_codigo e ignorando
   * mayúsculas/minúsculas y espacios extra. Si no encuentra nada, intenta
   * con el backend usando el texto como guid.
   */
  const buscarReserva = useCallback(async (texto) => {
    if (!texto.trim()) {
      setReservas(todasRef.current)
      setError('')
      return
    }

    const normalizar = (s) => String(s ?? '').trim().toLowerCase().replace(/[\s-]+/g, '')
    const termino = normalizar(texto)

    // 1. Búsqueda local por rev_codigo (más probable que sea lo que ingresa el usuario)
    const encontradas = todasRef.current.filter((r) => {
      const codigo = normalizar(r.rev_codigo)
      const guid = normalizar(r.rev_guid)
      return codigo === termino || codigo.includes(termino) || guid === termino
    })

    if (encontradas.length > 0) {
      setReservas(encontradas)
      setError('')
      return
    }

    // 2. Si no se encontró localmente, intenta con el backend (el texto podría ser un GUID)
    setCargando(true)
    setError('')
    try {
      const data = await obtenerReserva(texto.trim())
      const reserva = data?.data ?? data
      setReservas(reserva ? [reserva] : [])
      if (!reserva) setError('No se encontró ninguna reserva con ese código')
    } catch (err) {
      if (err?.response?.status === 404) {
        setError('No se encontró ninguna reserva con ese código')
      } else {
        setError(err?.response?.data?.message || 'No se pudo buscar la reserva')
      }
      setReservas([])
    } finally {
      setCargando(false)
    }
  }, [])

  return { reservas, cargando, error, cargarReservas, buscarReserva }
}
