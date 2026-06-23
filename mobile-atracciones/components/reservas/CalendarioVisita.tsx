import React, { useEffect, useState } from 'react';
import { ScrollView, StyleSheet, Text, TouchableOpacity, View } from 'react-native';
import { Colors } from '@/constants/Colors';
import { hoyLocalIso, listarDiasReservablesEnRango } from '@/lib/utils/formatFechas';

interface Props {
  fechaInicio?: string;
  fechaFin?: string;
  seleccionado: string;
  onSeleccionar: (dia: string) => void;
}

const MESES = ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun', 'Jul', 'Ago', 'Sep', 'Oct', 'Nov', 'Dic'];
const DIAS_SEM = ['D', 'L', 'M', 'X', 'J', 'V', 'S'];

export default function CalendarioVisita({ fechaInicio, fechaFin, seleccionado, onSeleccionar }: Props) {
  const [diasDisponibles, setDiasDisponibles] = useState<string[]>([]);
  const [mesActual, setMesActual] = useState(() => {
    const hoy = hoyLocalIso();
    return (fechaInicio && fechaInicio >= hoy ? fechaInicio : hoy).slice(0, 7);
  });

  useEffect(() => {
    const dias = listarDiasReservablesEnRango(fechaInicio, fechaFin);
    setDiasDisponibles(dias);
    if (dias.length > 0 && !seleccionado) onSeleccionar(dias[0]);
  }, [fechaInicio, fechaFin]);

  const [anio, mes] = mesActual.split('-').map(Number);
  const primerDia = new Date(Date.UTC(anio, mes - 1, 1)).getUTCDay();
  const diasEnMes = new Date(Date.UTC(anio, mes, 0)).getUTCDate();
  const set = new Set(diasDisponibles);

  const celdas: (string | null)[] = [
    ...Array(primerDia).fill(null),
    ...Array.from({ length: diasEnMes }, (_, i) => {
      const d = String(i + 1).padStart(2, '0');
      const m = String(mes).padStart(2, '0');
      return `${anio}-${m}-${d}`;
    }),
  ];

  const cambiarMes = (delta: number) => {
    const [y, m] = mesActual.split('-').map(Number);
    const d = new Date(Date.UTC(y, m - 1 + delta, 1));
    setMesActual(`${d.getUTCFullYear()}-${String(d.getUTCMonth() + 1).padStart(2, '0')}`);
  };

  if (diasDisponibles.length === 0) {
    return (
      <View style={styles.sinDias}>
        <Text style={styles.sinDiasText}>No hay fechas disponibles para este horario</Text>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      {/* Cabecera mes */}
      <View style={styles.header}>
        <TouchableOpacity onPress={() => cambiarMes(-1)} style={styles.navBtn}>
          <Text style={styles.navBtnText}>‹</Text>
        </TouchableOpacity>
        <Text style={styles.mesLabel}>{MESES[mes - 1]} {anio}</Text>
        <TouchableOpacity onPress={() => cambiarMes(1)} style={styles.navBtn}>
          <Text style={styles.navBtnText}>›</Text>
        </TouchableOpacity>
      </View>

      {/* Días de semana */}
      <View style={styles.grid}>
        {DIAS_SEM.map((d) => (
          <View key={d} style={styles.celda}>
            <Text style={styles.diaSem}>{d}</Text>
          </View>
        ))}

        {/* Celdas */}
        {celdas.map((fecha, i) => {
          if (!fecha) return <View key={`null-${i}`} style={styles.celda} />;
          const disponible = set.has(fecha);
          const selec = fecha === seleccionado;
          return (
            <TouchableOpacity
              key={fecha} style={styles.celda}
              onPress={() => disponible && onSeleccionar(fecha)}
              disabled={!disponible} activeOpacity={0.7}
            >
              <View style={[styles.dia, selec && styles.diaSelec, !disponible && styles.diaInactivo]}>
                <Text style={[styles.diaText, selec && styles.diaTextSelec, !disponible && styles.diaTextInactivo]}>
                  {fecha.slice(8)}
                </Text>
              </View>
            </TouchableOpacity>
          );
        })}
      </View>

      {/* Lista desplazable de días disponibles (para rangos largos) */}
      {diasDisponibles.length > 7 && (
        <ScrollView horizontal showsHorizontalScrollIndicator={false} style={styles.chips}>
          {diasDisponibles.map((d) => (
            <TouchableOpacity key={d} onPress={() => onSeleccionar(d)}
              style={[styles.chip, d === seleccionado && styles.chipActive]}>
              <Text style={[styles.chipText, d === seleccionado && styles.chipTextActive]}>{d.slice(5)}</Text>
            </TouchableOpacity>
          ))}
        </ScrollView>
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  container: { backgroundColor: Colors.surface, borderRadius: 16, padding: 16, marginBottom: 20 },
  header: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', marginBottom: 12 },
  mesLabel: { color: Colors.text, fontWeight: '700', fontSize: 16 },
  navBtn: { padding: 8 },
  navBtnText: { color: Colors.primary, fontSize: 22, fontWeight: '700' },
  grid: { flexDirection: 'row', flexWrap: 'wrap' },
  celda: { width: `${100 / 7}%`, alignItems: 'center', marginBottom: 6 },
  diaSem: { color: Colors.textMuted, fontSize: 11, fontWeight: '600', marginBottom: 4 },
  dia: { width: 36, height: 36, borderRadius: 18, alignItems: 'center', justifyContent: 'center' },
  diaSelec: { backgroundColor: Colors.primary },
  diaInactivo: { opacity: 0.3 },
  diaText: { color: Colors.text, fontSize: 14 },
  diaTextSelec: { color: '#fff', fontWeight: '700' },
  diaTextInactivo: { color: Colors.textMuted },
  sinDias: { backgroundColor: Colors.surface, borderRadius: 16, padding: 24, alignItems: 'center', marginBottom: 20 },
  sinDiasText: { color: Colors.textMuted, textAlign: 'center' },
  chips: { marginTop: 12 },
  chip: { paddingHorizontal: 12, paddingVertical: 6, borderRadius: 14, marginRight: 8, backgroundColor: Colors.border },
  chipActive: { backgroundColor: Colors.primary },
  chipText: { color: Colors.textMuted, fontSize: 12 },
  chipTextActive: { color: '#fff', fontWeight: '600' },
});
