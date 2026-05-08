import { apiClient } from './atraccionesApi'

/**
 * POST /api/v1/auth/login
 * Body: { login, password } — `login` debe ser el correo del usuario.
 */
export const login = async (loginValue, password) => {
  const response = await apiClient.post('/auth/login', {
    login: loginValue,
    password,
  })
  return response.data
}

/**
 * POST /api/v1/auth/registro
 * Body completo del contrato `RegistroClienteRequest`:
 *   { login, password, tipo_identificacion, numero_identificacion,
 *     nombres, apellidos, correo, telefono? }
 *
 * El backend ya crea el cliente y devuelve el token: NO debe llamarse
 * adicionalmente a /admin/clientes desde el flujo público.
 */
export const registro = async (payload) => {
  const body = {
    login: payload.login,
    password: payload.password,
    tipo_identificacion: payload.tipo_identificacion,
    numero_identificacion: payload.numero_identificacion,
    nombres: payload.nombres,
    apellidos: payload.apellidos,
    correo: payload.correo,
  }
  if (payload.telefono) body.telefono = payload.telefono
  const response = await apiClient.post('/auth/registro', body)
  return response.data
}
