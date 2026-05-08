import { useState } from 'react'
import * as authApi from '../../api/authApi'
import { useAuthContext } from '../../context/AuthContext'

export function useAuth() {
  const [cargando, setCargando] = useState(false)
  const [error, setError] = useState('')
  const { login } = useAuthContext()

  const iniciarSesion = async (loginValue, password) => {
    setCargando(true)
    setError('')
    try {
      const data = await authApi.login(loginValue, password)
      login(data.data.token, {
        login: data.data.login,
        roles: data.data.roles || [],
      })
      return data
    } catch (err) {
      const mensaje =
        err?.response?.data?.details?.[0] ||
        err?.response?.data?.message ||
        'No fue posible iniciar sesion'
      setError(mensaje)
      throw err
    } finally {
      setCargando(false)
    }
  }

  return { cargando, error, iniciarSesion }
}
