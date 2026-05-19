import { useState } from 'react'
import * as reservasApi from '../../api/reservasApi'
import { emitirToast } from '../../components/common/Toast'

/**
 * Hook que crea reservas (requiere sesión activa):
 *   { at_guid, hor_guid, lineas: [{ tck_guid, cantidad }], origen_canal? }
 */
export function useReserva() {
  const [cargando, setCargando] = useState(false)
  const [error, setError] = useState('')
  const [reservaCreada, setReservaCreada] = useState(null)

  const crearReserva = async (atGuid, horGuid, lineas, origenCanal = 'web', fechaVisita = undefined) => {
    setCargando(true)
    setError('')

    const body = {
      at_guid: atGuid,
      hor_guid: horGuid,
      lineas: lineas.map((item) => ({
        tck_guid: item.tck_guid,
        cantidad: Number(item.cantidad),
      })),
      origen_canal: origenCanal,
    }
    if (fechaVisita) {
      body.fecha_visita = fechaVisita
    }

    try {
      const response = await reservasApi.crearReserva(body)
      const reserva = response?.data ?? response
      setReservaCreada(reserva)
      emitirToast('Reserva creada correctamente.', 'success')
      return reserva
    } catch (err) {
      let mensaje
      if (err?.response?.status === 401) {
        mensaje = 'Debes iniciar sesión para reservar.'
      } else if (err?.response?.status === 409) {
        mensaje = 'No hay cupos suficientes para el horario seleccionado.'
      } else if (err?.response?.status === 400) {
        mensaje =
          err?.response?.data?.details?.[0] ||
          err?.response?.data?.message ||
          'Datos de reserva inválidos. Revisa los campos y vuelve a intentarlo.'
      } else {
        mensaje =
          err?.response?.data?.message ||
          err?.response?.data?.details?.[0] ||
          'No se pudo crear la reserva. Verifica los datos ingresados.'
      }
      setError(mensaje)
      throw err
    } finally {
      setCargando(false)
    }
  }

  return { cargando, error, reservaCreada, crearReserva }
}
