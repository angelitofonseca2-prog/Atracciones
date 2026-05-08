import { useState } from 'react'
import { adminApi } from '../../api/adminApi'
import { emitirToast } from '../../components/common/Toast'

const LIMIT = 10

export function useGestionAtracciones() {
  const [items, setItems] = useState([])
  const [cargando, setCargando] = useState(false)
  const [error, setError] = useState('')
  const [page, setPage] = useState(1)
  const [totalPages, setTotalPages] = useState(1)

  const cargar = async (p = 1) => {
    setCargando(true)
    setError('')
    try {
      const { data, pagination } = await adminApi.listarAtraccionesAdminConPaginacion({
        page: p, limit: LIMIT,
      })
      setItems(Array.isArray(data) ? data : [])
      setPage(p)
      const total = pagination?.total ?? data?.length ?? 0
      const limit = pagination?.limit ?? LIMIT
      setTotalPages(Math.max(1, Math.ceil(total / limit)))
    } catch (err) {
      setError(err?.response?.data?.message || 'No se pudieron cargar las atracciones.')
    } finally {
      setCargando(false)
    }
  }

  const guardar = async (payload, guid) => {
    if (guid) {
      await adminApi.updateAtraccion(guid, payload)
      emitirToast('Cambios guardados correctamente.', 'success')
    } else {
      await adminApi.createAtraccion(payload)
      emitirToast('Registro creado correctamente.', 'success')
    }
    await cargar(page)
  }

  const desactivar = async (guid) => {
    try {
      await adminApi.desactivarAtraccion(guid)
      emitirToast('Atracción desactivada correctamente.', 'success')
      await cargar(page)
    } catch (err) {
      emitirToast(
        err?.response?.data?.message || 'No se pudo desactivar la atracción.',
        'error',
      )
      throw err
    }
  }

  return { items, cargando, error, page, totalPages, cargar, guardar, desactivar }
}
