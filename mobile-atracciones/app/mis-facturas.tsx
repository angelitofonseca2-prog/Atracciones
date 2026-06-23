import React, { useCallback, useEffect, useState } from 'react';
import {
  FlatList, Modal, RefreshControl, ScrollView,
  StyleSheet, Text, TouchableOpacity, View,
} from 'react-native';
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
  rev_codigo?: string;
  total?: number;
  moneda?: string;
  estado?: string;
  fecha_emision?: string;
  fecha_creacion?: string;
  nombre_receptor?: string;
  correo_receptor?: string;
}

function estadoLabel(e: string | undefined) {
  const c = String(e ?? '').toUpperCase();
  if (c === 'A') return 'Emitida';
  if (c === 'C') return 'Cancelada';
  return e ?? '—';
}

function estadoColor(e: string | undefined) {
  const c = String(e ?? '').toUpperCase();
  if (c === 'A') return Colors.success;
  if (c === 'C') return Colors.danger;
  return Colors.warning;
}

function ModalDetalle({ factura, onCerrar }: { factura: Factura; onCerrar: () => void }) {
  return (
    <Modal visible animationType="slide" presentationStyle="formSheet" onRequestClose={onCerrar}>
      <SafeAreaView style={modal.safe}>
        <View style={modal.header}>
          <Text style={modal.titulo}>Factura #{factura.fac_numero ?? '—'}</Text>
          <TouchableOpacity onPress={onCerrar}><Text style={modal.cerrar}>✕</Text></TouchableOpacity>
        </View>
        <ScrollView contentContainerStyle={modal.scroll}>
          <Fila label="Numero" valor={factura.fac_numero ?? '—'} />
          {factura.rev_codigo && <Fila label="Codigo de reserva" valor={factura.rev_codigo} />}
          {factura.nombre_receptor && <Fila label="Receptor" valor={factura.nombre_receptor} />}
          {factura.correo_receptor && <Fila label="Correo" valor={factura.correo_receptor} />}
          <Fila
            label="Fecha de emision"
            valor={formatearFechaCorta(factura.fecha_emision ?? factura.fecha_creacion ?? '')}
          />
          <Fila
            label="Total"
            valor={`$${Number(factura.total ?? 0).toFixed(2)}${factura.moneda ? ` ${factura.moneda}` : ''}`}
            importante
          />
          <View style={modal.estadoRow}>
            <Text style={modal.estadoLabel}>Estado</Text>
            <Text style={[modal.estadoBadge, { color: estadoColor(factura.estado), borderColor: estadoColor(factura.estado), backgroundColor: `${estadoColor(factura.estado)}22` }]}>
              {estadoLabel(factura.estado)}
            </Text>
          </View>
          <Button title="Cerrar" onPress={onCerrar} variant="outline" style={{ marginTop: 24 }} />
        </ScrollView>
      </SafeAreaView>
    </Modal>
  );
}

function Fila({ label, valor, importante }: { label: string; valor: string; importante?: boolean }) {
  return (
    <View style={modal.fila}>
      <Text style={modal.filaLabel}>{label}</Text>
      <Text style={[modal.filaValor, importante && modal.filaImportante]}>{valor}</Text>
    </View>
  );
}

