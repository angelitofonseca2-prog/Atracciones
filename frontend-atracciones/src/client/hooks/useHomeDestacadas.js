import { useCallback, useState } from 'react'
import { listarAtracciones } from '../../api/atraccionesApi'
import { useGraphqlEnabled } from '../../config/graphqlUrl'
import { graphqlListarAtracciones } from '../../graphql/marketplaceApi'

export function useHomeDestacadas() {
  const graphqlOn = useGraphqlEnabled()
  const [destacadas, setDestacadas] = useState([])
  const [cargando, setCargando] = useState(false)
  const [error, setError] = useState('')

  const cargarDestacadas = useCallback(async () => {
    setCargando(true)
    setError('')
    try {
      const data = graphqlOn
        ? await graphqlListarAtracciones({ page: 1, limit: 6 })
        : await listarAtracciones({ page: 1, limit: 6 })
      setDestacadas(data.data || [])
    } catch (err) {
      setError(err?.response?.data?.message || err?.message || 'No se pudieron cargar las destacadas')
    } finally {
      setCargando(false)
    }
  }, [graphqlOn])

  return { destacadas, cargando, error, cargarDestacadas }
}
