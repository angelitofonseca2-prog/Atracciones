import { ApolloClient, InMemoryCache, createHttpLink, gql } from '@apollo/client'
import { setContext } from '@apollo/client/link/context'
import { getGraphqlUrl } from '../config/graphqlUrl'

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

export const apolloClient = new ApolloClient({
  link: authLink.concat(httpLink),
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