export default function MisFacturasScreen() {
  const { user } = useAuth();
  const [facturas, setFacturas] = useState<Factura[]>([]);
  const [cargando, setCargando] = useState(true);
  const [refrescando, setRefrescando] = useState(false);
  const [error, setError] = useState('');
  const [seleccionada, setSeleccionada] = useState<Factura | null>(null);

  const cargar = useCallback(async () => {
    try {
      setError('');
      const res = await listarMisFacturas() as Record<string, unknown>;
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
        renderItem={({ item: f }) => {
          const color = estadoColor(f.estado);
          return (
            <TouchableOpacity
              style={styles.card}
              activeOpacity={0.8}
              onPress={() => setSeleccionada(f)}
            >
              <View style={styles.cardHeader}>
                <Text style={styles.numero}>📄 {f.fac_numero ?? 'Factura'}</Text>
                <Text style={[styles.estadoBadge, { color, borderColor: color, backgroundColor: `${color}22` }]}>
                  {estadoLabel(f.estado)}
                </Text>
              </View>
              {f.rev_codigo && (
                <Text style={styles.revCodigo}>Reserva: {f.rev_codigo}</Text>
              )}
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
              <Text style={styles.verDetalle}>Ver detalle</Text>
            </TouchableOpacity>
          );
        }}
        ListHeaderComponent={
          error ? <Text style={styles.error}>{error}</Text> : null
        }
        ListEmptyComponent={
          !cargando ? (
            <View style={styles.empty}>
              <Text style={styles.emptyIcon}>🧾</Text>
              <Text style={styles.emptyText}>No tienes facturas aun</Text>
              <Text style={styles.emptySub}>Las facturas se generan al confirmar una reserva</Text>
              <Button title="Explorar atracciones" onPress={() => router.push('/(tabs)/catalogo')} style={{ marginTop: 20 }} />
            </View>
          ) : null
        }
      />
      {seleccionada && (
        <ModalDetalle factura={seleccionada} onCerrar={() => setSeleccionada(null)} />
      )}
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: Colors.background },
  list: { padding: 16 },
  card: { backgroundColor: Colors.surface, borderRadius: 14, padding: 16, marginBottom: 12 },
  cardHeader: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: 6 },
  numero: { color: Colors.text, fontWeight: '700', fontSize: 15, flex: 1 },
  estadoBadge: { fontSize: 11, fontWeight: '700', paddingHorizontal: 10, paddingVertical: 3, borderRadius: 20, borderWidth: 1, overflow: 'hidden' },
  revCodigo: { color: Colors.primary, fontSize: 12, marginBottom: 2, fontFamily: 'monospace' },
  receptor: { color: Colors.textMuted, fontSize: 13, marginBottom: 6 },
  cardFooter: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center' },
  fecha: { color: Colors.textMuted, fontSize: 13 },
  total: { color: Colors.primary, fontWeight: '700', fontSize: 16 },
  verDetalle: { color: Colors.primary, fontSize: 12, marginTop: 10 },
  error: { color: Colors.danger, textAlign: 'center', marginBottom: 16 },
  empty: { alignItems: 'center', paddingTop: 60 },
  emptyIcon: { fontSize: 56, marginBottom: 16 },
  emptyText: { color: Colors.text, fontSize: 18, fontWeight: '700', marginBottom: 8 },
  emptySub: { color: Colors.textMuted, fontSize: 14, textAlign: 'center', paddingHorizontal: 20 },
});

const modal = StyleSheet.create({
  safe: { flex: 1, backgroundColor: Colors.background },
  header: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', padding: 20, borderBottomWidth: 1, borderBottomColor: Colors.border },
  titulo: { color: Colors.text, fontSize: 18, fontWeight: '700' },
  cerrar: { color: Colors.textMuted, fontSize: 20, padding: 4 },
  scroll: { padding: 20 },
  fila: { flexDirection: 'row', justifyContent: 'space-between', flexWrap: 'wrap', marginBottom: 14 },
  filaLabel: { color: Colors.textMuted, fontSize: 14, flex: 1 },
  filaValor: { color: Colors.text, fontSize: 14, fontWeight: '500', maxWidth: '55%', textAlign: 'right' },
  filaImportante: { color: Colors.primary, fontSize: 16, fontWeight: '700' },
  estadoRow: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: 14 },
  estadoLabel: { color: Colors.textMuted, fontSize: 14 },
  estadoBadge: { fontSize: 12, fontWeight: '700', paddingHorizontal: 12, paddingVertical: 4, borderRadius: 20, borderWidth: 1, overflow: 'hidden' },
});
