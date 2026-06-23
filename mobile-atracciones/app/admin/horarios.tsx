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
  actualizarHorario, crearHorario,
  listarHorariosAdmin, listarTicketsDeAtraccion, listarTodasAtraccionesAdmin,
} from '@/lib/api/adminApi';
import { Colors } from '@/constants/Colors';

interface HorarioAdmin {
  hor_guid?: string; id?: string;
  tck_guid?: string; at_guid?: string;
  fecha?: string; fecha_fin?: string;
  hora_inicio?: string; hora_fin?: string;
  cupos_disponibles?: number;
  atraccion_nombre?: string;
  ticket_titulo?: string;
}

interface OpcionSelect { value: string; label: string }

interface FormState {
  at_guid: string;
  tck_guid: string;
  fecha: string;
  fecha_fin: string;
  hora_inicio: string;
  hora_fin: string;
  cupos_disponibles: string;
}

const FORM_VACIO: FormState = {
  at_guid: '', tck_guid: '', fecha: '', fecha_fin: '',
  hora_inicio: '', hora_fin: '', cupos_disponibles: '',
};

/** Normaliza 'HH:mm' o 'H:mm' a 'HH:mm:ss' */
function toHHmmss(hora: string): string {
  const t = hora.trim();
  if (!t) return '';
  const m = t.match(/^(\d{1,2}):(\d{2})(?::(\d{2}))?$/);
  if (!m) return t;
  const h = m[1].padStart(2, '0');
  const min = m[2];
  const sec = m[3] ?? '00';
  return `${h}:${min}:${sec}`;
}

const hoyIso = () => new Date().toISOString().slice(0, 10);

