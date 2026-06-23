import React, { useCallback, useEffect, useState } from 'react';
import {
  Alert, KeyboardAvoidingView, Platform, ScrollView,
  StyleSheet, Text, TouchableOpacity, View,
} from 'react-native';
import { router, useLocalSearchParams } from 'expo-router';
import { SafeAreaView } from 'react-native-safe-area-context';
import Button from '@/components/ui/Button';
import Input from '@/components/ui/Input';
import Spinner from '@/components/ui/Spinner';
import CalendarioVisita from '@/components/reservas/CalendarioVisita';
import {
  obtenerAtraccion,
  obtenerHorariosDisponibles,
  obtenerTicketsPorHorario,
} from '@/lib/api/atraccionesApi';
import { crearReserva, confirmarPagoReserva, DatosReceptor } from '@/lib/api/reservasApi';
import { obtenerPerfilCliente } from '@/lib/api/clientesApi';
import { useAuth } from '@/lib/context/AuthContext';
import { Colors } from '@/constants/Colors';

type Paso = 'horario' | 'tickets' | 'facturacion' | 'confirmacion';

interface Ticket { tck_guid: string; titulo?: string; tipo?: string; precio: number }
interface Horario {
  hor_guid: string; fecha: string; fecha_fin?: string;
  hora_inicio?: string; hora_fin?: string;
  cupos?: number; cupos_disponibles?: number;
}

