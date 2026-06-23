import React, { useCallback, useEffect, useState } from 'react';
import { Alert, ScrollView, StyleSheet, Text, View } from 'react-native';
import { router, useLocalSearchParams } from 'expo-router';
import { SafeAreaView } from 'react-native-safe-area-context';
import Badge from '@/components/ui/Badge';
import Button from '@/components/ui/Button';
import Input from '@/components/ui/Input';
import Spinner from '@/components/ui/Spinner';
import { cancelarReserva, obtenerReserva } from '@/lib/api/reservasApi';
import { crearResenia } from '@/lib/api/atraccionesApi';
import { esReservaCancelable, esReservaConfirmada } from '@/lib/utils/estadoReserva';
import { formatearFechaCorta } from '@/lib/utils/formatFechas';
import { Colors } from '@/constants/Colors';

export default function DetalleReservaScreen() {
  const { guid } = useLocalSearchParams<{ guid: string }>();
  const [reserva, setReserva] = useState<Record<string, unknown> | null>(null);
  const [cargando, setCargando] = useState(true);
  const [cancelando, setCancelando] = useState(false);
  const [enviandoResenia, setEnviandoResenia] = useState(false);

  // Reseña
  const [calificacion, setCalificacion] = useState(5);
  const [comentario, setComentario] = useState('');
  const [reseniaEnviada, setReseniaEnviada] = useState(false);

  const cargar = useCallback(async () => {
    if (!guid) return;
    try {
      const res = await obtenerReserva(guid);
      setReserva(res?.data ?? res);
    } catch {
      Alert.alert('Error', 'No se pudo cargar la reserva');
    } finally {
      setCargando(false);
    }
  }, [guid]);

  useEffect(() => { cargar(); }, [cargar]);

  const onCancelar = async () => {
    Alert.alert('Cancelar reserva', '¿Estás seguro que deseas cancelar esta reserva?', [
      { text: 'No', style: 'cancel' },
      {
        text: 'Sí, cancelar', style: 'destructive', onPress: async () => {
          setCancelando(true);
          try {
            await cancelarReserva(String(guid));
            await cargar();
            Alert.alert('Reserva cancelada');
          } catch {
            Alert.alert('Error', 'No se pudo cancelar la reserva');
          } finally {
            setCancelando(false);
          }
        },
      },
    ]);
  };

  const onResenia = async () => {
    if (!comentario.trim()) { Alert.alert('Escribe un comentario'); return; }
    setEnviandoResenia(true);
    try {
      const atGuid = String(reserva?.at_guid ?? reserva?.atraccion_guid ?? '');
      await crearResenia(atGuid, { calificacion, comentario });
      setReseniaEnviada(true);
      Alert.alert('¡Gracias por tu reseña!');
    } catch {
      Alert.alert('Error', 'No se pudo enviar la reseña');
    } finally {
      setEnviandoResenia(false);
    }
  };

  if (cargando) return <Spinner texto="Cargando reserva..." />;
  if (!reserva) return (
    <View style={styles.errorBox}>
      <Text style={styles.errorText}>Reserva no encontrada</Text>
      <Button title="Volver" onPress={() => router.back()} variant="outline" style={{ marginTop: 16 }} />
    </View>
  );

  const estado = String(reserva.estado ?? 'P');
  const cancelable = esReservaCancelable(estado);
  const confirmada = esReservaConfirmada(estado);
  const atGuid = String(reserva.at_guid ?? reserva.atraccion_guid ?? '');

  const detalles = (reserva.detalles ?? reserva.lineas ?? []) as Record<string, unknown>[];

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      <ScrollView contentContainerStyle={styles.scroll}>
        {/* Encabezado */}
        <View style={styles.encabezado}>
          <Text style={styles.codigo}>{String(reserva.rev_codigo ?? '—')}</Text>
          <Badge estado={estado} />
        </View>

        {reserva.atraccion_nombre && (
          <Text style={styles.nombre}>{String(reserva.atraccion_nombre)}</Text>
        )}

        {/* Datos */}
        <View style={styles.card}>
          <InfoRow label="Fecha de visita" valor={formatearFechaCorta(String(reserva.fecha_visita ?? ''))} />
          <InfoRow label="Fecha de reserva" valor={formatearFechaCorta(String(reserva.fecha_creacion ?? reserva.created_at ?? ''))} />
          {reserva.canal_venta && <InfoRow label="Canal" valor={String(reserva.canal_venta)} />}
        </View>

        {/* Detalles */}
        {detalles.length > 0 && (
          <View style={styles.card}>
            <Text style={styles.seccion}>Tickets</Text>
            {detalles.map((d, i) => (
              <View key={i} style={styles.detalleRow}>
                <Text style={styles.detalleName}>{String(d.ticket_nombre ?? d.nombre ?? 'Ticket')}</Text>
                <Text style={styles.detalleCant}>{String(d.cantidad ?? 1)} × ${Number(d.precio_unitario ?? d.precio ?? 0).toFixed(2)}</Text>
              </View>
            ))}
            <View style={styles.separador} />
            <View style={styles.detalleRow}>
              <Text style={[styles.detalleName, { fontWeight: '700' }]}>Total</Text>
              <Text style={[styles.detalleCant, { color: Colors.primary, fontWeight: '700' }]}>${Number(reserva.total ?? 0).toFixed(2)}</Text>
            </View>
          </View>
        )}

        {/* Acciones */}
        {cancelable && (
          <Button title="Cancelar reserva" onPress={onCancelar} loading={cancelando} variant="danger" style={{ marginBottom: 12 }} />
        )}

        {/* Reseña */}
        {confirmada && atGuid && !reseniaEnviada && (
          <View style={styles.card}>
            <Text style={styles.seccion}>Deja tu reseña</Text>
            <View style={styles.estrellas}>
              {[1, 2, 3, 4, 5].map((n) => (
                <Text key={n} style={[styles.estrella, n <= calificacion && styles.estrellaActiva]}
                  onPress={() => setCalificacion(n)}>★</Text>
              ))}
            </View>
            <Input
              label="Comentario"
              value={comentario}
              onChangeText={setComentario}
              multiline
              numberOfLines={3}
              placeholder="Comparte tu experiencia..."
              style={{ height: 90, textAlignVertical: 'top' }}
            />
            <Button title="Enviar reseña" onPress={onResenia} loading={enviandoResenia} />
          </View>
        )}

        {reseniaEnviada && (
          <View style={styles.reseniaOk}>
            <Text style={styles.reseniaOkText}>✓ Reseña enviada. ¡Gracias!</Text>
          </View>
        )}
      </ScrollView>
    </SafeAreaView>
  );
}

