import { Stack } from 'expo-router';
import { useEffect } from 'react';
import { router } from 'expo-router';
import { useAuth } from '@/lib/context/AuthContext';
import { Colors } from '@/constants/Colors';

export default function AdminLayout() {
  const { esAdmin, cargando, user } = useAuth();

  useEffect(() => {
    if (!cargando && !user) { router.replace('/auth/login'); return; }
    if (!cargando && !esAdmin) { router.replace('/(tabs)'); return; }
  }, [cargando, user, esAdmin]);

  if (cargando || !esAdmin) return null;

  return (
    <Stack
      screenOptions={{
        headerStyle: { backgroundColor: Colors.surface },
        headerTintColor: Colors.text,
        headerTitleStyle: { fontWeight: '700' },
        headerShadowVisible: false,
        contentStyle: { backgroundColor: Colors.background },
      }}
    >
      <Stack.Screen name="index" options={{ title: 'Panel Admin' }} />
      <Stack.Screen name="atracciones" options={{ title: 'Atracciones' }} />
      <Stack.Screen name="tickets" options={{ title: 'Tickets' }} />
      <Stack.Screen name="horarios" options={{ title: 'Horarios' }} />
      <Stack.Screen name="reservas" options={{ title: 'Reservas' }} />
      <Stack.Screen name="usuarios" options={{ title: 'Usuarios' }} />
      <Stack.Screen name="catalogos" options={{ title: 'Catálogos' }} />
    </Stack>
  );
}
