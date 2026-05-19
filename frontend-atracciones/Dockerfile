# Desarrollo: Vite accesible desde el host en :5173 (API en el navegador → gateway :5050)
FROM node:22-alpine
WORKDIR /app

COPY frontend-atracciones/package.json frontend-atracciones/package-lock.json ./
RUN npm ci

COPY frontend-atracciones/ .

EXPOSE 5173
ENV VITE_API_URL=http://localhost:5050/api/v2

CMD ["npm", "run", "dev", "--", "--host", "0.0.0.0", "--port", "5173"]
