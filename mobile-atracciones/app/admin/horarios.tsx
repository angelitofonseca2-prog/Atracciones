import React, { useCallback, useEffect, useState } from 'react';
import { Alert, FlatList, Modal, RefreshControl, ScrollView, StyleSheet, Text, TouchableOpacity, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import Button from '@/components/ui/Button';
import Input from '@/components/ui/Input';
import Spinner from '@/components/ui/Spinner';
import { actualizarHorario, crearHorario, listarHorariosAdmin } from '@/lib/api/adminApi';
import { formatearRangoFechas } from '@/lib/utils/formatFechas';
import { Colors } from '@/constants/Colors';

interface Horario { hor_guid?: string; at_guid?: string; fecha?: string; fecha_fin?: string; hora_inicio?: string; capacidad?: number; atraccion_nombre?: string; ticket_guid?: string; }

const FORM_VACIO = { at_guid: '', ticket_guid: '', fecha: '', fecha_fin: '', hora_inicio: '', capacidad: '' };

export default function AdminHorariosScreen() {
  const [items, setItems] = useState<Horario[]>([]);
  const [cargando, setCargando] = useState(true);
  const [refrescando, setRefrescando] = useState(false);
  const [modal, setModal] = useState(false);
  const [form, setForm] = useState(FORM_VACIO);
  const [editandoGuid, setEditandoGuid] = useState<string | null>(null);
  const [guardando, setGuardando] = useState(false);

  const cargar = useCallback(async () => {
    try {
      const res = await listarHorariosAdmin();
      const d = res?.data ?? res;
      setItems(Array.isArray(d) ? d : d?.items ?? d?.horarios ?? []);
    } catch { Alert.alert('Error', 'No se pudo cargar horarios'); }
    finally { setCargando(false); setRefrescando(false); }
  }, []);

  useEffect(() => { cargar(); }, [cargar]);

  const set = (k: string) => (v: string) => setForm((p) => ({ ...p, [k]: v }));

  const abrirEditar = (h: Horario) => {
    setForm({ at_guid: h.at_guid ?? '', ticket_guid: h.ticket_guid ?? '', fecha: h.fecha?.slice(0, 10) ?? '', fecha_fin: h.fecha_fin?.slice(0, 10) ?? '', hora_inicio: h.hora_inicio ?? '', capacidad: String(h.capacidad ?? '') });
    setEditandoGuid(h.hor_guid ?? null);
    setModal(true);
  };

  const onGuardar = async () => {
    if (!form.fecha || !form.hora_inicio) { Alert.alert('Completa fecha y hora'); return; }
    setGuardando(true);
    try {
      const payload = { ...form, capacidad: Number(form.capacidad) || 0 };
      if (editandoGuid) await actualizarHorario(editandoGuid, payload as Record<string, unknown>);
      else await crearHorario(payload as Record<string, unknown>);
      setModal(false);
      await cargar();
    } catch (err: unknown) {
      Alert.alert('Error', (err as { response?: { data?: { message?: string } } })?.response?.data?.message ?? 'Error al guardar');
    } finally { setGuardando(false); }
  };

  if (cargando) return <Spinner texto="Cargando horarios..." />;

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      <FlatList
        data={items}
        keyExtractor={(i) => String(i.hor_guid ?? Math.random())}
        contentContainerStyle={styles.list}
        refreshControl={<RefreshControl refreshing={refrescando} onRefresh={() => { setRefrescando(true); cargar(); }} tintColor={Colors.primary} />}
        renderItem={({ item: h }) => (
          <View style={styles.card}>
            <View style={styles.cardInfo}>
              {h.atraccion_nombre && <Text style={styles.atNombre}>{h.atraccion_nombre}</Text>}
              <Text style={styles.rango}>{formatearRangoFechas(h.fecha, h.fecha_fin)}</Text>
              <Text style={styles.hora}>🕐 {h.hora_inicio ?? '?'} · Cupos: {h.capacidad ?? '?'}</Text>
            </View>
            <TouchableOpacity onPress={() => abrirEditar(h)} style={styles.btnEdit}>
              <Text style={styles.btnEditText}>✎</Text>
            </TouchableOpacity>
          </View>
        )}
        ListHeaderComponent={
          <Button title="+ Nuevo Horario" onPress={() => { setForm(FORM_VACIO); setEditandoGuid(null); setModal(true); }} style={{ marginBottom: 16 }} />
        }
        ListEmptyComponent={<Text style={styles.empty}>No hay horarios</Text>}
      />

      <Modal visible={modal} animationType="slide" presentationStyle="pageSheet" onRequestClose={() => setModal(false)}>
        <SafeAreaView style={styles.modalSafe}>
          <View style={styles.modalHeader}>
            <Text style={styles.modalTitle}>{editandoGuid ? 'Editar' : 'Nuevo'} Horario</Text>
            <TouchableOpacity onPress={() => setModal(false)}><Text style={styles.cerrar}>✕</Text></TouchableOpacity>
          </View>
          <ScrollView contentContainerStyle={styles.modalScroll} keyboardShouldPersistTaps="handled">
            <Input label="Guid Atracción *" value={form.at_guid} onChangeText={set('at_guid')} placeholder="Guid de la atracción" />
            <Input label="Guid Ticket *" value={form.ticket_guid} onChangeText={set('ticket_guid')} placeholder="Guid del ticket" />
            <Input label="Fecha inicio * (YYYY-MM-DD)" value={form.fecha} onChangeText={set('fecha')} placeholder="2026-07-01" keyboardType="numeric" />
            <Input label="Fecha fin (YYYY-MM-DD)" value={form.fecha_fin} onChangeText={set('fecha_fin')} placeholder="2026-07-31" keyboardType="numeric" />
            <Input label="Hora inicio *" value={form.hora_inicio} onChangeText={set('hora_inicio')} placeholder="08:00" />
            <Input label="Capacidad total" value={form.capacidad} onChangeText={set('capacidad')} keyboardType="numeric" placeholder="50" />
            <Button title={editandoGuid ? 'Guardar cambios' : 'Crear horario'} onPress={onGuardar} loading={guardando} size="lg" />
          </ScrollView>
        </SafeAreaView>
      </Modal>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: Colors.background },
  list: { padding: 16 },
  card: { backgroundColor: Colors.surface, borderRadius: 14, padding: 14, marginBottom: 10, flexDirection: 'row', alignItems: 'center' },
  cardInfo: { flex: 1 },
  atNombre: { color: Colors.text, fontWeight: '700', fontSize: 14, marginBottom: 2 },
  rango: { color: Colors.primary, fontSize: 13, marginBottom: 2 },
  hora: { color: Colors.textMuted, fontSize: 13 },
  btnEdit: { width: 36, height: 36, borderRadius: 8, backgroundColor: `${Colors.primary}33`, alignItems: 'center', justifyContent: 'center' },
  btnEditText: { color: Colors.primary, fontSize: 18 },
  empty: { color: Colors.textMuted, textAlign: 'center', marginTop: 40 },
  modalSafe: { flex: 1, backgroundColor: Colors.background },
  modalHeader: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', padding: 20, borderBottomWidth: 1, borderBottomColor: Colors.border },
  modalTitle: { color: Colors.text, fontSize: 18, fontWeight: '700' },
  cerrar: { color: Colors.textMuted, fontSize: 20, padding: 4 },
  modalScroll: { padding: 20 },
});
