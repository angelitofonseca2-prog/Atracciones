import React, { useCallback, useEffect, useRef, useState } from 'react';
import { FlatList, RefreshControl, StyleSheet, Text, TextInput, TouchableOpacity, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import TarjetaAtraccion, { Atraccion } from '@/components/atracciones/TarjetaAtraccion';
import Spinner from '@/components/ui/Spinner';
import { listarAtracciones, obtenerFiltros } from '@/lib/api/atraccionesApi';
import { Colors } from '@/constants/Colors';

interface Filtros { ciudad?: string; tipo?: string; busqueda?: string }

export default function CatalogoScreen() {
  const [items, setItems] = useState<Atraccion[]>([]);
  const [ciudades, setCiudades] = useState<string[]>([]);
  const [tipos, setTipos] = useState<string[]>([]);
  const [filtros, setFiltros] = useState<Filtros>({});
  const [busqueda, setBusqueda] = useState('');
  const [cargando, setCargando] = useState(true);
  const [refrescando, setRefrescando] = useState(false);
  const [page, setPage] = useState(1);
  const [hayMas, setHayMas] = useState(true);
  const [error, setError] = useState('');
  const busquedaRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  const toArr = (r: unknown): Atraccion[] => {
    if (Array.isArray(r)) return r;
    const d = (r as Record<string, unknown>)?.data;
    if (Array.isArray(d)) return d;
    const i = (d as Record<string, unknown>)?.items ?? (d as Record<string, unknown>)?.atracciones;
    return Array.isArray(i) ? i : [];
  };

  const cargar = useCallback(async (reset = false) => {
    try {
      setError('');
      const p = reset ? 1 : page;
      const res = await listarAtracciones({ ...filtros, page: p, limit: 10 });
      const arr = toArr(res);
      setItems((prev) => reset ? arr : [...prev, ...arr]);
      setHayMas(arr.length === 10);
      if (!reset) setPage(p + 1);
    } catch {
      setError('Error al cargar. Verifica tu conexión.');
    } finally {
      setCargando(false);
      setRefrescando(false);
    }
  }, [filtros, page]);

  const cargarFiltros = useCallback(async () => {
    try {
      const res = await obtenerFiltros();
      const d = (res?.data ?? res) as Record<string, unknown>;
      const arr = (v: unknown) => (Array.isArray(v) ? v.map(String) : []);
      setCiudades(arr(d?.ciudades ?? d?.Ciudades));
      setTipos(arr(d?.tipos ?? d?.Tipos));
    } catch {}
  }, []);

  useEffect(() => { cargarFiltros(); }, [cargarFiltros]);

  useEffect(() => {
    setPage(1);
    setCargando(true);
    setItems([]);
    cargar(true);
  }, [filtros]);

  const onBusqueda = (v: string) => {
    setBusqueda(v);
    if (busquedaRef.current) clearTimeout(busquedaRef.current);
    busquedaRef.current = setTimeout(() => {
      setFiltros((p) => ({ ...p, busqueda: v || undefined }));
    }, 400);
  };

  const setCiudad = (c: string) =>
    setFiltros((p) => ({ ...p, ciudad: p.ciudad === c ? undefined : c }));
  const setTipo = (t: string) =>
    setFiltros((p) => ({ ...p, tipo: p.tipo === t ? undefined : t }));

  if (cargando && items.length === 0) return <Spinner texto="Cargando catálogo..." />;

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      <FlatList
        data={items}
        keyExtractor={(i) => String(i.at_guid ?? i.Id ?? Math.random())}
        renderItem={({ item }) => <TarjetaAtraccion item={item} />}
        contentContainerStyle={styles.list}
        showsVerticalScrollIndicator={false}
        onEndReached={() => { if (hayMas && !cargando) cargar(false); }}
        onEndReachedThreshold={0.3}
        refreshControl={
          <RefreshControl refreshing={refrescando} onRefresh={() => { setRefrescando(true); cargar(true); }} tintColor={Colors.primary} />
        }
        ListHeaderComponent={
          <View>
            {/* Buscador */}
            <View style={styles.searchRow}>
              <TextInput
                style={styles.searchInput}
                placeholder="🔍  Buscar atracciones..."
                placeholderTextColor={Colors.textMuted}
                value={busqueda}
                onChangeText={onBusqueda}
              />
            </View>

            {/* Filtros ciudades */}
            {ciudades.length > 0 && (
              <FlatList
                horizontal data={ciudades} keyExtractor={(c) => c}
                showsHorizontalScrollIndicator={false} style={styles.chips}
                renderItem={({ item: c }) => (
                  <TouchableOpacity style={[styles.chip, filtros.ciudad === c && styles.chipActive]} onPress={() => setCiudad(c)}>
                    <Text style={[styles.chipText, filtros.ciudad === c && styles.chipTextActive]}>📍 {c}</Text>
                  </TouchableOpacity>
                )}
              />
            )}

            {/* Filtros tipos */}
            {tipos.length > 0 && (
              <FlatList
                horizontal data={tipos} keyExtractor={(t) => t}
                showsHorizontalScrollIndicator={false} style={styles.chips}
                renderItem={({ item: t }) => (
                  <TouchableOpacity style={[styles.chip, filtros.tipo === t && styles.chipActive]} onPress={() => setTipo(t)}>
                    <Text style={[styles.chipText, filtros.tipo === t && styles.chipTextActive]}>{t}</Text>
                  </TouchableOpacity>
                )}
              />
            )}
            {error ? <Text style={styles.error}>{error}</Text> : null}
          </View>
        }
        ListFooterComponent={cargando ? <Spinner texto="" /> : null}
        ListEmptyComponent={!cargando ? <Text style={styles.empty}>No se encontraron atracciones</Text> : null}
      />
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: Colors.background },
  list: { padding: 16 },
  searchRow: { marginBottom: 12 },
  searchInput: {
    backgroundColor: Colors.surface, borderRadius: 12, borderWidth: 1, borderColor: Colors.border,
    paddingHorizontal: 16, paddingVertical: 12, color: Colors.text, fontSize: 15,
  },
  chips: { marginBottom: 12 },
  chip: {
    paddingHorizontal: 14, paddingVertical: 8, borderRadius: 20, marginRight: 8,
    backgroundColor: Colors.surface, borderWidth: 1, borderColor: Colors.border,
  },
  chipActive: { backgroundColor: Colors.primary, borderColor: Colors.primary },
  chipText: { color: Colors.textMuted, fontSize: 13 },
  chipTextActive: { color: '#fff', fontWeight: '600' },
  error: { color: Colors.danger, textAlign: 'center', marginBottom: 16 },
  empty: { color: Colors.textMuted, textAlign: 'center', marginTop: 60, fontSize: 15 },
});
