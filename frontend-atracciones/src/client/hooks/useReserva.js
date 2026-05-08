import { useState } from 'react'
import * as reservasApi from '../../api/reservasApi'
import { emitirToast } from '../../components/common/Toast'

/**
 * Hook que crea reservas alineadas con `CrearReservaRequest` (snake_case):
 *   {
 *     at_guid, hor_guid,
 *     lineas: [{ tck_guid, cantidad }],
 *     origen_canal?: 'web' | 'app',
 *     cliente_invitado?: { tipo_identificacion, numero_identificacion,
 *                          correo, telefono?, nombres?, apellidos?, ... }
 *   }
 */
export function useReserva() {
  const [cargando, setCargando] = useState(false)
  const [error, setError] = useState('')
  const [reservaCreada, setReservaCreada] = useState(null)

  const crearReserva = async (atGuid, horGuid, lineas, origenCanal = 'web', clienteInvitado = null) => {
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
    if (clienteInvitado) {
      body.cliente_invitado = clienteInvitado
    }

    try {
      const response = await reservasApi.crearReserva(body)
      const reserva = response?.data ?? response
      setReservaCreada(reserva)
      emitirToast('Reserva creada correctamente.', 'success')
      return reserva
    } catch (err) {
      let mensaje
      if (err?.response?.status === 409) {
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
