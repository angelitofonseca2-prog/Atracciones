import React from 'react';
import { ScrollView, StyleSheet, Text, TouchableOpacity, View } from 'react-native';
import { router } from 'expo-router';
import { SafeAreaView } from 'react-native-safe-area-context';
import Button from '@/components/ui/Button';
import { useAuth } from '@/lib/context/AuthContext';
import { Colors } from '@/constants/Colors';

interface MenuItem { icon: string; label: string; route: string; adminOnly?: boolean }

const MENU: MenuItem[] = [
  { icon: '📅', label: 'Mis Reservas', route: '/mis-reservas' },
  { icon: '🧾', label: 'Mis Facturas', route: '/mis-facturas' },
  { icon: '👤', label: 'Mi Perfil', route: '/perfil' },
  { icon: '🛠', label: 'Panel Admin', route: '/admin', adminOnly: true },
];

export default function CuentaScreen() {
  const { user, cerrarSesion, esAdmin } = useAuth();

  if (!user) {
    return (
      <SafeAreaView style={styles.safe}>
        <View style={styles.guestContainer}>
          <Text style={styles.guestIcon}>👤</Text>
          <Text style={styles.guestTitle}>No has iniciado sesión</Text>
          <Text style={styles.guestSub}>Inicia sesión para ver tus reservas y gestionar tu cuenta</Text>
          <Button title="Iniciar sesión" onPress={() => router.push('/auth/login')} size="lg" style={{ marginTop: 24 }} />
          <Button title="Crear cuenta" onPress={() => router.push('/auth/registro')} variant="outline" size="lg" style={{ marginTop: 12 }} />
        </View>
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaView style={styles.safe}>
      <ScrollView contentContainerStyle={styles.scroll}>
        {/* Avatar */}
        <View style={styles.avatarSection}>
          <View style={styles.avatar}>
            <Text style={styles.avatarText}>{user.nombre?.charAt(0)?.toUpperCase() ?? '?'}</Text>
          </View>
          <Text style={styles.nombre}>{user.nombre}</Text>
          <Text style={styles.correo}>{user.correo}</Text>
          {esAdmin && (
            <View style={styles.adminBadge}>
              <Text style={styles.adminBadgeText}>⚙ ADMIN</Text>
            </View>
          )}
        </View>

        {/* Menú */}
        <View style={styles.menu}>
          {MENU.filter((m) => !m.adminOnly || esAdmin).map((m) => (
            <TouchableOpacity key={m.route} style={styles.menuItem} onPress={() => router.push(m.route as never)} activeOpacity={0.7}>
              <Text style={styles.menuIcon}>{m.icon}</Text>
              <Text style={styles.menuLabel}>{m.label}</Text>
              <Text style={styles.menuArrow}>›</Text>
            </TouchableOpacity>
          ))}
        </View>

        <Button title="Cerrar sesión" onPress={cerrarSesion} variant="danger" style={{ marginTop: 24 }} />
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: Colors.background },
  scroll: { padding: 24 },
  guestContainer: { flex: 1, alignItems: 'center', justifyContent: 'center', padding: 40 },
  guestIcon: { fontSize: 64, marginBottom: 16 },
  guestTitle: { color: Colors.text, fontSize: 22, fontWeight: '700', marginBottom: 8 },
  guestSub: { color: Colors.textMuted, fontSize: 14, textAlign: 'center' },
  avatarSection: { alignItems: 'center', marginBottom: 32 },
  avatar: { width: 80, height: 80, borderRadius: 40, backgroundColor: Colors.primary, alignItems: 'center', justifyContent: 'center', marginBottom: 12 },
  avatarText: { fontSize: 32, color: '#fff', fontWeight: '700' },
  nombre: { color: Colors.text, fontSize: 20, fontWeight: '700', marginBottom: 4 },
  correo: { color: Colors.textMuted, fontSize: 14 },
  adminBadge: { backgroundColor: `${Colors.warning}33`, borderRadius: 20, paddingHorizontal: 12, paddingVertical: 4, marginTop: 8, borderWidth: 1, borderColor: Colors.warning },
  adminBadgeText: { color: Colors.warning, fontSize: 12, fontWeight: '700' },
  menu: { backgroundColor: Colors.surface, borderRadius: 16, overflow: 'hidden' },
  menuItem: { flexDirection: 'row', alignItems: 'center', padding: 18, borderBottomWidth: 1, borderBottomColor: Colors.border },
  menuIcon: { fontSize: 22, marginRight: 14 },
  menuLabel: { flex: 1, color: Colors.text, fontSize: 16 },
  menuArrow: { color: Colors.textMuted, fontSize: 22 },
});
