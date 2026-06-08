const DEFAULT_GRAPHQL_URL = 'http://localhost:5200/graphql'

export function getGraphqlUrl() {
  const fromEnv = import.meta.env.VITE_GRAPHQL_URL
  if (fromEnv && String(fromEnv).trim()) {
    return String(fromEnv).trim().replace(/\/+$/, '')
  }
  return DEFAULT_GRAPHQL_URL
}

export function useGraphqlEnabled() {
  const flag = import.meta.env.VITE_USE_GRAPHQL
  return flag === 'true' || flag === true
}
