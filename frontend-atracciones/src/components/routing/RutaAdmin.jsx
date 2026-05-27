import { Navigate } from 'react-router-dom'
import { useAuthContext } from '../../context/AuthContext'

const ADMIN_ROLE_KEYS = new Set(['ADMIN', 'ADMINISTRADOR'])

function esRolAdmin(roles) {
  if (!Array.isArray(roles)) return false
  return roles.some((rol) => ADMIN_ROLE_KEYS.has(String(rol).trim().toUpperCase()))
}

function RutaAdmin({ children }) {
  const { estaAutenticado, usuario, hydrated } = useAuthContext()
  const esAdministrador = esRolAdmin(usuario?.roles)

  if (!hydrated) return null

  if (!estaAutenticado) {
    return <Navigate to="/login" replace />
  }

  if (!esAdministrador) {
    return <Navigate to="/" replace />
  }

  return children
}

export default RutaAdmin
