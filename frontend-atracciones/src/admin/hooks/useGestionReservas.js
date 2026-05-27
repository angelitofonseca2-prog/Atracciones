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
      const status = err?.response?.status
      // Fallback defensivo: en algunos entornos existen filas legacy que rompen
      // el listado global. Reintentamos por estados válidos y fusionamos.
      if (status >= 500) {
        try {
          const estados = ['P', 'A', 'C', 'I']
          const respuestas = await Promise.all(
            estados.map((estado) =>
              adminApi.listarReservasAdmin({ page: p, limit: LIMIT, estado }),
            ),
          )
          const merged = []
          const seen = new Set()
          for (const r of respuestas) {
            for (const item of r?.data ?? []) {
              const id = item?.rev_guid ?? JSON.stringify(item)
              if (!seen.has(id)) {
                seen.add(id)
                merged.push(item)
              }
            }
          }
          setItems(merged)
          setPage(p)
          setTotalPages(Math.max(1, p))
          setError('')
          return
        } catch {
          // Si también falla el fallback, mostramos el error original.
        }
      }
      setError(err?.response?.data?.message || 'No se pudo cargar las reservas.')
    } finally {
      setCargando(false)
    }
  }

  return { items, cargando, error, page, totalPages, cargar }
}
