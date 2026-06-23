import React, { useCallback, useEffect, useState } from 'react';
import {
  Alert, FlatList, Modal, RefreshControl,
  ScrollView, StyleSheet, Text, TouchableOpacity, View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import Button from '@/components/ui/Button';
import Input from '@/components/ui/Input';
import Select from '@/components/ui/Select';
import Spinner from '@/components/ui/Spinner';
import {
  actualizarTicket, crearTicket,
  listarTicketsAdmin, listarTodasAtraccionesAdmin,
} from '@/lib/api/adminApi';
import { Colors } from '@/constants/Colors';

interface TicketAdmin {
  tck_guid?: string; id?: string;
  titulo?: string; nombre?: string;
  tipo_participante?: string;
  precio?: number;
  capacidad_maxima?: number;
  cupos_disponibles?: number;
  at_guid?: string;
  atraccion_nombre?: string;
}

interface OpcionSelect { value: string; label: string }

const TIPOS: OpcionSelect[] = [
  { value: 'Adulto', label: 'Adulto' },
  { value: 'Niño', label: 'Niño' },
  { value: 'Grupo', label: 'Grupo' },
  { value: 'Estudiante', label: 'Estudiante' },
  { value: 'Senior', label: 'Senior' },
];

interface FormState {
  at_guid: string;
  titulo: string;
  tipo_participante: string;
  precio: string;
  capacidad_maxima: string;
  cupos_disponibles: string;
}

const FORM_VACIO: FormState = {
  at_guid: '', titulo: '', tipo_participante: 'Adulto',
  precio: '', capacidad_maxima: '', cupos_disponibles: '',
};

export default function AdminTicketsScreen() {
  const [items, setItems] = useState<TicketAdmin[]>([]);
  const [atracciones, setAtracciones] = useState<OpcionSelect[]>([]);
  const [cargando, setCargando] = useState(true);
  const [refrescando, setRefrescando] = useState(false);
  const [modal, setModal] = useState(false);
  const [form, setForm] = useState<FormState>(FORM_VACIO);
  const [errores, setErrores] = useState<Record<string, string>>({});
  const [editandoGuid, setEditandoGuid] = useState<string | null>(null);
  const [guardando, setGuardando] = useState(false);

  const cargar = useCallback(async () => {
    try {
      const [tRes, aRes] = await Promise.allSettled([listarTicketsAdmin(), listarTodasAtraccionesAdmin()]);
      if (tRes.status === 'fulfilled') {
        const raw = tRes.value as Record<string, unknown>;
        const d = raw?.data ?? raw;
        setItems(Array.isArray(d) ? d : []);
      }
      if (aRes.status === 'fulfilled') {
        setAtracciones(
          aRes.value.map((a: Record<string, unknown>) => ({
            value: String(a.at_guid ?? a.id ?? ''),
            label: String(a.nombre ?? ''),
          })).filter((o) => o.value),
        );
      }
    } catch { Alert.alert('Error', 'No se pudo cargar tickets'); }
    finally { setCargando(false); setRefrescando(false); }
  }, []);

  useEffect(() => { cargar(); }, [cargar]);

  const set = (k: keyof FormState) => (v: string) =>
    setForm((p) => ({ ...p, [k]: v }));

  const abrirCrear = () => {
    setForm(FORM_VACIO);
    setErrores({});
    setEditandoGuid(null);
    setModal(true);
  };

  const abrirEditar = (t: TicketAdmin) => {
    setForm({
      at_guid: t.at_guid ?? '',
      titulo: t.titulo ?? t.nombre ?? '',
      tipo_participante: t.tipo_participante ?? 'Adulto',
      precio: String(t.precio ?? ''),
      capacidad_maxima: String(t.capacidad_maxima ?? ''),
      cupos_disponibles: String(t.cupos_disponibles ?? ''),
    });
    setErrores({});
    setEditandoGuid(t.tck_guid ?? t.id ?? null);
    setModal(true);
  };

  const validar = () => {
    const e: Record<string, string> = {};
    if (!editandoGuid && !form.at_guid) e.at_guid = 'Selecciona la atraccion';
    if (!form.titulo.trim()) e.titulo = 'El titulo es obligatorio';
    if (!form.tipo_participante) e.tipo_participante = 'Selecciona el tipo';
    if (!form.precio || Number(form.precio) <= 0) e.precio = 'El precio debe ser mayor a 0';
    if (!form.capacidad_maxima || Number(form.capacidad_maxima) <= 0) e.capacidad_maxima = 'Ingresa la capacidad maxima';
    if (!form.cupos_disponibles || Number(form.cupos_disponibles) < 0) e.cupos_disponibles = 'Ingresa los cupos disponibles';
    return e;
  };

  const onGuardar = async () => {
    const e = validar();
    if (Object.keys(e).length) { setErrores(e); return; }
    setGuardando(true);
    try {
      const payload: Record<string, unknown> = {
        titulo: form.titulo.trim(),
        tipo_participante: form.tipo_participante,
        precio: Number(form.precio),
        capacidad_maxima: Number(form.capacidad_maxima),
        cupos_disponibles: Number(form.cupos_disponibles),
      };
      if (!editandoGuid) payload.at_guid = form.at_guid;

      if (editandoGuid) await actualizarTicket(editandoGuid, payload);
      else await crearTicket(payload);
      setModal(false);
      await cargar();
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message
        ?? (err as Error)?.message ?? 'Error al guardar';
      setErrores((p) => ({ ...p, _global: msg }));
    } finally { setGuardando(false); }
  };

  if (cargando) return <Spinner texto="Cargando tickets..." />;

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      <FlatList
        data={items}
        keyExtractor={(t) => String(t.tck_guid ?? t.id ?? Math.random())}
        contentContainerStyle={styles.list}
        refreshControl={<RefreshControl refreshing={refrescando} onRefresh={() => { setRefrescando(true); cargar(); }} tintColor={Colors.primary} />}
        renderItem={({ item: t }) => (
          <View style={styles.card}>
            <View style={styles.cardInfo}>
              <Text style={styles.nombre}>{t.titulo ?? t.nombre}</Text>
              {t.atraccion_nombre && <Text style={styles.sub}>{t.atraccion_nombre}</Text>}
              <Text style={styles.tipo}>{t.tipo_participante} — ${Number(t.precio ?? 0).toFixed(2)}</Text>
              <Text style={styles.cupos}>Cupos: {t.cupos_disponibles ?? '?'} / {t.capacidad_maxima ?? '?'}</Text>
            </View>
            <TouchableOpacity onPress={() => abrirEditar(t)} style={styles.btnEdit}>
              <Text style={styles.btnEditText}>✎</Text>
            </TouchableOpacity>
          </View>
        )}
        ListHeaderComponent={<Button title="+ Nuevo Ticket" onPress={abrirCrear} style={{ marginBottom: 16 }} />}
        ListEmptyComponent={<Text style={styles.empty}>No hay tickets</Text>}
      />

      <Modal visible={modal} animationType="slide" presentationStyle="pageSheet" onRequestClose={() => setModal(false)}>
        <SafeAreaView style={styles.modalSafe}>
          <View style={styles.modalHeader}>
            <Text style={styles.modalTitle}>{editandoGuid ? 'Editar' : 'Nuevo'} Ticket</Text>
            <TouchableOpacity onPress={() => setModal(false)}><Text style={styles.cerrar}>✕</Text></TouchableOpacity>
          </View>
          <ScrollView contentContainerStyle={styles.modalScroll} keyboardShouldPersistTaps="handled">
            {!editandoGuid && (
              <Select
                label="Atraccion *"
                value={form.at_guid}
                onChange={(v) => { setForm((p) => ({ ...p, at_guid: v })); setErrores((p) => ({ ...p, at_guid: '' })); }}
                options={atracciones}
                placeholder="Selecciona la atraccion"
                error={errores.at_guid}
              />
            )}
            <Input label="Titulo *" value={form.titulo} onChangeText={(v) => { set('titulo')(v); setErrores((p) => ({ ...p, titulo: '' })); }} placeholder="Ej. Adulto general" error={errores.titulo} />
            <Select
              label="Tipo de participante *"
              value={form.tipo_participante}
              onChange={(v) => { setForm((p) => ({ ...p, tipo_participante: v })); setErrores((p) => ({ ...p, tipo_participante: '' })); }}
              options={TIPOS}
              placeholder="Selecciona tipo"
              error={errores.tipo_participante}
            />
            <Input label="Precio ($) *" value={form.precio} onChangeText={(v) => { set('precio')(v); setErrores((p) => ({ ...p, precio: '' })); }} keyboardType="numeric" placeholder="25.00" error={errores.precio} />
            <Input label="Capacidad maxima *" value={form.capacidad_maxima} onChangeText={(v) => { set('capacidad_maxima')(v); setErrores((p) => ({ ...p, capacidad_maxima: '' })); }} keyboardType="numeric" placeholder="50" error={errores.capacidad_maxima} />
            <Input label="Cupos disponibles *" value={form.cupos_disponibles} onChangeText={(v) => { set('cupos_disponibles')(v); setErrores((p) => ({ ...p, cupos_disponibles: '' })); }} keyboardType="numeric" placeholder="50" error={errores.cupos_disponibles} />
            {errores._global && <Text style={styles.errorText}>{errores._global}</Text>}
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
  tipo: { color: Colors.primary, fontSize: 13, marginTop: 2 },
  cupos: { color: Colors.textMuted, fontSize: 12 },
  btnEdit: { width: 36, height: 36, borderRadius: 8, backgroundColor: `${Colors.primary}33`, alignItems: 'center', justifyContent: 'center' },
  btnEditText: { color: Colors.primary, fontSize: 18 },
  empty: { color: Colors.textMuted, textAlign: 'center', marginTop: 40 },
  modalSafe: { flex: 1, backgroundColor: Colors.background },
  modalHeader: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', padding: 20, borderBottomWidth: 1, borderBottomColor: Colors.border },
  modalTitle: { color: Colors.text, fontSize: 18, fontWeight: '700' },
  cerrar: { color: Colors.textMuted, fontSize: 20, padding: 4 },
  modalScroll: { padding: 20 },
  errorText: { color: Colors.danger, fontSize: 13, marginBottom: 8 },
});
