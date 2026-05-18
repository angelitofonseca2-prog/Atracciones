import { useNavigate } from 'react-router-dom'
import { getFallbackRoute, puedeVolverEnHistorial } from '../../utils/navigationBack'

function BotonVolver({
  fallback,
  etiqueta = 'Volver',
  className = '',
}) {
  const navigate = useNavigate()

  const handleClick = () => {
    if (puedeVolverEnHistorial()) {
      navigate(-1)
      return
    }
    const destino = fallback ?? getFallbackRoute(window.location.pathname)
    navigate(destino)
  }

  return (
    <button
      type="button"
      className={`btn-volver ${className}`.trim()}
      onClick={handleClick}
      aria-label={etiqueta}
    >
      <span className="btn-volver__icon" aria-hidden="true">←</span>
      {etiqueta}
    </button>
  )
}

export default BotonVolver
