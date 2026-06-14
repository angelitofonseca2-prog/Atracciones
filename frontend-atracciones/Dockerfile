# Producción (Railway o build manual). Contexto = carpeta frontend-atracciones.
FROM node:22-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .

ARG VITE_API_URL=https://api-gateway-production-0afd.up.railway.app/api/v2
ARG VITE_GRAPHQL_URL=https://marketplace-gateway-production.up.railway.app/graphql
ARG VITE_USE_GRAPHQL=true
ENV VITE_API_URL=$VITE_API_URL
ENV VITE_GRAPHQL_URL=$VITE_GRAPHQL_URL
ENV VITE_USE_GRAPHQL=$VITE_USE_GRAPHQL

RUN npm run build

FROM node:22-alpine AS runtime
WORKDIR /app

ENV NODE_ENV=production
RUN npm install -g serve@14

COPY --from=build /app/dist ./dist

ENV PORT=3000
EXPOSE 3000

CMD ["sh", "-c", "serve -s dist -l ${PORT:-3000}"]
