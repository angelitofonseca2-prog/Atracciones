import { useLocation } from 'react-router-dom'
import { getFallbackRoute } from '../../utils/navigationBack'
import BotonVolver from '../common/BotonVolver'

function PageBackNav() {
  const { pathname } = useLocation()

  if (pathname === '/') {
    return null
  }

  return (
    <nav className="page-back-nav" aria-label="Navegación secundaria">
      <BotonVolver fallback={getFallbackRoute(pathname)} />
    </nav>
  )
}

export default PageBackNav
