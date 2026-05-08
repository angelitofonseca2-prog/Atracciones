import { useEffect, useState } from 'react'
import { adminApi } from '../../api/adminApi'
import { emitirToast } from '../../components/common/Toast'

export function useGestionTickets() {
  const [items, setItems] = useState([])
  const [cargando, setCargando] = useState(false)
  const [error, setError] = useState('')

  const cargar = async () => {
    setCargando(true)
    setError('')
    try {
      const data = await adminApi.listarTicketsAdmin()
      setItems(Array.isArray(data) ? data : [])
    } catch (err) {
      setError(err?.response?.data?.message || 'No se pudieron cargar los tickets.')
    } finally {
      setCargando(false)
    }
  }

  useEffect(() => { cargar() }, [])

  const guardar = async (payload, guid) => {
    if (guid) {
      await adminApi.updateTicket(guid, payload)
      emitirToast('Cambios guardados correctamente.', 'success')
    } else {
      await adminApi.createTicket(payload)
      emitirToast('Registro creado correctamente.', 'success')
    }
    await cargar()
  }

  const eliminar = async (guid) => {
    try {
      await adminApi.eliminarTicket(guid)
      emitirToast('Registro eliminado correctamente.', 'success')
      await cargar()
    } catch (err) {
      emitirToast(
        err?.response?.data?.message || 'No se pudo eliminar el ticket.',
        'error',
      )
      throw err
    }
  }

  return { items, cargando, error, guardar, eliminar, recargar: cargar }
}