export default function ReservarScreen() {
  const { guid } = useLocalSearchParams<{ guid: string }>();
  const { user } = useAuth();

  const [atraccion, setAtraccion] = useState<Record<string, unknown> | null>(null);
  const [horarios, setHorarios] = useState<Horario[]>([]);
  const [tickets, setTickets] = useState<Ticket[]>([]);
  const [cargando, setCargando] = useState(true);
  const [cargandoTickets, setCargandoTickets] = useState(false);

  const [horarioSel, setHorarioSel] = useState<Horario | null>(null);
  const [fechaSel, setFechaSel] = useState('');
  const [cantidades, setCantidades] = useState<Record<string, number>>({});

  const [form, setForm] = useState({
    nombre_receptor: '',
    apellido_receptor: '',
    correo_receptor: '',
    telefono_receptor: '',
    observacion: '',
  });
  const [formErrores, setFormErrores] = useState<Record<string, string>>({});

  const [paso, setPaso] = useState<Paso>('horario');
  const [enviando, setEnviando] = useState(false);
  const [reservaCreada, setReservaCreada] = useState<Record<string, unknown> | null>(null);
  const [factura, setFactura] = useState<Record<string, unknown> | null>(null);

  // Requerir login
  useEffect(() => {
    if (!user) {
      Alert.alert(
        'Inicio de sesión requerido',
        'Debes iniciar sesión para hacer una reserva.',
        [
          { text: 'Ir al login', onPress: () => router.replace('/auth/login') },
          { text: 'Cancelar', onPress: () => router.back() },
        ],
      );
    }
  }, [user]);

  const cargarDatos = useCallback(async () => {
    if (!guid) return;
    try {
      const [aRes, hRes] = await Promise.allSettled([
        obtenerAtraccion(guid),
        obtenerHorariosDisponibles(guid),
      ]);
      if (aRes.status === 'fulfilled') {
        const raw = aRes.value as Record<string, unknown>;
        setAtraccion((raw?.data as Record<string, unknown>) ?? raw);
      }
      if (hRes.status === 'fulfilled') {
        const raw = hRes.value as Record<string, unknown>;
        const d = raw?.data ?? raw;
        setHorarios((Array.isArray(d) ? d : []).map((h: Horario) => ({
          ...h,
          cupos_disponibles: h.cupos_disponibles ?? h.cupos,
          fecha_fin: h.fecha_fin ?? h.fecha,
        })));
      }
    } finally {
      setCargando(false);
    }
  }, [guid]);

  useEffect(() => { cargarDatos(); }, [cargarDatos]);

  // Precargar datos desde perfil real del cliente (no del JWT)
  useEffect(() => {
    if (!user) return;
    obtenerPerfilCliente()
      .then((res) => {
        const raw = res as Record<string, unknown>;
        const d = (raw?.data as Record<string, unknown>) ?? raw;
        setForm((p) => ({
          ...p,
          nombre_receptor: p.nombre_receptor || String(d?.nombres ?? d?.nombre ?? ''),
          apellido_receptor: p.apellido_receptor || String(d?.apellidos ?? d?.apellido ?? ''),
          correo_receptor: p.correo_receptor || String(d?.correo ?? user.correo ?? ''),
          telefono_receptor: p.telefono_receptor || String(d?.telefono ?? ''),
        }));
      })
      .catch(() => {
        // Fallback: solo el correo del JWT
        setForm((p) => ({
          ...p,
          correo_receptor: p.correo_receptor || user.correo || '',
        }));
      });
  }, [user]);

  const onSeleccionarHorario = async (h: Horario) => {
    setHorarioSel(h);
    setFechaSel('');
    setCantidades({});
    setTickets([]);
    if (!guid) return;
    setCargandoTickets(true);
    try {
      const arr = await obtenerTicketsPorHorario(guid, h.hor_guid);
      setTickets(Array.isArray(arr) ? arr : []);
    } catch {
      // Fallback a tickets generales de la atraccion
      try {
        const raw = await (await import('@/lib/api/atraccionesApi')).obtenerTicketsAtraccion(guid);
        const d = (raw as Record<string, unknown>)?.data ?? raw;
        setTickets(Array.isArray(d) ? d : []);
      } catch { setTickets([]); }
    } finally {
      setCargandoTickets(false);
    }
  };

  const cuposMax = horarioSel?.cupos_disponibles ?? horarioSel?.cupos ?? 999;
  const lineas = Object.entries(cantidades)
    .filter(([, c]) => c > 0)
    .map(([tck_guid, cantidad]) => ({ tck_guid, cantidad }));
  const subtotal = tickets.reduce((s, t) => s + (cantidades[t.tck_guid] ?? 0) * t.precio, 0);
  const iva = subtotal * 0.15;
  const total = subtotal + iva;
  const totalEntradas = lineas.reduce((s, l) => s + l.cantidad, 0);

  const ticketLabel = (t: Ticket) => t.titulo || t.tipo || 'Entrada';

  const validarForm = () => {
    const e: Record<string, string> = {};
    if (!form.nombre_receptor.trim()) e.nombre_receptor = 'El nombre es obligatorio';
    if (!form.correo_receptor.trim()) e.correo_receptor = 'El correo es obligatorio';
    else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.correo_receptor)) e.correo_receptor = 'Correo inválido';
    return e;
  };

  const onConfirmarFacturacion = async () => {
    const e = validarForm();
    if (Object.keys(e).length) { setFormErrores(e); return; }
    setEnviando(true);
    try {
      const reservaRes = await crearReserva({
        at_guid: String(guid),
        hor_guid: horarioSel!.hor_guid,
        lineas,
        fecha_visita: fechaSel || undefined,
        origen_canal: 'MOBILE',
      });
      const reservaRaw = (reservaRes as Record<string, unknown>)?.data ?? reservaRes;
      const rev = reservaRaw as Record<string, unknown>;
      setReservaCreada(rev);

      const revGuid = String(rev?.rev_guid ?? '');
      if (!revGuid) throw new Error('No se recibió identificador de reserva.');

      const datos: DatosReceptor = {
        nombre_receptor: form.nombre_receptor.trim(),
        correo_receptor: form.correo_receptor.trim(),
        ...(form.apellido_receptor.trim() ? { apellido_receptor: form.apellido_receptor.trim() } : {}),
        ...(form.telefono_receptor.trim() ? { telefono_receptor: form.telefono_receptor.trim() } : {}),
        ...(form.observacion.trim() ? { observacion: form.observacion.trim() } : {}),
      };
      const pagoRes = await confirmarPagoReserva(revGuid, datos);
      const facturaData = (pagoRes as Record<string, unknown>)?.data ?? pagoRes;
      setFactura(facturaData as Record<string, unknown>);
      setPaso('confirmacion');
    } catch (err: unknown) {
      const status = (err as { response?: { status?: number } })?.response?.status;
      let msg = (err as { response?: { data?: { message?: string; details?: string[] } } })
        ?.response?.data?.message
        ?? (err as Error)?.message
        ?? 'No se pudo completar la reserva.';
      if (status === 409 || /cupo|capacidad|sin lugar|agotad/i.test(msg)) {
        msg = 'No quedan cupos disponibles para este horario. Por favor elige otro.';
      }
      Alert.alert('Error', msg);
    } finally {
      setEnviando(false);
    }
  };

  if (cargando) return <Spinner texto="Cargando información..." />;
  if (!user) return null;

  const nombre = String(atraccion?.nombre ?? 'Atracción');

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      <KeyboardAvoidingView behavior={Platform.OS === 'ios' ? 'padding' : undefined} style={{ flex: 1 }}>
        {/* Barra de pasos */}
        <View style={styles.pasosBar}>
          {(['horario', 'tickets', 'facturacion', 'confirmacion'] as Paso[]).map((p, i) => (
            <View key={p} style={styles.pasoItem}>
              <View style={[styles.pasoCirculo, paso === p && styles.pasoActivo,
                (['horario', 'tickets', 'facturacion', 'confirmacion'] as Paso[]).indexOf(paso) > i && styles.pasoHecho]}>
                <Text style={styles.pasoNum}>{i + 1}</Text>
              </View>
              <Text style={[styles.pasoLabel, paso === p && styles.pasoLabelActivo]}>
                {['Horario', 'Tickets', 'Pago', 'Listo'][i]}
              </Text>
            </View>
          ))}
        </View>

        <ScrollView contentContainerStyle={styles.scroll} keyboardShouldPersistTaps="handled">
          <Text style={styles.titulo}>{nombre}</Text>

          {/* ── PASO 1: Horario ── */}
          {paso === 'horario' && (
            <View>
              <Text style={styles.seccion}>Selecciona un horario</Text>
              {horarios.length === 0 ? (
                <View style={styles.emptyBox}>
                  <Text style={styles.emptyText}>No hay horarios disponibles en este momento.</Text>
                </View>
              ) : (
                horarios.map((h) => {
                  const activo = horarioSel?.hor_guid === h.hor_guid;
                  const cupos = h.cupos_disponibles ?? h.cupos ?? 0;
                  const sinCupos = cupos <= 0;
                  return (
                    <TouchableOpacity
                      key={h.hor_guid}
                      style={[styles.horarioCard, activo && styles.horarioActivo, sinCupos && styles.horarioSinCupos]}
                      onPress={() => !sinCupos && onSeleccionarHorario(h)}
                      activeOpacity={sinCupos ? 1 : 0.8}
                    >
                      <Text style={styles.horarioHora}>🕐 {h.hora_inicio ?? '—'}{h.hora_fin ? ` – ${h.hora_fin}` : ''}</Text>
                      <Text style={styles.horarioFecha}>
                        {h.fecha?.slice(0, 10)}{h.fecha_fin && h.fecha_fin !== h.fecha ? ` → ${h.fecha_fin.slice(0, 10)}` : ''}
                      </Text>
                      <Text style={[styles.horarioCupo, sinCupos && { color: Colors.danger }]}>
                        {sinCupos ? '❌ Sin cupos' : `✅ ${cupos} cupos disponibles`}
                      </Text>
                    </TouchableOpacity>
                  );
                })
              )}

              {horarioSel && (
                <>
                  <Text style={[styles.seccion, { marginTop: 20 }]}>Elige el día de tu visita</Text>
                  <Text style={styles.rangoTexto}>
                    Rango del horario: {horarioSel.fecha?.slice(0, 10)} → {(horarioSel.fecha_fin ?? horarioSel.fecha)?.slice(0, 10)}
                  </Text>
                  <CalendarioVisita
                    fechaInicio={horarioSel.fecha}
                    fechaFin={horarioSel.fecha_fin ?? horarioSel.fecha}
                    seleccionado={fechaSel}
                    onSeleccionar={setFechaSel}
                  />
                </>
              )}

              <Button
                title="Siguiente"
                onPress={() => {
                  if (!horarioSel) { Alert.alert('Selecciona un horario'); return; }
                  if (!fechaSel) { Alert.alert('Selecciona el día de tu visita'); return; }
                  setPaso('tickets');
                }}
                size="lg"
                disabled={!horarioSel || !fechaSel}
                style={{ marginTop: 16 }}
              />
            </View>
          )}

          {/* ── PASO 2: Tickets ── */}
          {paso === 'tickets' && (
            <View>
              <Text style={styles.seccion}>Elige tus entradas</Text>
              <Text style={styles.rangoTexto}>Visita: {fechaSel} · Horario: {horarioSel?.hora_inicio}</Text>

              {cargandoTickets ? (
                <Spinner texto="Cargando entradas..." />
              ) : tickets.length === 0 ? (
                <Text style={styles.emptyText}>No hay información de tarifas disponible.</Text>
              ) : (
                tickets.map((t) => {
                  const cant = cantidades[t.tck_guid] ?? 0;
                  const otrasEntradas = lineas.filter((l) => l.tck_guid !== t.tck_guid).reduce((s, l) => s + l.cantidad, 0);
                  const maxPerm = Math.max(0, cuposMax - otrasEntradas);
                  return (
                    <View key={t.tck_guid} style={styles.ticketCard}>
                      <View style={styles.ticketInfo}>
                        <Text style={styles.ticketNombre}>{ticketLabel(t)}</Text>
                        <Text style={styles.ticketPrecio}>${Number(t.precio).toFixed(2)} / persona</Text>
                      </View>
                      <View style={styles.counter}>
                        <TouchableOpacity
                          style={[styles.counterBtn, cant === 0 && styles.counterBtnDisabled]}
                          onPress={() => setCantidades((p) => ({ ...p, [t.tck_guid]: Math.max(0, cant - 1) }))}
                          disabled={cant === 0}
                        >
                          <Text style={styles.counterBtnText}>−</Text>
                        </TouchableOpacity>
                        <Text style={styles.counterVal}>{cant}</Text>
                        <TouchableOpacity
                          style={[styles.counterBtn, cant >= maxPerm && styles.counterBtnDisabled]}
                          onPress={() => setCantidades((p) => ({ ...p, [t.tck_guid]: Math.min(maxPerm, cant + 1) }))}
                          disabled={cant >= maxPerm}
                        >
                          <Text style={styles.counterBtnText}>+</Text>
                        </TouchableOpacity>
                      </View>
                    </View>
                  );
                })
              )}

              {totalEntradas > 0 && (
                <View style={styles.totalBox}>
                  <View style={styles.totalRow}>
                    <Text style={styles.totalLabel}>Subtotal ({totalEntradas} entradas)</Text>
                    <Text style={styles.totalVal}>${subtotal.toFixed(2)}</Text>
                  </View>
                  <View style={styles.totalRow}>
                    <Text style={styles.totalLabel}>IVA 15%</Text>
                    <Text style={styles.totalVal}>${iva.toFixed(2)}</Text>
                  </View>
                  <View style={[styles.totalRow, { marginTop: 4 }]}>
                    <Text style={[styles.totalLabel, { fontWeight: '700', color: Colors.text }]}>Total</Text>
                    <Text style={[styles.totalVal, { fontWeight: '700', fontSize: 18, color: Colors.primary }]}>${total.toFixed(2)}</Text>
                  </View>
                </View>
              )}

              <View style={styles.botonesRow}>
                <Button title="Volver" onPress={() => setPaso('horario')} variant="ghost" style={{ flex: 1 }} />
                <Button
                  title="Continuar"
                  onPress={() => {
                    if (lineas.length === 0) { Alert.alert('Selecciona al menos una entrada'); return; }
                    setPaso('facturacion');
                  }}
                  disabled={lineas.length === 0}
                  style={{ flex: 2 }}
                />
              </View>
            </View>
          )}

          {/* ── PASO 3: Datos facturación + pago simulado ── */}
          {paso === 'facturacion' && (
            <View>
              <Text style={styles.seccion}>Resumen y datos de pago</Text>

              {/* Resumen */}
              <View style={styles.resumenCard}>
                <InfoRow label="Atracción" valor={nombre} />
                <InfoRow label="Fecha de visita" valor={fechaSel} />
                <InfoRow label="Horario" valor={horarioSel?.hora_inicio ?? ''} />
                {lineas.map((l) => {
                  const t = tickets.find((t) => t.tck_guid === l.tck_guid);
                  return (
                    <InfoRow
                      key={l.tck_guid}
                      label={ticketLabel(t ?? { tck_guid: l.tck_guid, precio: 0 })}
                      valor={`${l.cantidad} × $${Number(t?.precio ?? 0).toFixed(2)}`}
                    />
                  );
                })}
                <View style={styles.separador} />
                <InfoRow label="Subtotal" valor={`$${subtotal.toFixed(2)}`} />
                <InfoRow label="IVA 15%" valor={`$${iva.toFixed(2)}`} />
                <InfoRow label="TOTAL" valor={`$${total.toFixed(2)}`} importante />
              </View>

              {/* Datos de facturación */}
              <Text style={[styles.seccion, { marginTop: 8 }]}>Datos de facturación</Text>
              <View style={styles.simuladoBox}>
                <Text style={styles.simuladoTitle}>💳 Pago simulado</Text>
                <Text style={styles.simuladoText}>
                  Esta es una demostración. El pago se confirma automáticamente sin procesar dinero real.
                </Text>
              </View>

              <Input
                label="Nombre *"
                value={form.nombre_receptor}
                onChangeText={(v) => { setForm((p) => ({ ...p, nombre_receptor: v })); setFormErrores((p) => ({ ...p, nombre_receptor: '' })); }}
                placeholder="Tu nombre"
                error={formErrores.nombre_receptor}
              />
              <Input
                label="Apellido"
                value={form.apellido_receptor}
                onChangeText={(v) => setForm((p) => ({ ...p, apellido_receptor: v }))}
                placeholder="Tu apellido"
              />
              <Input
                label="Correo electrónico *"
                value={form.correo_receptor}
                onChangeText={(v) => { setForm((p) => ({ ...p, correo_receptor: v })); setFormErrores((p) => ({ ...p, correo_receptor: '' })); }}
                keyboardType="email-address"
                autoCapitalize="none"
                placeholder="correo@ejemplo.com"
                error={formErrores.correo_receptor}
              />
              <Input
                label="Teléfono"
                value={form.telefono_receptor}
                onChangeText={(v) => setForm((p) => ({ ...p, telefono_receptor: v }))}
                keyboardType="phone-pad"
                placeholder="0991234567"
              />

              <View style={styles.botonesRow}>
                <Button title="Volver" onPress={() => setPaso('tickets')} variant="ghost" style={{ flex: 1 }} />
                <Button
                  title="Confirmar y pagar"
                  onPress={onConfirmarFacturacion}
                  loading={enviando}
                  style={{ flex: 2 }}
                />
              </View>
            </View>
          )}

          {/* ── PASO 4: Confirmación ── */}
          {paso === 'confirmacion' && (
            <View style={styles.confirmBox}>
              <Text style={styles.confirmIcon}>🎉</Text>
              <Text style={styles.confirmTitle}>¡Pago confirmado!</Text>
              <Text style={styles.confirmSub}>Tu reserva fue procesada exitosamente</Text>

              <View style={styles.resumenCard}>
                {factura?.fac_numero && <InfoRow label="N° Factura" valor={String(factura.fac_numero)} />}
                {(reservaCreada?.rev_codigo ?? factura?.rev_codigo) && (
                  <InfoRow label="Código reserva" valor={String(reservaCreada?.rev_codigo ?? factura?.rev_codigo)} />
                )}
                {factura?.total != null && (
                  <InfoRow label="Total pagado" valor={`$${Number(factura.total).toFixed(2)} USD`} importante />
                )}
                {factura?.estado && <InfoRow label="Estado" valor={String(factura.estado)} />}
              </View>

              <Button
                title="Ver mis reservas"
                onPress={() => router.replace('/mis-reservas')}
                size="lg"
                style={{ marginTop: 20 }}
              />
              <Button
                title="Ver mis facturas"
                onPress={() => router.replace('/mis-facturas')}
                variant="outline"
                style={{ marginTop: 10 }}
              />
              <Button
                title="Ir al inicio"
                onPress={() => router.replace('/(tabs)')}
                variant="ghost"
                style={{ marginTop: 8 }}
              />
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
      <Text style={[rowStyles.valor, importante && rowStyles.importante]}>{valor}</Text>
    </View>
  );
}

