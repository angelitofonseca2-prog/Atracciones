import React, { useCallback, useEffect, useState } from 'react';
import { Alert, KeyboardAvoidingView, Platform, ScrollView, StyleSheet, Text, TouchableOpacity, View } from 'react-native';
import { router, useLocalSearchParams } from 'expo-router';
import { SafeAreaView } from 'react-native-safe-area-context';
import Button from '@/components/ui/Button';
import Input from '@/components/ui/Input';
import Select from '@/components/ui/Select';
import Spinner from '@/components/ui/Spinner';
import CalendarioVisita from '@/components/reservas/CalendarioVisita';
import { obtenerAtraccion, obtenerHorariosDisponibles, obtenerTicketsAtraccion } from '@/lib/api/atraccionesApi';
import { crearReserva, confirmarPagoReserva } from '@/lib/api/reservasApi';
import { useAuth } from '@/lib/context/AuthContext';
import { Colors } from '@/constants/Colors';

const TIPOS_ID = [
  { label: 'Cédula', value: 'CEDULA' },
  { label: 'Pasaporte', value: 'PASAPORTE' },
  { label: 'RUC', value: 'RUC' },
  { label: 'Otro', value: 'OTRO' },
];

type Paso = 'horario' | 'tickets' | 'cliente' | 'pago' | 'confirmacion';

interface Ticket { tck_guid?: string; Id?: string; nombre?: string; Nombre?: string; precio?: number; Precio?: number; capacidad_disponible?: number; }
interface Horario { hor_guid?: string; Id?: string; fecha?: string; Fecha?: string; fecha_fin?: string; hora_inicio?: string; HoraInicio?: string; capacidad?: number; Capacidad?: number; capacidad_disponible?: number; }