function InfoRow({ label, valor }: { label: string; valor: string }) {
  return (
    <View style={rowStyles.row}>
      <Text style={rowStyles.label}>{label}</Text>
      <Text style={rowStyles.valor}>{valor}</Text>
    </View>
  );
}

const rowStyles = StyleSheet.create({
  row: { flexDirection: 'row', justifyContent: 'space-between', marginBottom: 10 },
  label: { color: Colors.textMuted, fontSize: 14 },
  valor: { color: Colors.text, fontSize: 14, fontWeight: '500', maxWidth: '60%', textAlign: 'right' },
});

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: Colors.background },
  scroll: { padding: 20 },
  encabezado: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: 8 },
  codigo: { color: Colors.text, fontSize: 20, fontWeight: '700' },
  nombre: { color: Colors.textMuted, fontSize: 15, marginBottom: 20 },
  card: { backgroundColor: Colors.surface, borderRadius: 16, padding: 16, marginBottom: 16 },
  seccion: { color: Colors.text, fontWeight: '700', fontSize: 16, marginBottom: 12 },
  detalleRow: { flexDirection: 'row', justifyContent: 'space-between', marginBottom: 8 },
  detalleName: { color: Colors.textMuted, fontSize: 14 },
  detalleCant: { color: Colors.text, fontSize: 14 },
  separador: { height: 1, backgroundColor: Colors.border, marginVertical: 8 },
  estrellas: { flexDirection: 'row', gap: 8, marginBottom: 14 },
  estrella: { fontSize: 32, color: Colors.border },
  estrellaActiva: { color: Colors.warning },
  reseniaOk: { backgroundColor: `${Colors.success}22`, borderRadius: 12, padding: 16, alignItems: 'center', borderWidth: 1, borderColor: Colors.success },
  reseniaOkText: { color: Colors.success, fontWeight: '600' },
  errorBox: { flex: 1, alignItems: 'center', justifyContent: 'center', padding: 40, backgroundColor: Colors.background },
  errorText: { color: Colors.danger, fontSize: 16 },
});
