import '../global.css';
import { ApolloProvider } from '@apollo/client';
import { SplashScreen, Stack } from 'expo-router';
import { StatusBar } from 'expo-status-bar';
import { useEffect } from 'react';
import { GestureHandlerRootView } from 'react-native-gesture-handler';
import { apolloClient } from '@/lib/graphql/client';
import { AuthProvider } from '@/lib/context/AuthContext';
import { Colors } from '@/constants/Colors';

SplashScreen.preventAutoHideAsync();

export default function RootLayout() {
  useEffect(() => {
    SplashScreen.hideAsync();
  }, []);

  return (
    <GestureHandlerRootView style={{ flex: 1 }}>
      <ApolloProvider client={apolloClient}>
        <AuthProvider>
          <StatusBar style="light" />
          <Stack
            screenOptions={{
              headerStyle: { backgroundColor: Colors.surface },
              headerTintColor: Colors.text,
              headerTitleStyle: { fontWeight: '700' },
              contentStyle: { backgroundColor: Colors.background },
              headerShadowVisible: false,
            }}
          >
            <Stack.Screen name="(tabs)" options={{ headerShown: false }} />
            <Stack.Screen name="auth/login" options={{ title: 'Iniciar sesión', headerShown: false }} />
            <Stack.Screen name="auth/registro" options={{ title: 'Crear cuenta', headerShown: false }} />
            <Stack.Screen name="atracciones/[guid]" options={{ title: '' }} />
            <Stack.Screen name="reservar/[guid]" options={{ title: 'Reservar' }} />
            <Stack.Screen name="mis-reservas" options={{ title: 'Mis Reservas' }} />
            <Stack.Screen name="mis-reservas/[guid]" options={{ title: 'Detalle de Reserva' }} />
            <Stack.Screen name="mis-facturas" options={{ title: 'Mis Facturas' }} />
            <Stack.Screen name="perfil" options={{ title: 'Mi Perfil' }} />
            <Stack.Screen name="admin" options={{ headerShown: false }} />
          </Stack>
        </AuthProvider>
      </ApolloProvider>
    </GestureHandlerRootView>
  );
}