export default function AdminHorariosScreen() {
  const [items, setItems] = useState<HorarioAdmin[]>([]);
  const [atracciones, setAtracciones] = useState<OpcionSelect[]>([]);
  const [tickets, setTickets] = useState<OpcionSelect[]>([]);
  const [cargando, setCargando] = useState(true);
  const [refrescando, setRefrescando] = useState(false);
  const [modal, setModal] = useState(false);
  const [form, setForm] = useState<FormState>({ ...FORM_VACIO, fecha: hoyIso() });
  const [errores, setErrores] = useState<Record<string, string>>({});
  const [editandoGuid, setEditandoGuid] = useState<string | null>(null);
  const [guardando, setGuardando] = useState(false);
  const [cargandoTickets, setCargandoTickets] = useState(false);

  const cargar = useCallback(async () => {
    try {
      const [hRes, aRes] = await Promise.allSettled([listarHorariosAdmin(), listarTodasAtraccionesAdmin()]);
      if (hRes.status === 'fulfilled') {
        const raw = hRes.value as Record<string, unknown>;
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
    } catch { Alert.alert('Error', 'No se pudo cargar horarios'); }
    finally { setCargando(false); setRefrescando(false); }
  }, []);

  useEffect(() => { cargar(); }, [cargar]);

  const cargarTicketsDe = async (atGuid: string) => {
    if (!atGuid) { setTickets([]); return; }
    setCargandoTickets(true);
    try {
      const res = await listarTicketsDeAtraccion(atGuid);
      const raw = res as Record<string, unknown>;
      const d = raw?.data ?? raw;
      setTickets(
        (Array.isArray(d) ? d : []).map((t: unknown) => {
          const x = t as Record<string, unknown>;
          return {
            value: String(x.tck_guid ?? x.id ?? ''),
            label: String(x.titulo ?? x.nombre ?? ''),
          };
        }).filter((o) => o.value),
      );
    } catch { setTickets([]); }
    finally { setCargandoTickets(false); }
  };

  const set = (k: keyof FormState) => (v: string) =>
    setForm((p) => ({ ...p, [k]: v }));

  const onAtraccionChange = (v: string) => {
    setForm((p) => ({ ...p, at_guid: v, tck_guid: '' }));
    setErrores((p) => ({ ...p, at_guid: '', tck_guid: '' }));
    setTickets([]);
    cargarTicketsDe(v);
  };

  const abrirCrear = () => {
    setForm({ ...FORM_VACIO, fecha: hoyIso() });
    setErrores({});
    setEditandoGuid(null);
    setTickets([]);
    setModal(true);
  };

  const abrirEditar = async (h: HorarioAdmin) => {
    setForm({
      at_guid: h.at_guid ?? '',
      tck_guid: h.tck_guid ?? '',
      fecha: h.fecha?.slice(0, 10) ?? hoyIso(),
      fecha_fin: h.fecha_fin?.slice(0, 10) ?? '',
      hora_inicio: h.hora_inicio ?? '',
      hora_fin: h.hora_fin ?? '',
      cupos_disponibles: String(h.cupos_disponibles ?? ''),
    });
    setErrores({});
    setEditandoGuid(h.hor_guid ?? h.id ?? null);
    setModal(true);
    if (h.at_guid) await cargarTicketsDe(h.at_guid);
  };

  const validar = () => {
    const e: Record<string, string> = {};
    if (!editandoGuid && !form.tck_guid) {
      if (!form.at_guid) e.at_guid = 'Selecciona la atraccion';
      else e.tck_guid = 'Selecciona el ticket';
    }
    if (!form.fecha) e.fecha = 'La fecha es obligatoria';
    const horaRegex = /^\d{1,2}:\d{2}(:\d{2})?$/;
    if (!form.hora_inicio.trim() || !horaRegex.test(form.hora_inicio.trim()))
      e.hora_inicio = 'Formato HH:mm o HH:mm:ss';
    if (form.hora_fin && !horaRegex.test(form.hora_fin.trim()))
      e.hora_fin = 'Formato HH:mm o HH:mm:ss';
    if (!form.cupos_disponibles || Number(form.cupos_disponibles) < 0)
      e.cupos_disponibles = 'Ingresa los cupos disponibles';
    return e;
  };

  const onGuardar = async () => {
    const e = validar();
    if (Object.keys(e).length) { setErrores(e); return; }
    setGuardando(true);
    try {
      const payload: Record<string, unknown> = {
        fecha: form.fecha,
        hora_inicio: toHHmmss(form.hora_inicio),
        cupos_disponibles: Number(form.cupos_disponibles),
      };
      if (!editandoGuid) payload.tck_guid = form.tck_guid;
      if (form.fecha_fin.trim()) payload.fecha_fin = form.fecha_fin.trim();
      if (form.hora_fin.trim()) payload.hora_fin = toHHmmss(form.hora_fin);

      if (editandoGuid) await actualizarHorario(editandoGuid, payload);
      else await crearHorario(payload);
      setModal(false);
      await cargar();
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message
        ?? (err as Error)?.message ?? 'Error al guardar';
      setErrores((p) => ({ ...p, _global: msg }));
    } finally { setGuardando(false); }
  };

  if (cargando) return <Spinner texto="Cargando horarios..." />;

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      <FlatList
        data={items}
        keyExtractor={(h) => String(h.hor_guid ?? h.id ?? Math.random())}
        contentContainerStyle={styles.list}
        refreshControl={<RefreshControl refreshing={refrescando} onRefresh={() => { setRefrescando(true); cargar(); }} tintColor={Colors.primary} />}
        renderItem={({ item: h }) => (
          <View style={styles.card}>
            <View style={styles.cardInfo}>
              {h.atraccion_nombre && <Text style={styles.atraccion}>{h.atraccion_nombre}</Text>}
              {h.ticket_titulo && <Text style={styles.ticket}>{h.ticket_titulo}</Text>}
              <Text style={styles.fecha}>{h.fecha?.slice(0, 10)}{h.fecha_fin ? ` → ${h.fecha_fin.slice(0, 10)}` : ''}</Text>
              <Text style={styles.hora}>{h.hora_inicio}{h.hora_fin ? ` — ${h.hora_fin}` : ''}</Text>
              <Text style={styles.cupos}>Cupos: {h.cupos_disponibles ?? '?'}</Text>
            </View>
            <TouchableOpacity onPress={() => abrirEditar(h)} style={styles.btnEdit}>
              <Text style={styles.btnEditText}>✎</Text>
            </TouchableOpacity>
          </View>
        )}
        ListHeaderComponent={<Button title="+ Nuevo Horario" onPress={abrirCrear} style={{ marginBottom: 16 }} />}
        ListEmptyComponent={<Text style={styles.empty}>No hay horarios</Text>}
      />

      <Modal visible={modal} animationType="slide" presentationStyle="pageSheet" onRequestClose={() => setModal(false)}>
        <SafeAreaView style={styles.modalSafe}>
          <View style={styles.modalHeader}>
            <Text style={styles.modalTitle}>{editandoGuid ? 'Editar' : 'Nuevo'} Horario</Text>
            <TouchableOpacity onPress={() => setModal(false)}><Text style={styles.cerrar}>✕</Text></TouchableOpacity>
          </View>
          <ScrollView contentContainerStyle={styles.modalScroll} keyboardShouldPersistTaps="handled">
            {!editandoGuid && (
              <>
                <Select
                  label="Atraccion *"
                  value={form.at_guid}
                  onChange={onAtraccionChange}
                  options={atracciones}
                  placeholder="Selecciona la atraccion"
                  error={errores.at_guid}
                />
                {cargandoTickets ? (
                  <Text style={styles.hint}>Cargando tickets...</Text>
                ) : (
                  <Select
                    label="Ticket *"
                    value={form.tck_guid}
                    onChange={(v) => { setForm((p) => ({ ...p, tck_guid: v })); setErrores((p) => ({ ...p, tck_guid: '' })); }}
                    options={tickets}
                    placeholder={form.at_guid ? 'Selecciona el ticket' : 'Primero selecciona atraccion'}
                    error={errores.tck_guid}
                  />
                )}
                {form.at_guid && tickets.length === 0 && !cargandoTickets && (
                  <Text style={styles.hint}>Esta atraccion no tiene tickets. Crea uno primero.</Text>
                )}
              </>
            )}

            <Input label="Fecha inicio *" value={form.fecha} onChangeText={(v) => { set('fecha')(v); setErrores((p) => ({ ...p, fecha: '' })); }} placeholder="YYYY-MM-DD" error={errores.fecha} />
            <Input label="Fecha fin (opcional)" value={form.fecha_fin} onChangeText={set('fecha_fin')} placeholder="YYYY-MM-DD" />
            <Input label="Hora inicio * (HH:mm)" value={form.hora_inicio} onChangeText={(v) => { set('hora_inicio')(v); setErrores((p) => ({ ...p, hora_inicio: '' })); }} placeholder="09:00" keyboardType="numbers-and-punctuation" error={errores.hora_inicio} />
            <Input label="Hora fin (HH:mm)" value={form.hora_fin} onChangeText={(v) => { set('hora_fin')(v); setErrores((p) => ({ ...p, hora_fin: '' })); }} placeholder="11:00" keyboardType="numbers-and-punctuation" error={errores.hora_fin} />
            <Input label="Cupos disponibles *" value={form.cupos_disponibles} onChangeText={(v) => { set('cupos_disponibles')(v); setErrores((p) => ({ ...p, cupos_disponibles: '' })); }} keyboardType="numeric" placeholder="20" error={errores.cupos_disponibles} />
            {errores._global && <Text style={styles.errorText}>{errores._global}</Text>}
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
  atraccion: { color: Colors.text, fontWeight: '700', fontSize: 15, marginBottom: 2 },
  ticket: { color: Colors.primary, fontSize: 13, marginBottom: 2 },
  fecha: { color: Colors.textMuted, fontSize: 13 },
  hora: { color: Colors.text, fontSize: 13 },
  cupos: { color: Colors.textMuted, fontSize: 12, marginTop: 2 },
  btnEdit: { width: 36, height: 36, borderRadius: 8, backgroundColor: `${Colors.primary}33`, alignItems: 'center', justifyContent: 'center' },
  btnEditText: { color: Colors.primary, fontSize: 18 },
  empty: { color: Colors.textMuted, textAlign: 'center', marginTop: 40 },
  modalSafe: { flex: 1, backgroundColor: Colors.background },
  modalHeader: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', padding: 20, borderBottomWidth: 1, borderBottomColor: Colors.border },
  modalTitle: { color: Colors.text, fontSize: 18, fontWeight: '700' },
  cerrar: { color: Colors.textMuted, fontSize: 20, padding: 4 },
  modalScroll: { padding: 20 },
  hint: { color: Colors.textMuted, fontSize: 12, fontStyle: 'italic', marginBottom: 12 },
  errorText: { color: Colors.danger, fontSize: 13, marginBottom: 8 },
});
