# Atracciones — App Móvil (React Native + Expo)

App nativa para Android (e iOS) que consume el mismo API Gateway y GraphQL del proyecto.

## Requisitos previos

- Node.js 20+
- npm 10+
- Expo CLI: `npm install -g expo-cli`
- EAS CLI (para builds): `npm install -g eas-cli`
- Expo Go app en tu celular (para probar sin compilar)

## Instalación

```bash
cd mobile-atracciones
npm install
```

## Desarrollo local

```bash
# Iniciar en modo Expo Go (escanea QR con la app Expo Go)
npx expo start

# Iniciar en emulador Android
npx expo start --android

# Iniciar en emulador iOS (solo macOS)
npx expo start --ios
```

## Variables de entorno

Edita `app.json` → campo `extra` con tus URLs de Railway:

```json
{
  "extra": {
    "apiUrl": "https://TU-API-GATEWAY.up.railway.app/api/v2",
    "graphqlUrl": "https://TU-MARKETPLACE-GATEWAY.up.railway.app/graphql",
    "useGraphql": true
  }
}
```

## Generar APK Android (para distribuir directamente)

1. Crea una cuenta gratuita en [expo.dev](https://expo.dev)
2. Inicia sesión: `eas login`
3. Configura el proyecto: `eas build:configure`
4. Genera el APK:

```bash
# APK de vista previa (para instalar directamente en Android)
eas build -p android --profile preview

# AAB de producción (para Google Play)
eas build -p android --profile production
```

5. Descarga el APK desde el dashboard de Expo Build
6. Instala en Android: transfiere el .apk al celular y ábrelo

## Estructura del proyecto

```
mobile-atracciones/
├── app/                   # Expo Router (rutas por archivos)
│   ├── (tabs)/            # Tabs: Home, Explorar, Mi Cuenta
│   ├── auth/              # Login y Registro
│   ├── atracciones/       # Detalle de atracción
│   ├── reservar/          # Flujo completo de reserva
│   ├── mis-reservas/      # Mis reservas y detalle
│   ├── mis-facturas.tsx   # Mis facturas
│   ├── perfil.tsx         # Mi perfil
│   └── admin/             # Panel administrador
├── components/            # Componentes reutilizables
│   ├── ui/                # Button, Input, Badge, Spinner, Select
│   ├── atracciones/       # TarjetaAtraccion
│   └── reservas/          # CalendarioVisita
├── lib/                   # Lógica de negocio adaptada del web
│   ├── api/               # axios + interceptors (SecureStore para JWT)
│   ├── graphql/           # Apollo Client + queries/mutations/subscriptions
│   ├── utils/             # validaciones, fechas, estados
│   └── context/           # AuthContext
├── constants/             # Colors, Config (URLs Railway)
└── hooks/                 # Hooks de datos
```

## Notas

- El JWT se guarda en `expo-secure-store` (encriptado en el dispositivo)
- Fallback automático de GraphQL a REST si el gateway no está disponible
- El panel admin solo es visible para usuarios con rol ADMIN
