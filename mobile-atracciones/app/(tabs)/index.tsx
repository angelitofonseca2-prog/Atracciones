import React, { useCallback, useEffect, useState } from 'react';
import { FlatList, RefreshControl, StyleSheet, Text, TouchableOpacity, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { router } from 'expo-router';
import TarjetaAtraccion, { Atraccion } from '@/components/atracciones/TarjetaAtraccion';
import Spinner from '@/components/ui/Spinner';
import { listarAtracciones } from '@/lib/api/atraccionesApi';
import { Colors } from '@/constants/Colors';

export default function HomeScreen() {
  const [destacadas, setDestacadas] = useState<Atraccion[]>([]);
  const [recientes, setRecientes] = useState<Atraccion[]>([]);
  const [cargando, setCargando] = useState(true);
  const [refrescando, setRefrescando] = useState(false);
  const [error, setError] = useState('');

  const cargar = useCallback(async () => {
    try {
      setError('');
      const toArr = (r: unknown): Atraccion[] => {
        if (Array.isArray(r)) return r;
        const d = (r as Record<string, unknown>)?.data;
        if (Array.isArray(d)) return d;
        const i = (d as Record<string, unknown>)?.items ?? (d as Record<string, unknown>)?.atracciones;
        return Array.isArray(i) ? i : [];
      };
      const [dest, rec] = await Promise.allSettled([
        listarAtracciones({ ordenar_por: 'highest_weighted_rating', limit: 6 }),
        listarAtracciones({ ordenar_por: 'trending', limit: 8 }),
      ]);
      const destArr = dest.status === 'fulfilled' ? toArr(dest.value) : [];
      const recArr = rec.status === 'fulfilled' ? toArr(rec.value) : [];
      setDestacadas(destArr);
      setRecientes(recArr);
      if (destArr.length === 0 && recArr.length === 0) {
        setError('No se pudo cargar el catálogo. Verifica tu conexión.');
      }
    } catch {
      setError('No se pudo cargar el catálogo. Verifica tu conexión.');
    } finally {
      setCargando(false);
      setRefrescando(false);
    }
  }, []);

  useEffect(() => { cargar(); }, [cargar]);

  if (cargando) return <Spinner texto="Cargando atracciones..." />;

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      <FlatList
        data={recientes}
        keyExtractor={(i) => String(i.at_guid ?? i.Id ?? Math.random())}
        renderItem={({ item }) => <TarjetaAtraccion item={item} />}
        contentContainerStyle={styles.list}
        showsVerticalScrollIndicator={false}
        refreshControl={
          <RefreshControl refreshing={refrescando} onRefresh={() => { setRefrescando(true); cargar(); }} tintColor={Colors.primary} />
        }
        ListHeaderComponent={
          <>
            <View style={styles.heroBanner}>
              <Text style={styles.heroEmoji}>🌍</Text>
              <Text style={styles.heroTitle}>Descubre tu próxima aventura</Text>
              <Text style={styles.heroSub}>Reserva experiencias únicas en Ecuador y el mundo</Text>
              <TouchableOpacity style={styles.heroBtn} onPress={() => router.push('/(tabs)/catalogo')}>
                <Text style={styles.heroBtnText}>Explorar todo →</Text>
              </TouchableOpacity>
            </View>

            {error ? <Text style={styles.error}>{error}</Text> : null}

            {destacadas.length > 0 && (
              <>
                <Text style={styles.sectionTitle}>⭐ Destacadas</Text>
                {destacadas.map((a) => <TarjetaAtraccion key={String(a.at_guid ?? a.Id)} item={a} />)}
              </>
            )}

            <Text style={styles.sectionTitle}>🆕 Recientes</Text>
          </>
        }
        ListEmptyComponent={
          !error ? <Text style={styles.empty}>No hay atracciones disponibles</Text> : null
        }
      />
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: Colors.background },
  list: { padding: 16 },
  heroBanner: {
    backgroundColor: Colors.surface, borderRadius: 20, padding: 24,
    alignItems: 'center', marginBottom: 28,
  },
  heroEmoji: { fontSize: 48, marginBottom: 12 },
  heroTitle: { color: Colors.text, fontSize: 22, fontWeight: '700', textAlign: 'center', marginBottom: 8 },
  heroSub: { color: Colors.textMuted, fontSize: 14, textAlign: 'center', marginBottom: 16 },
  heroBtn: { backgroundColor: Colors.primary, paddingHorizontal: 24, paddingVertical: 12, borderRadius: 24 },
  heroBtnText: { color: '#fff', fontWeight: '700', fontSize: 15 },
  sectionTitle: { color: Colors.text, fontSize: 18, fontWeight: '700', marginBottom: 14, marginTop: 8 },
  error: { color: Colors.danger, textAlign: 'center', marginBottom: 16 },
  empty: { color: Colors.textMuted, textAlign: 'center', marginTop: 40 },
});
