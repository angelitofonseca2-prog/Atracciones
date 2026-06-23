import React, { useCallback, useEffect, useState } from 'react';
import { Alert, FlatList, RefreshControl, StyleSheet, Text, TouchableOpacity, View } from 'react-native';
import { router } from 'expo-router';
import { SafeAreaView } from 'react-native-safe-area-context';
import Badge from '@/components/ui/Badge';
import Button from '@/components/ui/Button';
import Spinner from '@/components/ui/Spinner';
import { listarMisReservas } from '@/lib/api/reservasApi';
import { useAuth } from '@/lib/context/AuthContext';
import { normalizarReserva, ReservaNormalizada } from '@/lib/utils/mappers';
import { formatearFechaCorta } from '@/lib/utils/formatFechas';
import { Colors } from '@/constants/Colors';

export default function MisReservasScreen() {
  const { user } = useAuth();
  const [reservas, setReservas] = useState<ReservaNormalizada[]>([]);
  const [cargando, setCargando] = useState(true);
  const [refrescando, setRefrescando] = useState(false);
  const [error, setError] = useState('');

  const cargar = useCallback(async () => {
    try {
      setError('');
      const res = await listarMisReservas() as Record<string, unknown>;
      const d = (res?.data ?? res) as Record<string, unknown>[];
      const lista = Array.isArray(d) ? d.map((r) => normalizarReserva(r as Record<string, unknown>)) : [];
      setReservas(lista);
    } catch (e: unknown) {
      const msg = (e as { response?: { data?: { message?: string } } })?.response?.data?.message;
      setError(msg ?? 'No se pudo cargar tus reservas');
    } finally {
      setCargando(false);
      setRefrescando(false);
    }
  }, []);

  useEffect(() => {
    if (!user) { router.replace('/auth/login'); return; }
    cargar();
  }, [user, cargar]);

  if (!user) return null;
  if (cargando) return <Spinner texto="Cargando reservas..." />;

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      <FlatList
        data={reservas}
        keyExtractor={(r) => r.rev_guid || r.rev_codigo || String(Math.random())}
        contentContainerStyle={styles.list}
        refreshControl={
          <RefreshControl
            refreshing={refrescando}
            onRefresh={() => { setRefrescando(true); cargar(); }}
            tintColor={Colors.primary}
          />
        }
        renderItem={({ item: r }) => (
          <TouchableOpacity
            style={styles.card}
            activeOpacity={0.8}
            onPress={() => router.push(`/mis-reservas/${r.rev_guid}`)}
          >
            <View style={styles.cardHeader}>
              <Text style={styles.codigo}>{r.rev_codigo || '—'}</Text>
              <Badge estado={r.rev_estado} />
            </View>
            {r.atraccion_nombre ? (
              <Text style={styles.nombre}>{r.atraccion_nombre}</Text>
            ) : null}
            <View style={styles.cardFooter}>
              <Text style={styles.fecha}>
                📅 {formatearFechaCorta(r.fecha_visita || r.rev_fecha_reserva_utc)}
              </Text>
              {r.rev_total > 0 && (
                <Text style={styles.total}>${r.rev_total.toFixed(2)}</Text>
              )}
            </View>
          </TouchableOpacity>
        )}
        ListHeaderComponent={
          error ? <Text style={styles.error}>{error}</Text> : null
        }
        ListEmptyComponent={
          !cargando ? (
            <View style={styles.empty}>
              <Text style={styles.emptyIcon}>📅</Text>
              <Text style={styles.emptyText}>No tienes reservas aún</Text>
              <Button
                title="Explorar atracciones"
                onPress={() => router.push('/(tabs)/catalogo')}
                style={{ marginTop: 16 }}
              />
            </View>
          ) : null
        }
      />
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: Colors.background },
  list: { padding: 16 },
  card: { backgroundColor: Colors.surface, borderRadius: 14, padding: 16, marginBottom: 12 },
  cardHeader: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: 8 },
  codigo: { color: Colors.text, fontWeight: '700', fontSize: 15 },
  nombre: { color: Colors.textMuted, fontSize: 13, marginBottom: 10 },
  cardFooter: { flexDirection: 'row', justifyContent: 'space-between' },
  fecha: { color: Colors.textMuted, fontSize: 13 },
  total: { color: Colors.primary, fontWeight: '700' },
  error: { color: Colors.danger, textAlign: 'center', marginBottom: 16 },
  empty: { alignItems: 'center', paddingTop: 60 },
  emptyIcon: { fontSize: 48, marginBottom: 16 },
  emptyText: { color: Colors.textMuted, fontSize: 16 },
});
