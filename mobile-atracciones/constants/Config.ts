import Constants from 'expo-constants';

const extra = Constants.expoConfig?.extra ?? {};

export const API_URL: string =
  (extra.apiUrl as string) ?? 'https://api-gateway-production-0afd.up.railway.app/api/v2';

export const GRAPHQL_URL: string =
  (extra.graphqlUrl as string) ?? 'https://marketplace-gateway-production.up.railway.app/graphql';

export const GRAPHQL_WS_URL: string = GRAPHQL_URL.replace(/^http/, 'ws');

export const USE_GRAPHQL: boolean = (extra.useGraphql as boolean) ?? false;
