import { useCallback, useState } from 'react'
import { listarAtracciones } from '../../api/atraccionesApi'

export function useHomeDestacadas() {
  const [destacadas, setDestacadas] = useState([])
  const [cargando, setCargando] = useState(false)
  const [error, setError] = useState('')

  const cargarDestacadas = useCallback(async () => {
    setCargando(true)
    setError('')
    try {
      const data = await listarAtracciones({ page: 1, limit: 6 })
      setDestacadas(data.data || [])
    } catch (err) {
      setError(err?.response?.data?.message || 'No se pudieron cargar las destacadas')
    } finally {
      setCargando(false)
    }
  }, [])

  return { destacadas, cargando, error, cargarDestacadas }
}
