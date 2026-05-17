import { useCallback, useState } from 'react'
import { obtenerPerfilCliente } from '../../api/clientesApi'

/** Mapea el perfil CRM al formato de formularios de reserva / pago. */
export function mapPerfilAFormulario(perfil) {
  if (!perfil) return null
  return {
    tipo_identificacion: perfil.tipo_identificacion ?? '',
    numero_identificacion: perfil.numero_identificacion ?? '',
    nombres: perfil.nombres ?? '',
    apellidos: perfil.apellidos ?? '',
    correo: perfil.correo ?? '',
    telefono: perfil.telefono ?? '',
  }
}

export function mapPerfilAPago(perfil) {
  if (!perfil) return null
  return {
    nombre_receptor: perfil.nombres ?? '',
    apellido_receptor: perfil.apellidos ?? '',
    correo_receptor: perfil.correo ?? '',
    telefono_receptor: perfil.telefono ?? '',
    observacion: '',
  }
}

export function usePerfilCliente() {
  const [perfil, setPerfil] = useState(null)
  const [cargando, setCargando] = useState(false)
  const [error, setError] = useState('')

  const cargarPerfil = useCallback(async () => {
    setCargando(true)
    setError('')
    try {
      const data = await obtenerPerfilCliente()
      setPerfil(data)
      return data
    } catch (err) {
      const msg =
        err?.response?.data?.message ||
        err?.response?.data?.details?.[0] ||
        'No se pudo cargar tu perfil.'
      setError(msg)
      throw err
    } finally {
      setCargando(false)
    }
  }, [])

  return { perfil, cargando, error, cargarPerfil }
}
