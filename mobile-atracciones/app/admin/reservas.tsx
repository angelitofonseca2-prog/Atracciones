import React, { useCallback, useEffect, useState } from 'react';
import { FlatList, RefreshControl, StyleSheet, Text, TouchableOpacity, View } from 'react-native';
import { router } from 'expo-router';
import { SafeAreaView } from 'react-native-safe-area-context';
import Badge from '@/components/ui/Badge';
import Spinner from '@/components/ui/Spinner';
import { listarReservasAdmin } from '@/lib/api/adminApi';
import { formatearFechaCorta } from '@/lib/utils/formatFechas';
import { Colors } from '@/constants/Colors';

interface Reserva {
  rev_guid?: string; rev_codigo?: string;
  rev_estado?: string; estado?: string;
  fecha_visita?: string; rev_total?: number; total?: number;
  atraccion_nombre?: string; cliente_nombre?: string;
}

export default function AdminReservasScreen() {
  const [items, setItems] = useState<Reserva[]>([]);
  const [cargando, setCargando] = useState(true);
  const [refrescando, setRefrescando] = useState(false);

  const cargar = useCallback(async () => {
    try {
      const res = await listarReservasAdmin();
      const d = res?.data ?? res;
      setItems(Array.isArray(d) ? d : d?.items ?? d?.reservas ?? []);
    } catch {} finally { setCargando(false); setRefrescando(false); }
  }, []);

  useEffect(() => { cargar(); }, [cargar]);

  if (cargando) return <Spinner texto="Cargando reservas..." />;

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      <FlatList
        data={items}
        keyExtractor={(r) => String(r.rev_guid ?? Math.random())}
        contentContainerStyle={styles.list}
        refreshControl={<RefreshControl refreshing={refrescando} onRefresh={() => { setRefrescando(true); cargar(); }} tintColor={Colors.primary} />}
        renderItem={({ item: r }) => (
          <TouchableOpacity style={styles.card} activeOpacity={0.8} onPress={() => router.push(`/mis-reservas/${r.rev_guid}` as never)}>
            <View style={styles.cardHeader}>
              <Text style={styles.codigo}>{r.rev_codigo ?? '—'}</Text>
              <Badge estado={r.rev_estado ?? r.estado ?? 'P'} />
            </View>
            {r.cliente_nombre && <Text style={styles.cliente}>👤 {r.cliente_nombre}</Text>}
            <View style={styles.cardFooter}>
              <Text style={styles.fecha}>📅 {formatearFechaCorta(r.fecha_visita ?? '')}</Text>
              {(r.rev_total ?? r.total) != null && (
                <Text style={styles.total}>${Number(r.rev_total ?? r.total).toFixed(2)}</Text>
              )}
            </View>
          </TouchableOpacity>
        )}
        ListEmptyComponent={<Text style={styles.empty}>No hay reservas</Text>}
      />
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: Colors.background },
  list: { padding: 16 },
  card: { backgroundColor: Colors.surface, borderRadius: 14, padding: 16, marginBottom: 10 },
  cardHeader: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: 6 },
  codigo: { color: Colors.text, fontWeight: '700', fontSize: 15 },
  cliente: { color: Colors.textMuted, fontSize: 13, marginBottom: 8 },
  cardFooter: { flexDirection: 'row', justifyContent: 'space-between' },
  fecha: { color: Colors.textMuted, fontSize: 13 },
  total: { color: Colors.primary, fontWeight: '700' },
  empty: { color: Colors.textMuted, textAlign: 'center', marginTop: 40 },
});
