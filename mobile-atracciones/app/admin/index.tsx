import React from 'react';
import { ScrollView, StyleSheet, Text, TouchableOpacity, View } from 'react-native';
import { router } from 'expo-router';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Colors } from '@/constants/Colors';

const SECCIONES = [
  { icon: '🏔', label: 'Atracciones', ruta: '/admin/atracciones', desc: 'Crear, editar y eliminar atracciones' },
  { icon: '🎟', label: 'Tickets', ruta: '/admin/tickets', desc: 'Gestionar tipos de tickets y precios' },
  { icon: '🗓', label: 'Horarios', ruta: '/admin/horarios', desc: 'Programar horarios y disponibilidad' },
  { icon: '📅', label: 'Reservas', ruta: '/admin/reservas', desc: 'Ver y gestionar todas las reservas' },
  { icon: '👥', label: 'Usuarios', ruta: '/admin/usuarios', desc: 'Ver usuarios registrados' },
  { icon: '🗺', label: 'Catálogos', ruta: '/admin/catalogos', desc: 'Destinos, categorías, idiomas e incluye' },
];

export default function AdminDashboard() {
  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      <ScrollView contentContainerStyle={styles.scroll}>
        <View style={styles.header}>
          <Text style={styles.titulo}>⚙ Panel de Administración</Text>
          <Text style={styles.sub}>Gestiona todos los recursos de la plataforma</Text>
        </View>

        {SECCIONES.map((s) => (
          <TouchableOpacity key={s.ruta} style={styles.card} activeOpacity={0.8} onPress={() => router.push(s.ruta as never)}>
            <Text style={styles.icon}>{s.icon}</Text>
            <View style={styles.cardInfo}>
              <Text style={styles.cardLabel}>{s.label}</Text>
              <Text style={styles.cardDesc}>{s.desc}</Text>
            </View>
            <Text style={styles.arrow}>›</Text>
          </TouchableOpacity>
        ))}
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: Colors.background },
  scroll: { padding: 20 },
  header: { marginBottom: 24 },
  titulo: { color: Colors.text, fontSize: 22, fontWeight: '700', marginBottom: 4 },
  sub: { color: Colors.textMuted, fontSize: 14 },
  card: { backgroundColor: Colors.surface, borderRadius: 14, padding: 16, marginBottom: 12, flexDirection: 'row', alignItems: 'center', gap: 14 },
  icon: { fontSize: 32, width: 46, textAlign: 'center' },
  cardInfo: { flex: 1 },
  cardLabel: { color: Colors.text, fontSize: 16, fontWeight: '700', marginBottom: 2 },
  cardDesc: { color: Colors.textMuted, fontSize: 13 },
  arrow: { color: Colors.textMuted, fontSize: 22 },
});
