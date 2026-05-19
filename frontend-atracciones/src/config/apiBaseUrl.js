/** Gateway público en Railway (build sin VITE_API_URL). */
const DEFAULT_PRODUCTION_API =
  'https://api-gateway-production-5c80b.up.railway.app/api/v2'

const DEFAULT_LOCAL_API = 'http://localhost:5050/api/v2'

/**
 * Base URL del API (sin barra final). Normaliza v1 → v2 por despliegues antiguos.
 */
export function getApiBaseUrl() {
  let url = import.meta.env.VITE_API_URL

  if (!url) {
    url = import.meta.env.PROD ? DEFAULT_PRODUCTION_API : DEFAULT_LOCAL_API
  }

  if (typeof url === 'string' && url.includes('/api/v1')) {
    url = url.replace(/\/api\/v1\/?$/, '/api/v2')
  }

  return url.replace(/\/$/, '')
}
