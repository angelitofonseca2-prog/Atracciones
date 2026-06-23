import React, { useCallback, useEffect, useState } from 'react';
import { ScrollView, StyleSheet, Text, TouchableOpacity, View } from 'react-native';
import { Image } from 'expo-image';
import { router, useLocalSearchParams, useNavigation } from 'expo-router';
import { SafeAreaView } from 'react-native-safe-area-context';
import Button from '@/components/ui/Button';
import Spinner from '@/components/ui/Spinner';
import { listarResenias, obtenerAtraccion } from '@/lib/api/atraccionesApi';
import { Colors } from '@/constants/Colors';

export default function DetalleAtraccionScreen() {
  const { guid } = useLocalSearchParams<{ guid: string }>();
  const navigation = useNavigation();
  const [atraccion, setAtraccion] = useState<Record<string, unknown> | null>(null);
  const [resenias, setResenias] = useState<unknown[]>([]);
  const [cargando, setCargando] = useState(true);
  const [error, setError] = useState('');

  const cargar = useCallback(async () => {
    if (!guid) return;
    try {
      const [aRes, rRes] = await Promise.allSettled([
        obtenerAtraccion(guid),
        listarResenias(guid),
      ]);
      if (aRes.status === 'fulfilled') {
        // API: { status, message, data: {...atraccion...} }
        const raw = aRes.value as Record<string, unknown>;
        const d = (raw?.data as Record<string, unknown>) ?? raw;
        setAtraccion(d);
        navigation.setOptions({ title: String(d?.nombre ?? d?.Nombre ?? 'Detalle') });
      } else {
        setError('No se pudo cargar la atracción');
      }
      if (rRes.status === 'fulfilled') {
        // API: { status, data: [...] }
        const raw = rRes.value as Record<string, unknown>;
        const arr = raw?.data ?? raw;
        setResenias(Array.isArray(arr) ? arr : []);
      }
    } catch {
      setError('Error al cargar');
    } finally {
      setCargando(false);
    }
  }, [guid]);

  useEffect(() => { cargar(); }, [cargar]);

  if (cargando) return <Spinner texto="Cargando detalle..." />;
  if (error || !atraccion)
    return (
      <View style={styles.errorBox}>
        <Text style={styles.errorText}>{error || 'Atracción no encontrada'}</Text>
        <Button title="Volver" onPress={() => router.back()} variant="outline" style={{ marginTop: 16 }} />
      </View>
    );

  const g = (k1: string, k2: string) => String(atraccion[k1] ?? atraccion[k2] ?? '');
  const nombre = g('nombre', 'Nombre');
  const ciudad = g('ciudad', 'Ciudad');
  const pais = g('pais', 'Pais');
  const imagen = g('imagen_principal', 'ImagenPrincipal');
  const descripcion = g('descripcion', 'Descripcion') || g('descripcion_larga', 'DescripcionLarga') || g('descripcion_corta', 'DescripcionCorta');
  const calificacion = Number(atraccion['calificacion'] ?? atraccion['Calificacion'] ?? 0);
  const precioDesde = Number(atraccion['precio_desde'] ?? atraccion['PrecioDesde'] ?? 0);
  const incluye = (atraccion['incluye'] ?? atraccion['Incluye'] ?? []) as string[];
  const idiomas = (atraccion['idiomas_disponibles'] ?? atraccion['idiomas'] ?? atraccion['Idiomas'] ?? []) as string[];

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      <ScrollView showsVerticalScrollIndicator={false}>
        {imagen ? (
          <Image source={{ uri: imagen }} style={styles.imagen} contentFit="cover" />
        ) : (
          <View style={[styles.imagen, styles.imagenPlaceholder]}>
            <Text style={{ fontSize: 64 }}>🏔</Text>
          </View>
        )}

        <View style={styles.content}>
          <Text style={styles.nombre}>{nombre}</Text>
          <Text style={styles.ubicacion}>📍 {[ciudad, pais].filter(Boolean).join(', ')}</Text>

          <View style={styles.statsRow}>
            {calificacion > 0 && (
              <View style={styles.stat}>
                <Text style={styles.statValue}>⭐ {calificacion.toFixed(1)}</Text>
                <Text style={styles.statLabel}>Calificación</Text>
              </View>
            )}
            {precioDesde > 0 && (
              <View style={styles.stat}>
                <Text style={styles.statValue}>${precioDesde.toFixed(2)}</Text>
                <Text style={styles.statLabel}>Desde</Text>
              </View>
            )}
            {resenias.length > 0 && (
              <View style={styles.stat}>
                <Text style={styles.statValue}>{resenias.length}</Text>
                <Text style={styles.statLabel}>Reseñas</Text>
              </View>
            )}
          </View>

          {descripcion ? (
            <View style={styles.section}>
              <Text style={styles.sectionTitle}>Descripción</Text>
              <Text style={styles.descripcion}>{descripcion}</Text>
            </View>
          ) : null}

          {incluye.length > 0 && (
            <View style={styles.section}>
              <Text style={styles.sectionTitle}>¿Qué incluye?</Text>
              {incluye.map((item, i) => (
                <Text key={i} style={styles.incluyeItem}>✓ {typeof item === 'string' ? item : JSON.stringify(item)}</Text>
              ))}
            </View>
          )}

          {idiomas.length > 0 && (
            <View style={styles.section}>
              <Text style={styles.sectionTitle}>Idiomas</Text>
              <Text style={styles.descripcion}>{idiomas.join(' · ')}</Text>
            </View>
          )}

          {resenias.length > 0 && (
            <View style={styles.section}>
              <Text style={styles.sectionTitle}>Reseñas</Text>
              {resenias.slice(0, 3).map((r, i) => {
                const rev = r as Record<string, unknown>;
                return (
                  <View key={i} style={styles.resenia}>
                    <Text style={styles.reseniaCalif}>{'⭐'.repeat(Number(rev.calificacion ?? rev.Calificacion ?? 0))}</Text>
                    <Text style={styles.reseniaComent}>{String(rev.comentario ?? rev.Comentario ?? '')}</Text>
                  </View>
                );
              })}
            </View>
          )}

          <Button
            title="Reservar ahora →"
            onPress={() => router.push(`/reservar/${guid}`)}
            size="lg"
            style={{ marginTop: 8, marginBottom: 32 }}
          />
        </View>
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: Colors.background },
  imagen: { width: '100%', height: 280 },
  imagenPlaceholder: { backgroundColor: Colors.surface, alignItems: 'center', justifyContent: 'center' },
  content: { padding: 20 },
  nombre: { color: Colors.text, fontSize: 24, fontWeight: '700', marginBottom: 8 },
  ubicacion: { color: Colors.textMuted, fontSize: 14, marginBottom: 16 },
  statsRow: { flexDirection: 'row', gap: 24, marginBottom: 24 },
  stat: { alignItems: 'center' },
  statValue: { color: Colors.primary, fontSize: 18, fontWeight: '700' },
  statLabel: { color: Colors.textMuted, fontSize: 12 },
  section: { marginBottom: 24 },
  sectionTitle: { color: Colors.text, fontSize: 17, fontWeight: '700', marginBottom: 10, borderBottomWidth: 1, borderBottomColor: Colors.border, paddingBottom: 6 },
  descripcion: { color: Colors.textMuted, fontSize: 14, lineHeight: 22 },
  incluyeItem: { color: Colors.textMuted, fontSize: 14, marginBottom: 4 },
  resenia: { backgroundColor: Colors.surface, borderRadius: 10, padding: 14, marginBottom: 10 },
  reseniaCalif: { marginBottom: 4, fontSize: 13 },
  reseniaComent: { color: Colors.textMuted, fontSize: 14 },
  errorBox: { flex: 1, alignItems: 'center', justifyContent: 'center', padding: 40, backgroundColor: Colors.background },
  errorText: { color: Colors.danger, fontSize: 16 },
});
