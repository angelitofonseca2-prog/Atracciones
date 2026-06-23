import React, { useCallback, useEffect, useState } from 'react';
import { Alert, FlatList, Modal, RefreshControl, ScrollView, StyleSheet, Text, TouchableOpacity, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import Button from '@/components/ui/Button';
import Input from '@/components/ui/Input';
import Spinner from '@/components/ui/Spinner';
import { actualizarAtraccion, crearAtraccion, eliminarAtraccion, listarAtraccionesAdmin } from '@/lib/api/adminApi';
import { Colors } from '@/constants/Colors';

interface AtraccionAdmin { at_guid?: string; nombre?: string; ciudad?: string; precio_desde?: number; estado?: string; }

const FORM_VACIO = { nombre: '', ciudad: '', pais: '', descripcion_corta: '', descripcion: '', duracion_horas: '', precio_desde: '' };

export default function AdminAtraccionesScreen() {
  const [items, setItems] = useState<AtraccionAdmin[]>([]);
  const [cargando, setCargando] = useState(true);
  const [refrescando, setRefrescando] = useState(false);
  const [modal, setModal] = useState(false);
  const [form, setForm] = useState(FORM_VACIO);
  const [editandoGuid, setEditandoGuid] = useState<string | null>(null);
  const [guardando, setGuardando] = useState(false);

  const cargar = useCallback(async () => {
    try {
      const res = await listarAtraccionesAdmin();
      const d = res?.data ?? res;
      setItems(Array.isArray(d) ? d : d?.items ?? d?.atracciones ?? []);
    } catch { Alert.alert('Error', 'No se pudo cargar atracciones'); }
    finally { setCargando(false); setRefrescando(false); }
  }, []);

  useEffect(() => { cargar(); }, [cargar]);

  const set = (k: string) => (v: string) => setForm((p) => ({ ...p, [k]: v }));

  const abrirCrear = () => { setForm(FORM_VACIO); setEditandoGuid(null); setModal(true); };
  const abrirEditar = (a: AtraccionAdmin) => {
    setForm({ nombre: a.nombre ?? '', ciudad: a.ciudad ?? '', pais: '', descripcion_corta: '', descripcion: '', duracion_horas: '', precio_desde: String(a.precio_desde ?? '') });
    setEditandoGuid(a.at_guid ?? null);
    setModal(true);
  };

  const onGuardar = async () => {
    if (!form.nombre.trim()) { Alert.alert('El nombre es requerido'); return; }
    setGuardando(true);
    try {
      const payload = { ...form, precio_desde: Number(form.precio_desde) || 0 };
      if (editandoGuid) await actualizarAtraccion(editandoGuid, payload as Record<string, unknown>);
      else await crearAtraccion(payload as Record<string, unknown>);
      setModal(false);
      await cargar();
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message ?? 'Error al guardar';
      Alert.alert('Error', msg);
    } finally { setGuardando(false); }
  };

  const onEliminar = (a: AtraccionAdmin) => {
    Alert.alert('Eliminar', `¿Eliminar "${a.nombre}"?`, [
      { text: 'Cancelar', style: 'cancel' },
      {
        text: 'Eliminar', style: 'destructive', onPress: async () => {
          try { await eliminarAtraccion(a.at_guid!); await cargar(); }
          catch { Alert.alert('Error', 'No se pudo eliminar'); }
        },
      },
    ]);
  };

  if (cargando) return <Spinner texto="Cargando atracciones..." />;

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      <FlatList
        data={items}
        keyExtractor={(i) => String(i.at_guid ?? Math.random())}
        contentContainerStyle={styles.list}
        refreshControl={<RefreshControl refreshing={refrescando} onRefresh={() => { setRefrescando(true); cargar(); }} tintColor={Colors.primary} />}
        renderItem={({ item: a }) => (
          <View style={styles.card}>
            <View style={styles.cardInfo}>
              <Text style={styles.nombre}>{a.nombre}</Text>
              <Text style={styles.sub}>{[a.ciudad].filter(Boolean).join(', ')}</Text>
              {a.precio_desde != null && <Text style={styles.precio}>Desde ${Number(a.precio_desde).toFixed(2)}</Text>}
            </View>
            <View style={styles.acciones}>
              <TouchableOpacity onPress={() => abrirEditar(a)} style={styles.btnEdit}><Text style={styles.btnEditText}>✎</Text></TouchableOpacity>
              <TouchableOpacity onPress={() => onEliminar(a)} style={styles.btnDel}><Text style={styles.btnDelText}>✕</Text></TouchableOpacity>
            </View>
          </View>
        )}
        ListHeaderComponent={
          <Button title="+ Nueva Atracción" onPress={abrirCrear} style={{ marginBottom: 16 }} />
        }
        ListEmptyComponent={<Text style={styles.empty}>No hay atracciones</Text>}
      />

      <Modal visible={modal} animationType="slide" presentationStyle="pageSheet" onRequestClose={() => setModal(false)}>
        <SafeAreaView style={styles.modalSafe}>
          <View style={styles.modalHeader}>
            <Text style={styles.modalTitle}>{editandoGuid ? 'Editar' : 'Nueva'} Atracción</Text>
            <TouchableOpacity onPress={() => setModal(false)}><Text style={styles.cerrar}>✕</Text></TouchableOpacity>
          </View>
          <ScrollView contentContainerStyle={styles.modalScroll} keyboardShouldPersistTaps="handled">
            <Input label="Nombre *" value={form.nombre} onChangeText={set('nombre')} placeholder="Ej. Galápagos Tour" />
            <Input label="Ciudad" value={form.ciudad} onChangeText={set('ciudad')} placeholder="Ej. Quito" />
            <Input label="País" value={form.pais} onChangeText={set('pais')} placeholder="Ej. Ecuador" />
            <Input label="Precio desde ($)" value={form.precio_desde} onChangeText={set('precio_desde')} keyboardType="numeric" placeholder="0.00" />
            <Input label="Duración (horas)" value={form.duracion_horas} onChangeText={set('duracion_horas')} keyboardType="numeric" placeholder="2" />
            <Input label="Descripción corta" value={form.descripcion_corta} onChangeText={set('descripcion_corta')} multiline numberOfLines={2} style={{ height: 70, textAlignVertical: 'top' }} />
            <Input label="Descripción completa" value={form.descripcion} onChangeText={set('descripcion')} multiline numberOfLines={4} style={{ height: 110, textAlignVertical: 'top' }} />
            <Button title={editandoGuid ? 'Guardar cambios' : 'Crear atracción'} onPress={onGuardar} loading={guardando} size="lg" />
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
  precio: { color: Colors.primary, fontSize: 12, marginTop: 2 },
  acciones: { flexDirection: 'row', gap: 8 },
  btnEdit: { width: 36, height: 36, borderRadius: 8, backgroundColor: `${Colors.primary}33`, alignItems: 'center', justifyContent: 'center' },
  btnEditText: { color: Colors.primary, fontSize: 18 },
  btnDel: { width: 36, height: 36, borderRadius: 8, backgroundColor: `${Colors.danger}33`, alignItems: 'center', justifyContent: 'center' },
  btnDelText: { color: Colors.danger, fontSize: 16 },
  empty: { color: Colors.textMuted, textAlign: 'center', marginTop: 40 },
  modalSafe: { flex: 1, backgroundColor: Colors.background },
  modalHeader: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', padding: 20, borderBottomWidth: 1, borderBottomColor: Colors.border },
  modalTitle: { color: Colors.text, fontSize: 18, fontWeight: '700' },
  cerrar: { color: Colors.textMuted, fontSize: 20, padding: 4 },
  modalScroll: { padding: 20 },
});
