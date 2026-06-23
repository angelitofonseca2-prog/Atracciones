import React, { useCallback, useEffect, useState } from 'react';
import { FlatList, RefreshControl, StyleSheet, Text, View } from 'react-native';
import { router } from 'expo-router';
import { SafeAreaView } from 'react-native-safe-area-context';
import Spinner from '@/components/ui/Spinner';
import Button from '@/components/ui/Button';
import { listarMisFacturas } from '@/lib/api/facturasApi';
import { useAuth } from '@/lib/context/AuthContext';
import { formatearFechaCorta } from '@/lib/utils/formatFechas';
import { Colors } from '@/constants/Colors';

interface Factura { fac_guid?: string; numero?: string; fecha?: string; total?: number; estado?: string; }

export default function MisFacturasScreen() {
  const { user } = useAuth();
  const [facturas, setFacturas] = useState<Factura[]>([]);
  const [cargando, setCargando] = useState(true);
  const [refrescando, setRefrescando] = useState(false);
  const [error, setError] = useState('');

  const cargar = useCallback(async () => {
    try {
      setError('');
      const res = await listarMisFacturas();
      const d = res?.data ?? res;
      setFacturas(Array.isArray(d) ? d : d?.items ?? d?.facturas ?? []);
    } catch {
      setError('No se pudo cargar tus facturas');
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
  if (cargando) return <Spinner texto="Cargando facturas..." />;

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      <FlatList
        data={facturas}
        keyExtractor={(f) => String(f.fac_guid ?? Math.random())}
        contentContainerStyle={styles.list}
        refreshControl={<RefreshControl refreshing={refrescando} onRefresh={() => { setRefrescando(true); cargar(); }} tintColor={Colors.primary} />}
        renderItem={({ item: f }) => (
          <View style={styles.card}>
            <View style={styles.cardHeader}>
              <Text style={styles.numero}>🧾 {f.numero ?? '—'}</Text>
              <Text style={styles.total}>${Number(f.total ?? 0).toFixed(2)}</Text>
            </View>
            <Text style={styles.fecha}>📅 {formatearFechaCorta(f.fecha ?? '')}</Text>
            {f.estado && <Text style={styles.estado}>{f.estado}</Text>}
          </View>
        )}
        ListHeaderComponent={error ? <Text style={styles.error}>{error}</Text> : null}
        ListEmptyComponent={
          !cargando ? (
            <View style={styles.empty}>
              <Text style={styles.emptyIcon}>🧾</Text>
              <Text style={styles.emptyText}>No tienes facturas aún</Text>
              <Button title="Ver mis reservas" onPress={() => router.push('/mis-reservas')} style={{ marginTop: 16 }} />
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
  cardHeader: { flexDirection: 'row', justifyContent: 'space-between', marginBottom: 6 },
  numero: { color: Colors.text, fontWeight: '700', fontSize: 15 },
  total: { color: Colors.primary, fontWeight: '700', fontSize: 16 },
  fecha: { color: Colors.textMuted, fontSize: 13 },
  estado: { color: Colors.textMuted, fontSize: 12, marginTop: 4 },
  error: { color: Colors.danger, textAlign: 'center', marginBottom: 16 },
  empty: { alignItems: 'center', paddingTop: 60 },
  emptyIcon: { fontSize: 48, marginBottom: 16 },
  emptyText: { color: Colors.textMuted, fontSize: 16 },
});
