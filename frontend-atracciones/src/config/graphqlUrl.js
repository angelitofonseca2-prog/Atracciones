const DEFAULT_GRAPHQL_URL = 'http://localhost:5200/graphql'

export function getGraphqlUrl() {
  const fromEnv = import.meta.env.VITE_GRAPHQL_URL
  if (fromEnv && String(fromEnv).trim()) {
    return String(fromEnv).trim().replace(/\/+$/, '')
  }
  return DEFAULT_GRAPHQL_URL
}

/** URL WebSocket para GraphQL subscriptions (ws:// o wss://). */
export function getGraphqlWsUrl() {
  const http = getGraphqlUrl()
  // Convierte http → ws y https → wss automáticamente.
  return http.replace(/^http/, 'ws')
}

export function useGraphqlEnabled() {
  const flag = import.meta.env.VITE_USE_GRAPHQL
  return flag === 'true' || flag === true
}
