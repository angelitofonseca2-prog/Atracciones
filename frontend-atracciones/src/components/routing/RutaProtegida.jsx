import { Navigate, useLocation } from 'react-router-dom'
import { useAuthContext } from '../../context/AuthContext'

function RutaProtegida({ children }) {
  const { estaAutenticado } = useAuthContext()
  const location = useLocation()

  if (!estaAutenticado) {
    return <Navigate to="/login" state={{ from: location }} replace />
  }

  return children
}

export default RutaProtegida