export default function ReservarScreen() {
  const { guid } = useLocalSearchParams<{ guid: string }>();
  const { user } = useAuth();

  // Data
  const [atraccion, setAtraccion] = useState<Record<string, unknown> | null>(null);
  const [horarios, setHorarios] = useState<Horario[]>([]);
  const [tickets, setTickets] = useState<Ticket[]>([]);
  const [cargando, setCargando] = useState(true);

  // Selecciones
  const [horarioSel, setHorarioSel] = useState<Horario | null>(null);
  const [fechaSel, setFechaSel] = useState('');
  const [cantidades, setCantidades] = useState<Record<string, number>>({});

  // Datos cliente invitado
  const [datosCliente, setDatosCliente] = useState({
    nombres: '', apellidos: '', correo: '', telefono: '',
    tipo_identificacion: 'CEDULA', numero_identificacion: '',
  });

  // Estado flujo
  const [paso, setPaso] = useState<Paso>('horario');
  const [enviando, setEnviando] = useState(false);
  const [reservaCreada, setReservaCreada] = useState<Record<string, unknown> | null>(null);

  const cargar = useCallback(async () => {
    if (!guid) return;
    try {
      const [aRes, hRes, tRes] = await Promise.allSettled([
        obtenerAtraccion(guid),
        obtenerHorariosDisponibles(guid),
        obtenerTicketsAtraccion(guid),
      ]);
      if (aRes.status === 'fulfilled') setAtraccion(aRes.value?.data ?? aRes.value);
      if (hRes.status === 'fulfilled') {
        const d = hRes.value?.data ?? hRes.value;
        setHorarios(Array.isArray(d) ? d : d?.items ?? d?.horarios ?? []);
      }
      if (tRes.status === 'fulfilled') {
        const d = tRes.value?.data ?? tRes.value;
        setTickets(Array.isArray(d) ? d : d?.items ?? d?.tickets ?? []);
      }
    } catch {}
    finally { setCargando(false); }
  }, [guid]);

  useEffect(() => { cargar(); }, [cargar]);

  const horarioId = (h: Horario) => String(h.hor_guid ?? h.Id ?? '');
  const ticketId = (t: Ticket) => String(t.tck_guid ?? t.Id ?? '');
  const totalCant = Object.values(cantidades).reduce((a, b) => a + b, 0);
  const totalPrecio = tickets.reduce((sum, t) => {
    const qty = cantidades[ticketId(t)] ?? 0;
    return sum + qty * Number(t.precio ?? t.Precio ?? 0);
  }, 0);

  const lineas = tickets
    .filter((t) => (cantidades[ticketId(t)] ?? 0) > 0)
    .map((t) => ({ tck_guid: ticketId(t), cantidad: cantidades[ticketId(t)] }));

  const confirmarHorario = () => {
    if (!horarioSel) { Alert.alert('Selecciona un horario'); return; }
    if (!fechaSel) { Alert.alert('Selecciona una fecha de visita'); return; }
    setPaso('tickets');
  };

  const confirmarTickets = () => {
    if (totalCant === 0) { Alert.alert('Selecciona al menos un ticket'); return; }
    setPaso(user ? 'pago' : 'cliente');
  };

  const crearYConfirmar = async () => {
    setEnviando(true);
    try {
      const payload = {
        at_guid: String(guid),
        hor_guid: horarioId(horarioSel!),
        lineas,
        fecha_visita: fechaSel,
        origen_canal: 'MOBILE',
        ...(!user ? {
          cliente_invitado: {
            tipo_identificacion: datosCliente.tipo_identificacion,
            numero_identificacion: datosCliente.numero_identificacion,
            nombres: datosCliente.nombres,
            apellidos: datosCliente.apellidos,
            correo: datosCliente.correo,
            telefono: datosCliente.telefono || undefined,
          },
        } : {}),
      };
      const res = await crearReserva(payload);
      const rev = res?.data ?? res;
      setReservaCreada(rev);

      // Confirmar pago simulado
      const revGuid = String(rev?.rev_guid ?? rev?.RevGuid ?? '');
      if (revGuid) {
        await confirmarPagoReserva(revGuid);
      }
      setPaso('confirmacion');
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message
        ?? 'No se pudo completar la reserva. Inténtalo de nuevo.';
      Alert.alert('Error', msg);
    } finally {
      setEnviando(false);
    }
  };

  if (cargando) return <Spinner texto="Cargando información..." />;

  const nombre = String(atraccion?.nombre ?? atraccion?.Nombre ?? 'Atracción');

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      <KeyboardAvoidingView behavior={Platform.OS === 'ios' ? 'padding' : undefined} style={{ flex: 1 }}>
        {/* Indicador de paso */}
        <View style={styles.pasos}>
          {(['horario', 'tickets', 'pago', 'confirmacion'] as Paso[]).map((p, i) => (
            <View key={p} style={styles.pasoItem}>
              <View style={[styles.pasoCirculo, paso === p && styles.pasoActivo]}>
                <Text style={styles.pasoNum}>{i + 1}</Text>
              </View>
            </View>
          ))}
        </View>

        <ScrollView contentContainerStyle={styles.scroll} keyboardShouldPersistTaps="handled">
          <Text style={styles.titulo}>{nombre}</Text>

          {/* PASO 1 — Horario */}
          {paso === 'horario' && (
            <View>
              <Text style={styles.seccion}>Selecciona un horario</Text>
              {horarios.length === 0 ? (
                <Text style={styles.sinHorarios}>No hay horarios disponibles</Text>
              ) : (
                horarios.map((h) => {
                  const id = horarioId(h);
                  const activo = horarioSel && horarioId(horarioSel) === id;
                  return (
                    <TouchableOpacity key={id} style={[styles.horarioCard, activo && styles.horarioActivo]}
                      onPress={() => { setHorarioSel(h); setFechaSel(''); }} activeOpacity={0.8}>
                      <Text style={styles.horarioHora}>🕐 {String(h.hora_inicio ?? h.HoraInicio ?? 'Sin hora')}</Text>
                      <Text style={styles.horarioFecha}>
                        {String(h.fecha ?? h.Fecha ?? '').slice(0, 10)}
                        {h.fecha_fin ? ` — ${String(h.fecha_fin).slice(0, 10)}` : ''}
                      </Text>
                      <Text style={styles.horarioCupo}>
                        Disponibles: {h.capacidad_disponible ?? h.capacidad ?? h.Capacidad ?? '?'}
                      </Text>
                    </TouchableOpacity>
                  );
                })
              )}

              {horarioSel && (
                <>
                  <Text style={styles.seccion}>Selecciona fecha de visita</Text>
                  <CalendarioVisita
                    fechaInicio={String(horarioSel.fecha ?? horarioSel.Fecha ?? '')}
                    fechaFin={String(horarioSel.fecha_fin ?? horarioSel.fecha ?? horarioSel.Fecha ?? '')}
                    seleccionado={fechaSel}
                    onSeleccionar={setFechaSel}
                  />
                </>
              )}

              <Button title="Siguiente →" onPress={confirmarHorario} size="lg" disabled={!horarioSel || !fechaSel} />
            </View>
          )}

          {/* PASO 2 — Tickets */}
          {paso === 'tickets' && (
            <View>
              <Text style={styles.seccion}>Elige tus tickets</Text>
              {tickets.length === 0 ? (
                <Text style={styles.sinHorarios}>No hay tickets disponibles</Text>
              ) : (
                tickets.map((t) => {
                  const id = ticketId(t);
                  const cant = cantidades[id] ?? 0;
                  const max = Number(t.capacidad_disponible ?? horarioSel?.capacidad_disponible ?? horarioSel?.capacidad ?? 99);
                  return (
                    <View key={id} style={styles.ticketCard}>
                      <View style={styles.ticketInfo}>
                        <Text style={styles.ticketNombre}>{String(t.nombre ?? t.Nombre ?? 'Ticket')}</Text>
                        <Text style={styles.ticketPrecio}>${Number(t.precio ?? t.Precio ?? 0).toFixed(2)}</Text>
                      </View>
                      <View style={styles.counter}>
                        <TouchableOpacity style={styles.counterBtn} onPress={() => setCantidades((p) => ({ ...p, [id]: Math.max(0, (p[id] ?? 0) - 1) }))} disabled={cant === 0}>
                          <Text style={styles.counterBtnText}>−</Text>
                        </TouchableOpacity>
                        <Text style={styles.counterVal}>{cant}</Text>
                        <TouchableOpacity style={styles.counterBtn} onPress={() => setCantidades((p) => ({ ...p, [id]: Math.min(max, (p[id] ?? 0) + 1) }))} disabled={cant >= max}>
                          <Text style={styles.counterBtnText}>+</Text>
                        </TouchableOpacity>
                      </View>
                    </View>
                  );
                })
              )}

              {totalCant > 0 && (
                <View style={styles.totalBox}>
                  <Text style={styles.totalLabel}>Total ({totalCant} tickets)</Text>
                  <Text style={styles.totalValor}>${totalPrecio.toFixed(2)}</Text>
                </View>
              )}

              <View style={styles.botonesRow}>
                <Button title="← Volver" onPress={() => setPaso('horario')} variant="ghost" style={{ flex: 1 }} />
                <Button title="Siguiente →" onPress={confirmarTickets} disabled={totalCant === 0} style={{ flex: 2 }} />
              </View>
            </View>
          )}

          {/* PASO 2.5 — Datos cliente invitado */}
          {paso === 'cliente' && !user && (
            <View>
              <Text style={styles.seccion}>Tus datos de contacto</Text>
              <Input label="Nombres *" value={datosCliente.nombres} onChangeText={(v) => setDatosCliente((p) => ({ ...p, nombres: v }))} placeholder="Ej. Juan" />
              <Input label="Apellidos *" value={datosCliente.apellidos} onChangeText={(v) => setDatosCliente((p) => ({ ...p, apellidos: v }))} placeholder="Ej. Pérez" />
              <Input label="Correo *" value={datosCliente.correo} onChangeText={(v) => setDatosCliente((p) => ({ ...p, correo: v }))} keyboardType="email-address" autoCapitalize="none" />
              <Select label="Tipo ID *" value={datosCliente.tipo_identificacion} onChange={(v) => setDatosCliente((p) => ({ ...p, tipo_identificacion: v }))} options={TIPOS_ID} />
              <Input label="Número ID *" value={datosCliente.numero_identificacion} onChangeText={(v) => setDatosCliente((p) => ({ ...p, numero_identificacion: v }))} keyboardType="numeric" />
              <Input label="Teléfono" value={datosCliente.telefono} onChangeText={(v) => setDatosCliente((p) => ({ ...p, telefono: v }))} keyboardType="phone-pad" />
              <View style={styles.botonesRow}>
                <Button title="← Volver" onPress={() => setPaso('tickets')} variant="ghost" style={{ flex: 1 }} />
                <Button title="Continuar →" onPress={() => setPaso('pago')} style={{ flex: 2 }} />
              </View>
            </View>
          )}

          {/* PASO 3 — Resumen y pago */}
          {paso === 'pago' && (
            <View>
              <Text style={styles.seccion}>Resumen de tu reserva</Text>
              <View style={styles.resumenCard}>
                <InfoRow label="Atracción" valor={nombre} />
                <InfoRow label="Fecha" valor={fechaSel} />
                <InfoRow label="Horario" valor={String(horarioSel?.hora_inicio ?? horarioSel?.HoraInicio ?? '')} />
                {lineas.map((l) => {
                  const t = tickets.find((t) => ticketId(t) === l.tck_guid);
                  return <InfoRow key={l.tck_guid} label={String(t?.nombre ?? t?.Nombre ?? l.tck_guid)} valor={`${l.cantidad} × $${Number(t?.precio ?? t?.Precio ?? 0).toFixed(2)}`} />;
                })}
                <View style={styles.separador} />
                <InfoRow label="TOTAL" valor={`$${totalPrecio.toFixed(2)}`} importante />
              </View>

              <View style={styles.simuladoBox}>
                <Text style={styles.simuladoTitle}>💳 Pago simulado</Text>
                <Text style={styles.simuladoText}>Esta es una demostración. El pago se confirma automáticamente sin procesar dinero real.</Text>
              </View>

              <View style={styles.botonesRow}>
                <Button title="← Volver" onPress={() => setPaso(user ? 'tickets' : 'cliente')} variant="ghost" style={{ flex: 1 }} />
                <Button title="✓ Confirmar y pagar" onPress={crearYConfirmar} loading={enviando} style={{ flex: 2 }} />
              </View>
            </View>
          )}

          {/* PASO 4 — Confirmación */}
          {paso === 'confirmacion' && (
            <View style={styles.confirmBox}>
              <Text style={styles.confirmIcon}>🎉</Text>
              <Text style={styles.confirmTitle}>¡Reserva confirmada!</Text>
              <Text style={styles.confirmSub}>Tu reserva fue procesada exitosamente</Text>
              {reservaCreada?.rev_codigo && (
                <View style={styles.codigoBox}>
                  <Text style={styles.codigoLabel}>Código de reserva</Text>
                  <Text style={styles.codigo}>{String(reservaCreada.rev_codigo)}</Text>
                </View>
              )}
              <Button title="Ver mis reservas" onPress={() => router.replace('/mis-reservas')} size="lg" style={{ marginTop: 24 }} />
              <Button title="Ir al inicio" onPress={() => router.replace('/(tabs)')} variant="ghost" style={{ marginTop: 12 }} />
            </View>
          )}
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}

