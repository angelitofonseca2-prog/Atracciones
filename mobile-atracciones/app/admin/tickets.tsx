import React, { useCallback, useEffect, useState } from 'react';
import { Alert, FlatList, Modal, RefreshControl, ScrollView, StyleSheet, Text, TouchableOpacity, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import Button from '@/components/ui/Button';
import Input from '@/components/ui/Input';
import Spinner from '@/components/ui/Spinner';
import { actualizarTicket, crearTicket, listarAtraccionesAdmin, listarTicketsAdmin } from '@/lib/api/adminApi';
import { Colors } from '@/constants/Colors';

interface Ticket { tck_guid?: string; nombre?: string; precio?: number; at_guid?: string; atraccion_nombre?: string; }

const FORM_VACIO = { nombre: '', precio: '', at_guid: '', descripcion: '' };

export default function AdminTicketsScreen() {
  const [items, setItems] = useState<Ticket[]>([]);
  const [atracciones, setAtracciones] = useState<{ value: string; label: string }[]>([]);
  const [cargando, setCargando] = useState(true);
  const [refrescando, setRefrescando] = useState(false);
  const [modal, setModal] = useState(false);
  const [form, setForm] = useState(FORM_VACIO);
  const [editandoGuid, setEditandoGuid] = useState<string | null>(null);
  const [guardando, setGuardando] = useState(false);

  const cargar = useCallback(async () => {
    try {
      const [tRes, aRes] = await Promise.allSettled([listarTicketsAdmin(), listarAtraccionesAdmin()]);
      if (tRes.status === 'fulfilled') {
        const d = tRes.value?.data ?? tRes.value;
        setItems(Array.isArray(d) ? d : d?.items ?? d?.tickets ?? []);
      }
      if (aRes.status === 'fulfilled') {
        const d = aRes.value?.data ?? aRes.value;
        const arr = Array.isArray(d) ? d : d?.items ?? d?.atracciones ?? [];
        setAtracciones(arr.map((a: Record<string, unknown>) => ({ value: String(a.at_guid ?? a.Id ?? ''), label: String(a.nombre ?? a.Nombre ?? '') })));
      }
    } catch { Alert.alert('Error', 'No se pudo cargar'); }
    finally { setCargando(false); setRefrescando(false); }
  }, []);

  useEffect(() => { cargar(); }, [cargar]);

  const set = (k: string) => (v: string) => setForm((p) => ({ ...p, [k]: v }));

  const abrirEditar = (t: Ticket) => {
    setForm({ nombre: t.nombre ?? '', precio: String(t.precio ?? ''), at_guid: t.at_guid ?? '', descripcion: '' });
    setEditandoGuid(t.tck_guid ?? null);
    setModal(true);
  };

  const onGuardar = async () => {
    if (!form.nombre.trim()) { Alert.alert('El nombre es requerido'); return; }
    setGuardando(true);
    try {
      const payload = { ...form, precio: Number(form.precio) || 0 };
      if (editandoGuid) await actualizarTicket(editandoGuid, payload as Record<string, unknown>);
      else await crearTicket(payload as Record<string, unknown>);
      setModal(false);
      await cargar();
    } catch (err: unknown) {
      Alert.alert('Error', (err as { response?: { data?: { message?: string } } })?.response?.data?.message ?? 'Error al guardar');
    } finally { setGuardando(false); }
  };

  if (cargando) return <Spinner texto="Cargando tickets..." />;

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      <FlatList
        data={items}
        keyExtractor={(i) => String(i.tck_guid ?? Math.random())}
        contentContainerStyle={styles.list}
        refreshControl={<RefreshControl refreshing={refrescando} onRefresh={() => { setRefrescando(true); cargar(); }} tintColor={Colors.primary} />}
        renderItem={({ item: t }) => (
          <View style={styles.card}>
            <View style={styles.cardInfo}>
              <Text style={styles.nombre}>{t.nombre}</Text>
              {t.atraccion_nombre && <Text style={styles.sub}>{t.atraccion_nombre}</Text>}
              <Text style={styles.precio}>${Number(t.precio ?? 0).toFixed(2)}</Text>
            </View>
            <TouchableOpacity onPress={() => abrirEditar(t)} style={styles.btnEdit}>
              <Text style={styles.btnEditText}>✎</Text>
            </TouchableOpacity>
          </View>
        )}
        ListHeaderComponent={
          <Button title="+ Nuevo Ticket" onPress={() => { setForm(FORM_VACIO); setEditandoGuid(null); setModal(true); }} style={{ marginBottom: 16 }} />
        }
        ListEmptyComponent={<Text style={styles.empty}>No hay tickets</Text>}
      />

      <Modal visible={modal} animationType="slide" presentationStyle="pageSheet" onRequestClose={() => setModal(false)}>
        <SafeAreaView style={styles.modalSafe}>
          <View style={styles.modalHeader}>
            <Text style={styles.modalTitle}>{editandoGuid ? 'Editar' : 'Nuevo'} Ticket</Text>
            <TouchableOpacity onPress={() => setModal(false)}><Text style={styles.cerrar}>✕</Text></TouchableOpacity>
          </View>
          <ScrollView contentContainerStyle={styles.modalScroll} keyboardShouldPersistTaps="handled">
            <Input label="Nombre *" value={form.nombre} onChangeText={set('nombre')} placeholder="Ej. Adulto" />
            <Input label="Precio ($) *" value={form.precio} onChangeText={set('precio')} keyboardType="numeric" placeholder="25.00" />
            <Input label="Guid de Atracción" value={form.at_guid} onChangeText={set('at_guid')} placeholder="Guid de la atracción" />
            <Input label="Descripción" value={form.descripcion} onChangeText={set('descripcion')} multiline numberOfLines={2} style={{ height: 70, textAlignVertical: 'top' }} />
            <Button title={editandoGuid ? 'Guardar cambios' : 'Crear ticket'} onPress={onGuardar} loading={guardando} size="lg" />
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
  nombre: { color: Colors.text, fontWeight: '700', fontSize: 15, marginBottom: 2 },
  sub: { color: Colors.textMuted, fontSize: 13 },
  precio: { color: Colors.primary, fontSize: 13, marginTop: 2 },
  btnEdit: { width: 36, height: 36, borderRadius: 8, backgroundColor: `${Colors.primary}33`, alignItems: 'center', justifyContent: 'center' },
  btnEditText: { color: Colors.primary, fontSize: 18 },
  empty: { color: Colors.textMuted, textAlign: 'center', marginTop: 40 },
  modalSafe: { flex: 1, backgroundColor: Colors.background },
  modalHeader: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', padding: 20, borderBottomWidth: 1, borderBottomColor: Colors.border },
  modalTitle: { color: Colors.text, fontSize: 18, fontWeight: '700' },
  cerrar: { color: Colors.textMuted, fontSize: 20, padding: 4 },
  modalScroll: { padding: 20 },
});
