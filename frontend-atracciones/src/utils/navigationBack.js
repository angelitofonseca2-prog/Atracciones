/**
 * Ruta de respaldo cuando no hay historial de navegación en la app.
 */
export function getFallbackRoute(pathname) {
  if (pathname.startsWith('/admin/') && pathname !== '/admin') {
    return '/admin'
  }

  const reservarMatch = pathname.match(/^\/reservar\/([^/]+)/)
  if (reservarMatch) {
    return `/atracciones/${reservarMatch[1]}`
  }

  const detalleMatch = pathname.match(/^\/atracciones\/([^/]+)/)
  if (detalleMatch) {
    return '/atracciones'
  }

  if (pathname === '/login' || pathname === '/registro') {
    return '/'
  }

  return '/'
}

/** true si React Router puede hacer navigate(-1) dentro de la sesión. */
export function puedeVolverEnHistorial() {
  const idx = window.history.state?.idx
  return typeof idx === 'number' && idx > 0
}
