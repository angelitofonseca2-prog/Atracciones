import React, { useCallback, useEffect, useState } from 'react';
import { FlatList, RefreshControl, StyleSheet, Text, TouchableOpacity, View } from 'react-native';
import { router } from 'expo-router';
import { SafeAreaView } from 'react-native-safe-area-context';
import Button from '@/components/ui/Button';
import Spinner from '@/components/ui/Spinner';
import { listarMisFacturas } from '@/lib/api/facturasApi';
import { useAuth } from '@/lib/context/AuthContext';
import { formatearFechaCorta } from '@/lib/utils/formatFechas';
import { Colors } from '@/constants/Colors';

interface Factura {
  fac_guid?: string;
  fac_numero?: string;
  numero?: string;
  rev_guid?: string;
  total?: number;
  estado?: string;
  fecha_emision?: string;
  fecha_creacion?: string;
  nombre_receptor?: string;
}

export default function MisFacturasScreen() {
  const { user } = useAuth();
  const [facturas, setFacturas] = useState<Factura[]>([]);
  const [cargando, setCargando] = useState(true);
  const [refrescando, setRefrescando] = useState(false);
  const [error, setError] = useState('');

  const cargar = useCallback(async () => {
    try {
      setError('');
      const res = await listarMisFacturas() as Record<string, unknown>;
      // API: { status, message, data: [...] }
      const d = res?.data ?? res;
      setFacturas(Array.isArray(d) ? d : []);
    } catch (e: unknown) {
      const msg = (e as { response?: { data?: { message?: string } } })
        ?.response?.data?.message ?? 'No se pudo cargar tus facturas';
      setError(msg);
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
        keyExtractor={(f) => String(f.fac_guid ?? f.fac_numero ?? Math.random())}
        contentContainerStyle={styles.list}
        refreshControl={
          <RefreshControl
            refreshing={refrescando}
            onRefresh={() => { setRefrescando(true); cargar(); }}
            tintColor={Colors.primary}
          />
        }
        renderItem={({ item: f }) => (
          <TouchableOpacity
            style={styles.card}
            activeOpacity={0.8}
            onPress={() => f.rev_guid && router.push(`/mis-reservas/${f.rev_guid}`)}
          >
            <View style={styles.cardHeader}>
              <Text style={styles.numero}>📄 {f.fac_numero ?? f.numero ?? 'Factura'}</Text>
              <Text style={[
                styles.estadoBadge,
                f.estado === 'EMITIDA' ? styles.emitida : styles.pendiente,
              ]}>
                {f.estado ?? '—'}
              </Text>
            </View>
            {f.nombre_receptor && (
              <Text style={styles.receptor}>{f.nombre_receptor}</Text>
            )}
            <View style={styles.cardFooter}>
              <Text style={styles.fecha}>
                📅 {formatearFechaCorta(f.fecha_emision ?? f.fecha_creacion ?? '')}
              </Text>
              {f.total != null && (
                <Text style={styles.total}>${Number(f.total).toFixed(2)}</Text>
              )}
            </View>
            {f.rev_guid && (
              <Text style={styles.verReserva}>Ver reserva →</Text>
            )}
          </TouchableOpacity>
        )}
        ListHeaderComponent={
          error ? <Text style={styles.error}>{error}</Text> : null
        }
        ListEmptyComponent={
          !cargando ? (
            <View style={styles.empty}>
              <Text style={styles.emptyIcon}>🧾</Text>
              <Text style={styles.emptyText}>No tienes facturas aún</Text>
              <Text style={styles.emptySub}>
                Las facturas se generan al confirmar una reserva
              </Text>
              <Button
                title="Explorar atracciones"
                onPress={() => router.push('/(tabs)/catalogo')}
                style={{ marginTop: 20 }}
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
  numero: { color: Colors.text, fontWeight: '700', fontSize: 15, flex: 1 },
  estadoBadge: { fontSize: 11, fontWeight: '700', paddingHorizontal: 10, paddingVertical: 3, borderRadius: 20 },
  emitida: { backgroundColor: `${Colors.success}22`, color: Colors.success },
  pendiente: { backgroundColor: `${Colors.warning}22`, color: Colors.warning },
  receptor: { color: Colors.textMuted, fontSize: 13, marginBottom: 8 },
  cardFooter: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center' },
  fecha: { color: Colors.textMuted, fontSize: 13 },
  total: { color: Colors.primary, fontWeight: '700', fontSize: 16 },
  verReserva: { color: Colors.primary, fontSize: 12, marginTop: 10 },
  error: { color: Colors.danger, textAlign: 'center', marginBottom: 16 },
  empty: { alignItems: 'center', paddingTop: 60 },
  emptyIcon: { fontSize: 56, marginBottom: 16 },
  emptyText: { color: Colors.text, fontSize: 18, fontWeight: '700', marginBottom: 8 },
  emptySub: { color: Colors.textMuted, fontSize: 14, textAlign: 'center', paddingHorizontal: 20 },
});
