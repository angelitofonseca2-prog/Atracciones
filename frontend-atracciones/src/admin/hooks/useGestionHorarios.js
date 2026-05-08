import { useEffect, useState } from 'react'
import { adminApi } from '../../api/adminApi'
import { emitirToast } from '../../components/common/Toast'

export function useGestionHorarios() {
  const [items, setItems] = useState([])
  const [cargando, setCargando] = useState(false)
  const [error, setError] = useState('')

  const cargar = async () => {
    setCargando(true)
    setError('')
    try {
      const data = await adminApi.listarHorariosAdmin()
      setItems(Array.isArray(data) ? data : [])
    } catch (err) {
      setError(err?.response?.data?.message || 'No se pudieron cargar los horarios.')
    } finally {
      setCargando(false)
    }
  }

  useEffect(() => { cargar() }, [])

  const crear = async (payload) => {
    await adminApi.createHorario(payload)
    emitirToast('Horario creado correctamente.', 'success')
    await cargar()
  }

  const actualizar = async (guid, payload) => {
    await adminApi.actualizarHorario(guid, payload)
    emitirToast('Horario actualizado correctamente.', 'success')
    await cargar()
  }

  const eliminar = async (guid) => {
    try {
      await adminApi.eliminarHorario(guid)
      emitirToast('Horario eliminado correctamente.', 'success')
      await cargar()
    } catch (err) {
      emitirToast(
        err?.response?.data?.message || 'No se pudo eliminar el horario.',
        'error',
      )
      throw err
    }
  }

  return { items, cargando, error, crear, actualizar, eliminar, recargar: cargar }
}
