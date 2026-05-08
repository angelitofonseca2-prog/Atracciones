import { Navigate } from 'react-router-dom'
import { useAuthContext } from '../../context/AuthContext'

function RutaAdmin({ children }) {
  const { estaAutenticado, usuario } = useAuthContext()
  const esAdministrador = usuario?.roles?.includes('ADMIN')

  if (!estaAutenticado || !esAdministrador) {
    return <Navigate to="/" replace />
  }

  return children
}

export default RutaAdmin