function InfoRow({ label, valor, importante }: { label: string; valor: string; importante?: boolean }) {
  return (
    <View style={rowStyles.row}>
      <Text style={rowStyles.label}>{label}</Text>
      <Text style={[rowStyles.valor, importante && rowStyles.valorImportante]}>{valor}</Text>
    </View>
  );
}

const rowStyles = StyleSheet.create({
  row: { flexDirection: 'row', justifyContent: 'space-between', marginBottom: 10 },
  label: { color: Colors.textMuted, fontSize: 14 },
  valor: { color: Colors.text, fontSize: 14, fontWeight: '500', maxWidth: '60%', textAlign: 'right' },
  valorImportante: { color: Colors.primary, fontSize: 16, fontWeight: '700' },
});

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: Colors.background },
  scroll: { padding: 20 },
  pasos: { flexDirection: 'row', justifyContent: 'center', gap: 16, padding: 12, backgroundColor: Colors.surface },
  pasoItem: { alignItems: 'center' },
  pasoCirculo: { width: 28, height: 28, borderRadius: 14, backgroundColor: Colors.border, alignItems: 'center', justifyContent: 'center' },
  pasoActivo: { backgroundColor: Colors.primary },
  pasoNum: { color: '#fff', fontWeight: '700', fontSize: 12 },
  titulo: { color: Colors.text, fontSize: 20, fontWeight: '700', marginBottom: 20 },
  seccion: { color: Colors.text, fontSize: 17, fontWeight: '700', marginBottom: 14, borderBottomWidth: 1, borderBottomColor: Colors.border, paddingBottom: 6 },
  sinHorarios: { color: Colors.textMuted, textAlign: 'center', marginVertical: 20 },
  horarioCard: { backgroundColor: Colors.surface, borderRadius: 12, padding: 16, marginBottom: 10, borderWidth: 1.5, borderColor: Colors.border },
  horarioActivo: { borderColor: Colors.primary, backgroundColor: `${Colors.primary}18` },
  horarioHora: { color: Colors.text, fontWeight: '700', fontSize: 16, marginBottom: 4 },
  horarioFecha: { color: Colors.textMuted, fontSize: 13, marginBottom: 4 },
  horarioCupo: { color: Colors.success, fontSize: 12 },
  ticketCard: { backgroundColor: Colors.surface, borderRadius: 12, padding: 14, marginBottom: 10, flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center' },
  ticketInfo: { flex: 1 },
  ticketNombre: { color: Colors.text, fontWeight: '600', fontSize: 15 },
  ticketPrecio: { color: Colors.primary, fontSize: 14, marginTop: 2 },
  counter: { flexDirection: 'row', alignItems: 'center', gap: 12 },
  counterBtn: { width: 36, height: 36, borderRadius: 18, backgroundColor: Colors.border, alignItems: 'center', justifyContent: 'center' },
  counterBtnText: { color: Colors.text, fontSize: 20, fontWeight: '700', lineHeight: 24 },
  counterVal: { color: Colors.text, fontSize: 18, fontWeight: '700', minWidth: 24, textAlign: 'center' },
  totalBox: { flexDirection: 'row', justifyContent: 'space-between', backgroundColor: `${Colors.primary}22`, borderRadius: 12, padding: 16, marginVertical: 12 },
  totalLabel: { color: Colors.text, fontWeight: '700' },
  totalValor: { color: Colors.primary, fontWeight: '700', fontSize: 18 },
  botonesRow: { flexDirection: 'row', gap: 10, marginTop: 8 },
  resumenCard: { backgroundColor: Colors.surface, borderRadius: 16, padding: 18, marginBottom: 16 },
  separador: { height: 1, backgroundColor: Colors.border, marginVertical: 10 },
  simuladoBox: { backgroundColor: `${Colors.warning}22`, borderRadius: 12, padding: 16, marginBottom: 16, borderWidth: 1, borderColor: Colors.warning },
  simuladoTitle: { color: Colors.warning, fontWeight: '700', marginBottom: 6 },
  simuladoText: { color: Colors.textMuted, fontSize: 13, lineHeight: 20 },
  confirmBox: { alignItems: 'center', paddingTop: 40 },
  confirmIcon: { fontSize: 72, marginBottom: 20 },
  confirmTitle: { color: Colors.text, fontSize: 26, fontWeight: '700', marginBottom: 10 },
  confirmSub: { color: Colors.textMuted, fontSize: 15, marginBottom: 24 },
  codigoBox: { backgroundColor: Colors.surface, borderRadius: 16, padding: 20, alignItems: 'center', width: '100%' },
  codigoLabel: { color: Colors.textMuted, fontSize: 13, marginBottom: 6 },
  codigo: { color: Colors.primary, fontSize: 22, fontWeight: '700', letterSpacing: 2 },
});