const rowStyles = StyleSheet.create({
  row: { flexDirection: 'row', justifyContent: 'space-between', marginBottom: 8, flexWrap: 'wrap', gap: 4 },
  label: { color: Colors.textMuted, fontSize: 14, flex: 1 },
  valor: { color: Colors.text, fontSize: 14, fontWeight: '500', maxWidth: '55%', textAlign: 'right' },
  importante: { color: Colors.primary, fontSize: 16, fontWeight: '700' },
});

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: Colors.background },
  pasosBar: { flexDirection: 'row', justifyContent: 'center', paddingVertical: 12, gap: 16, backgroundColor: Colors.surface, borderBottomWidth: 1, borderBottomColor: Colors.border },
  pasoItem: { alignItems: 'center', gap: 4 },
  pasoCirculo: { width: 28, height: 28, borderRadius: 14, backgroundColor: Colors.border, alignItems: 'center', justifyContent: 'center' },
  pasoActivo: { backgroundColor: Colors.primary },
  pasoHecho: { backgroundColor: Colors.success },
  pasoNum: { color: '#fff', fontWeight: '700', fontSize: 12 },
  pasoLabel: { color: Colors.textMuted, fontSize: 10 },
  pasoLabelActivo: { color: Colors.primary, fontWeight: '700' },
  scroll: { padding: 20 },
  titulo: { color: Colors.text, fontSize: 20, fontWeight: '700', marginBottom: 20 },
  seccion: { color: Colors.text, fontSize: 17, fontWeight: '700', marginBottom: 14, borderBottomWidth: 1, borderBottomColor: Colors.border, paddingBottom: 6 },
  emptyBox: { backgroundColor: Colors.surface, borderRadius: 12, padding: 20, alignItems: 'center', marginBottom: 16 },
  emptyText: { color: Colors.textMuted, textAlign: 'center' },
  horarioCard: { backgroundColor: Colors.surface, borderRadius: 12, padding: 16, marginBottom: 10, borderWidth: 1.5, borderColor: Colors.border },
  horarioActivo: { borderColor: Colors.primary, backgroundColor: `${Colors.primary}18` },
  horarioSinCupos: { opacity: 0.5 },
  horarioHora: { color: Colors.text, fontWeight: '700', fontSize: 16, marginBottom: 4 },
  horarioFecha: { color: Colors.textMuted, fontSize: 13, marginBottom: 4 },
  horarioCupo: { color: Colors.success, fontSize: 13 },
  rangoTexto: { color: Colors.textMuted, fontSize: 13, marginBottom: 12 },
  ticketCard: { backgroundColor: Colors.surface, borderRadius: 12, padding: 14, marginBottom: 10, flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center' },
  ticketInfo: { flex: 1 },
  ticketNombre: { color: Colors.text, fontWeight: '600', fontSize: 15 },
  ticketPrecio: { color: Colors.primary, fontSize: 13, marginTop: 2 },
  counter: { flexDirection: 'row', alignItems: 'center', gap: 10 },
  counterBtn: { width: 36, height: 36, borderRadius: 18, backgroundColor: Colors.primary, alignItems: 'center', justifyContent: 'center' },
  counterBtnDisabled: { backgroundColor: Colors.border },
  counterBtnText: { color: '#fff', fontSize: 20, fontWeight: '700', lineHeight: 24 },
  counterVal: { color: Colors.text, fontSize: 18, fontWeight: '700', minWidth: 24, textAlign: 'center' },
  totalBox: { backgroundColor: Colors.surface, borderRadius: 12, padding: 16, marginVertical: 12 },
  totalRow: { flexDirection: 'row', justifyContent: 'space-between', marginBottom: 6 },
  totalLabel: { color: Colors.textMuted, fontSize: 14 },
  totalVal: { color: Colors.text, fontSize: 14 },
  resumenCard: { backgroundColor: Colors.surface, borderRadius: 16, padding: 18, marginBottom: 16 },
  separador: { height: 1, backgroundColor: Colors.border, marginVertical: 10 },
  simuladoBox: { backgroundColor: `${Colors.warning}22`, borderRadius: 12, padding: 14, marginBottom: 16, borderWidth: 1, borderColor: Colors.warning },
  simuladoTitle: { color: Colors.warning, fontWeight: '700', marginBottom: 4 },
  simuladoText: { color: Colors.textMuted, fontSize: 13, lineHeight: 20 },
  botonesRow: { flexDirection: 'row', gap: 10, marginTop: 12 },
  confirmBox: { alignItems: 'center', paddingTop: 24 },
  confirmIcon: { fontSize: 72, marginBottom: 16 },
  confirmTitle: { color: Colors.text, fontSize: 24, fontWeight: '700', marginBottom: 8 },
  confirmSub: { color: Colors.textMuted, fontSize: 15, marginBottom: 24 },
});
