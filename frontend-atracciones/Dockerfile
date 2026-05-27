# Producción (Railway o build manual). Contexto = carpeta frontend-atracciones.
FROM node:22-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .

ARG VITE_API_URL=https://api-gateway-production-0afd.up.railway.app/api/v2
ENV VITE_API_URL=$VITE_API_URL

RUN npm run build

FROM node:22-alpine AS runtime
WORKDIR /app

ENV NODE_ENV=production
RUN npm install -g serve@14

COPY --from=build /app/dist ./dist

ENV PORT=3000
EXPOSE 3000

CMD ["sh", "-c", "serve -s dist -l ${PORT:-3000}"]
