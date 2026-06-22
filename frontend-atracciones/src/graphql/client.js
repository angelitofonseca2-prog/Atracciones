import {
  ApolloClient,
  InMemoryCache,
  createHttpLink,
  gql,
  split,
} from '@apollo/client'
import { setContext } from '@apollo/client/link/context'
import { GraphQLWsLink } from '@apollo/client/link/subscriptions'
import { getMainDefinition } from '@apollo/client/utilities'
import { createClient as createWsClient } from 'graphql-ws'
import { getGraphqlUrl, getGraphqlWsUrl } from '../config/graphqlUrl'

const httpLink = createHttpLink({
  uri: getGraphqlUrl(),
})

const authLink = setContext((_, { headers }) => {
  const token = localStorage.getItem('token')
  const correlationId =
    typeof crypto !== 'undefined' && crypto.randomUUID
      ? crypto.randomUUID()
      : `${Date.now()}-${Math.random().toString(36).slice(2)}`

  return {
    headers: {
      ...headers,
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      'X-Correlation-ID': correlationId,
    },
  }
})

// Enlace WebSocket para subscriptions (GraphQL over WebSockets).
// Si el gateway no tiene WebSocket activo, el hook de reserva usa polling como fallback.
const wsLink = new GraphQLWsLink(
  createWsClient({
    url: getGraphqlWsUrl(),
    connectionParams: () => {
      const token = localStorage.getItem('token')
      return token ? { Authorization: `Bearer ${token}` } : {}
    },
    // Reconecta automáticamente si se cae la conexión.
    shouldRetry: () => true,
    retryAttempts: 5,
    retryWait: (attempt) =>
      new Promise((resolve) => setTimeout(resolve, Math.min(1000 * 2 ** attempt, 30000))),
    on: {
      error: (err) => {
        // eslint-disable-next-line no-console
        console.warn('[GraphQL WS] error de conexión, se usará polling como fallback →', err)
      },
    },
  }),
)

// Las subscriptions van por WebSocket; queries y mutations por HTTP.
const splitLink = split(
  ({ query }) => {
    const def = getMainDefinition(query)
    return def.kind === 'OperationDefinition' && def.operation === 'subscription'
  },
  wsLink,
  authLink.concat(httpLink),
)

export const apolloClient = new ApolloClient({
  link: splitLink,
  cache: new InMemoryCache(),
})

export const QUERIES = {
  ATRACCIONES: gql`
    query Atracciones(
      $ciudad: String
      $tipo: String
      $subtipo: String
      $idioma: String
      $calificacionMin: Float
      $disponible: Boolean
      $ordenarPor: String
      $page: Int
      $limit: Int
    ) {
      atracciones(
        ciudad: $ciudad
        tipo: $tipo
        subtipo: $subtipo
        idioma: $idioma
        calificacionMin: $calificacionMin
        disponible: $disponible
        ordenarPor: $ordenarPor
        page: $page
        limit: $limit
      )
    }
  `,
  FILTROS: gql`
    query Filtros($ciudad: String) {
      filtros(ciudad: $ciudad)
    }
  `,
  ATRACCION: gql`
    query Atraccion($guid: UUID!) {
      atraccion(guid: $guid)
    }
  `,
  HORARIOS: gql`
    query Horarios($atGuid: UUID!, $disponibles: Boolean) {
      horarios(atGuid: $atGuid, disponibles: $disponibles)
    }
  `,
  TICKETS: gql`
    query Tickets($atGuid: UUID!) {
      tickets(atGuid: $atGuid)
    }
  `,
  ESTADO_RESERVA: gql`
    query EstadoReserva($seguimientoId: UUID!) {
      estadoReserva(seguimientoId: $seguimientoId) {
        seguimientoId
        revGuid
        revCodigo
        estado
        motivoRechazo
        correlationId
      }
    }
  `,
}

export const SUBSCRIPTIONS = {
  ESTADO_RESERVA: gql`
    subscription OnEstadoReservaActualizado($seguimientoId: UUID!) {
      onEstadoReservaActualizado(seguimientoId: $seguimientoId) {
        seguimientoId
        revGuid
        revCodigo
        estado
        motivoRechazo
        correlationId
      }
    }
  `,
}

export const MUTATIONS = {
  SOLICITAR_RESERVA: gql`
    mutation SolicitarReserva($input: SolicitarReservaInput!) {
      solicitarReserva(input: $input) {
        seguimientoId
        revGuid
        estado
        correlationId
      }
    }
  `,
}

export function parseGraphqlJson(raw) {
  if (!raw) return null
  if (typeof raw === 'string') return JSON.parse(raw)
  return raw
}
