import { useState } from 'react'
import { adminApi } from '../../api/adminApi'

const LIMIT = 10

export function useGestionReservas() {
  const [items, setItems] = useState([])
  const [cargando, setCargando] = useState(false)
  const [error, setError] = useState('')
  const [page, setPage] = useState(1)
  const [totalPages, setTotalPages] = useState(1)

  const cargar = async (p = 1) => {
    setCargando(true)
    setError('')
    try {
      const { data, pagination } = await adminApi.listarReservasAdmin({ page: p, limit: LIMIT })
      setItems(Array.isArray(data) ? data : [])
      setPage(p)
      const total = pagination?.total ?? data?.length ?? 0
      const limit = pagination?.limit ?? LIMIT
      setTotalPages(Math.max(1, Math.ceil(total / limit)))
    } catch (err) {
      setError(err?.response?.data?.message || 'No se pudo cargar las reservas.')
    } finally {
      setCargando(false)
    }
  }

  return { items, cargando, error, page, totalPages, cargar }
}
