import {
  ApolloClient,
  InMemoryCache,
  createHttpLink,
  split,
  gql,
} from '@apollo/client';
import { setContext } from '@apollo/client/link/context';
import { GraphQLWsLink } from '@apollo/client/link/subscriptions';
import { getMainDefinition } from '@apollo/client/utilities';
import { createClient as createWsClient } from 'graphql-ws';
import * as SecureStore from 'expo-secure-store';
import { GRAPHQL_URL, GRAPHQL_WS_URL } from '@/constants/Config';
import { TOKEN_KEY } from '../api/client';

const httpLink = createHttpLink({ uri: GRAPHQL_URL });

const authLink = setContext(async (_, { headers }) => {
  const token = await SecureStore.getItemAsync(TOKEN_KEY);
  const correlationId = `${Date.now()}-${Math.random().toString(36).slice(2)}`;
  return {
    headers: {
      ...headers,
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      'X-Correlation-ID': correlationId,
    },
  };
});

const wsLink = new GraphQLWsLink(
  createWsClient({
    url: GRAPHQL_WS_URL,
    connectionParams: async () => {
      const token = await SecureStore.getItemAsync(TOKEN_KEY);
      return token ? { Authorization: `Bearer ${token}` } : {};
    },
    shouldRetry: () => true,
    retryAttempts: 5,
  }),
);

const splitLink = split(
  ({ query }) => {
    const def = getMainDefinition(query);
    return def.kind === 'OperationDefinition' && def.operation === 'subscription';
  },
  wsLink,
  authLink.concat(httpLink),
);

export const apolloClient = new ApolloClient({
  link: splitLink,
  cache: new InMemoryCache(),
});

export const QUERIES = {
  ATRACCIONES: gql`
    query Atracciones($ciudad: String, $tipo: String, $subtipo: String, $idioma: String,
      $calificacionMin: Float, $disponible: Boolean, $ordenarPor: String, $page: Int, $limit: Int) {
      atracciones(ciudad: $ciudad, tipo: $tipo, subtipo: $subtipo, idioma: $idioma,
        calificacionMin: $calificacionMin, disponible: $disponible,
        ordenarPor: $ordenarPor, page: $page, limit: $limit)
    }
  `,
  ATRACCION: gql`query Atraccion($guid: UUID!) { atraccion(guid: $guid) }`,
  HORARIOS: gql`query Horarios($atGuid: UUID!, $disponibles: Boolean) { horarios(atGuid: $atGuid, disponibles: $disponibles) }`,
  TICKETS: gql`query Tickets($atGuid: UUID!) { tickets(atGuid: $atGuid) }`,
  ESTADO_RESERVA: gql`
    query EstadoReserva($seguimientoId: UUID!) {
      estadoReserva(seguimientoId: $seguimientoId) {
        seguimientoId revGuid revCodigo estado motivoRechazo correlationId
      }
    }
  `,
};

export const MUTATIONS = {
  SOLICITAR_RESERVA: gql`
    mutation SolicitarReserva($input: SolicitarReservaInput!) {
      solicitarReserva(input: $input) { seguimientoId revGuid estado correlationId }
    }
  `,
};

export const SUBSCRIPTIONS = {
  ESTADO_RESERVA: gql`
    subscription OnEstadoReservaActualizado($seguimientoId: UUID!) {
      onEstadoReservaActualizado(seguimientoId: $seguimientoId) {
        seguimientoId revGuid revCodigo estado motivoRechazo correlationId
      }
    }
  `,
};

export function parseGraphqlJson(raw: unknown): unknown {
  if (!raw) return null;
  if (typeof raw === 'string') return JSON.parse(raw);
  return raw;
}
