import React, { useCallback, useEffect, useState } from 'react';
import {
  Alert, FlatList, Modal, RefreshControl,
  ScrollView, StyleSheet, Text, TouchableOpacity, View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import Button from '@/components/ui/Button';
import Input from '@/components/ui/Input';
import Spinner from '@/components/ui/Spinner';
import {
  crearCategoria, crearDestino, crearIdioma, crearIncluye,
  listarCategorias, listarDestinos, listarIdiomas, listarIncluye,
} from '@/lib/api/adminApi';
import { Colors } from '@/constants/Colors';

type Seccion = 'destinos' | 'categorias' | 'idiomas' | 'incluye';

const SECCIONES: { key: Seccion; label: string }[] = [
  { key: 'destinos', label: 'Destinos' },
  { key: 'categorias', label: 'Categorias' },
  { key: 'idiomas', label: 'Idiomas' },
  { key: 'incluye', label: 'Incluye' },
];

const getGuid = (item: Record<string, unknown>, seccion: Seccion) => {
  if (seccion === 'destinos') return String(item.des_guid ?? item.guid ?? '');
  if (seccion === 'categorias') return String(item.cat_guid ?? item.guid ?? '');
  if (seccion === 'idiomas') return String(item.id_guid ?? item.guid ?? '');
  if (seccion === 'incluye') return String(item.incluye_guid ?? item.guid ?? '');
  return String(item.guid ?? '');
};

const getLabel = (item: Record<string, unknown>, seccion: Seccion) => {
  if (seccion === 'destinos') {
    const nombre = String(item.nombre ?? '');
    const pais = String(item.pais ?? '');
    return pais ? `${nombre} — ${pais}` : nombre;
  }
  if (seccion === 'idiomas' || seccion === 'incluye')
    return String(item.descripcion ?? item.nombre ?? '');
  return String(item.nombre ?? '');
};

interface FormDestino { nombre: string; pais: string; imagen_url: string }
interface FormCategoria { nombre: string }
interface FormIdioma { descripcion: string }
interface FormIncluye { descripcion: string }

export default function AdminCatalogosScreen() {
  const [seccion, setSeccion] = useState<Seccion>('destinos');
  const [datos, setDatos] = useState<Record<string, unknown>[]>([]);
  const [cargando, setCargando] = useState(true);
  const [refrescando, setRefrescando] = useState(false);
  const [modal, setModal] = useState(false);
  const [guardando, setGuardando] = useState(false);
  const [errores, setErrores] = useState<Record<string, string>>({});

  // Forms por sección
  const [fDest, setFDest] = useState<FormDestino>({ nombre: '', pais: '', imagen_url: '' });
  const [fCat, setFCat] = useState<FormCategoria>({ nombre: '' });
  const [fIdi, setFIdi] = useState<FormIdioma>({ descripcion: '' });
  const [fInc, setFInc] = useState<FormIncluye>({ descripcion: '' });

  const cargar = useCallback(async (s?: Seccion) => {
    const actual = s ?? seccion;
    setCargando(true);
    try {
      let res;
      if (actual === 'destinos') res = await listarDestinos();
      else if (actual === 'categorias') res = await listarCategorias();
      else if (actual === 'idiomas') res = await listarIdiomas();
      else res = await listarIncluye();
      const raw = res as Record<string, unknown>;
      const d = raw?.data ?? raw;
      setDatos(Array.isArray(d) ? d : []);
    } catch { Alert.alert('Error', 'No se pudo cargar'); }
    finally { setCargando(false); setRefrescando(false); }
  }, [seccion]);

  useEffect(() => { cargar(); }, [cargar]);

  const cambiarSeccion = (s: Seccion) => {
    setSeccion(s);
    cargar(s);
  };

  const abrirModal = () => {
    setFDest({ nombre: '', pais: '', imagen_url: '' });
    setFCat({ nombre: '' });
    setFIdi({ descripcion: '' });
    setFInc({ descripcion: '' });
    setErrores({});
    setModal(true);
  };

  const validar = () => {
    const e: Record<string, string> = {};
    if (seccion === 'destinos') {
      if (!fDest.nombre.trim()) e.nombre = 'El nombre es obligatorio';
      if (!fDest.pais.trim()) e.pais = 'El pais es obligatorio';
    } else if (seccion === 'categorias') {
      if (!fCat.nombre.trim()) e.nombre = 'El nombre es obligatorio';
    } else if (seccion === 'idiomas') {
      if (!fIdi.descripcion.trim()) e.descripcion = 'La descripcion es obligatoria';
    } else if (seccion === 'incluye') {
      if (!fInc.descripcion.trim()) e.descripcion = 'La descripcion es obligatoria';
    }
    return e;
  };

  const onGuardar = async () => {
    const e = validar();
    if (Object.keys(e).length) { setErrores(e); return; }
    setGuardando(true);
    try {
      if (seccion === 'destinos') {
        const p: Record<string, unknown> = { nombre: fDest.nombre.trim(), pais: fDest.pais.trim() };
        if (fDest.imagen_url.trim()) p.imagen_url = fDest.imagen_url.trim();
        await crearDestino(p);
      } else if (seccion === 'categorias') {
        await crearCategoria({ nombre: fCat.nombre.trim() });
      } else if (seccion === 'idiomas') {
        await crearIdioma({ descripcion: fIdi.descripcion.trim() });
      } else {
        await crearIncluye({ descripcion: fInc.descripcion.trim() });
      }
      setModal(false);
      await cargar();
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message
        ?? (err as Error)?.message ?? 'Error al guardar';
      setErrores((p) => ({ ...p, _global: msg }));
    } finally { setGuardando(false); }
  };

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      {/* Tabs de sección */}
      <ScrollView horizontal showsHorizontalScrollIndicator={false} style={styles.tabs} contentContainerStyle={styles.tabsContent}>
        {SECCIONES.map((s) => (
          <TouchableOpacity key={s.key} style={[styles.tab, seccion === s.key && styles.tabActivo]} onPress={() => cambiarSeccion(s.key)}>
            <Text style={[styles.tabText, seccion === s.key && styles.tabTextActivo]}>{s.label}</Text>
          </TouchableOpacity>
        ))}
      </ScrollView>

      {cargando ? (
        <Spinner texto={`Cargando ${seccion}...`} />
      ) : (
        <FlatList
          data={datos}
          keyExtractor={(i) => getGuid(i, seccion) || String(Math.random())}
          contentContainerStyle={styles.list}
          refreshControl={<RefreshControl refreshing={refrescando} onRefresh={() => { setRefrescando(true); cargar(); }} tintColor={Colors.primary} />}
          renderItem={({ item }) => (
            <View style={styles.card}>
              <Text style={styles.itemLabel}>{getLabel(item, seccion)}</Text>
              <Text style={styles.itemGuid}>{getGuid(item, seccion).slice(0, 8)}…</Text>
            </View>
          )}
          ListHeaderComponent={
            <Button
              title={`+ Nuevo ${SECCIONES.find((s) => s.key === seccion)?.label.replace(/s$/, '') ?? ''}`}
              onPress={abrirModal}
              style={{ marginBottom: 16 }}
            />
          }
          ListEmptyComponent={<Text style={styles.empty}>No hay {seccion}</Text>}
        />
      )}

      <Modal visible={modal} animationType="slide" presentationStyle="formSheet" onRequestClose={() => setModal(false)}>
        <SafeAreaView style={styles.modalSafe}>
          <View style={styles.modalHeader}>
            <Text style={styles.modalTitle}>
              Nuevo {SECCIONES.find((s) => s.key === seccion)?.label.replace(/s$/, '')}
            </Text>
            <TouchableOpacity onPress={() => setModal(false)}><Text style={styles.cerrar}>✕</Text></TouchableOpacity>
          </View>
          <ScrollView contentContainerStyle={styles.modalScroll} keyboardShouldPersistTaps="handled">
            {seccion === 'destinos' && (
              <>
                <Input label="Nombre *" value={fDest.nombre} onChangeText={(v) => { setFDest((p) => ({ ...p, nombre: v })); setErrores((p) => ({ ...p, nombre: '' })); }} placeholder="Paris" error={errores.nombre} />
                <Input label="Pais *" value={fDest.pais} onChangeText={(v) => { setFDest((p) => ({ ...p, pais: v })); setErrores((p) => ({ ...p, pais: '' })); }} placeholder="Francia" error={errores.pais} />
                <Input label="URL de imagen (opcional)" value={fDest.imagen_url} onChangeText={(v) => setFDest((p) => ({ ...p, imagen_url: v }))} placeholder="https://..." />
              </>
            )}
            {seccion === 'categorias' && (
              <Input label="Nombre *" value={fCat.nombre} onChangeText={(v) => { setFCat({ nombre: v }); setErrores((p) => ({ ...p, nombre: '' })); }} placeholder="Aventura" error={errores.nombre} />
            )}
            {seccion === 'idiomas' && (
              <Input label="Descripcion *" value={fIdi.descripcion} onChangeText={(v) => { setFIdi({ descripcion: v }); setErrores((p) => ({ ...p, descripcion: '' })); }} placeholder="Espanol" error={errores.descripcion} />
            )}
            {seccion === 'incluye' && (
              <Input label="Descripcion *" value={fInc.descripcion} onChangeText={(v) => { setFInc({ descripcion: v }); setErrores((p) => ({ ...p, descripcion: '' })); }} placeholder="Transporte incluido" error={errores.descripcion} />
            )}
            {errores._global && <Text style={styles.errorText}>{errores._global}</Text>}
            <Button title="Guardar" onPress={onGuardar} loading={guardando} size="lg" />
          </ScrollView>
        </SafeAreaView>
      </Modal>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: Colors.background },
  tabs: { flexGrow: 0, borderBottomWidth: 1, borderBottomColor: Colors.border },
  tabsContent: { paddingHorizontal: 16, paddingVertical: 8, gap: 8 },
  tab: { paddingHorizontal: 16, paddingVertical: 8, borderRadius: 20, backgroundColor: Colors.surface, borderWidth: 1, borderColor: Colors.border },
  tabActivo: { backgroundColor: Colors.primary, borderColor: Colors.primary },
  tabText: { color: Colors.textMuted, fontWeight: '600', fontSize: 13 },
  tabTextActivo: { color: '#fff' },
  list: { padding: 16 },
  card: { backgroundColor: Colors.surface, borderRadius: 12, padding: 14, marginBottom: 10 },
  itemLabel: { color: Colors.text, fontWeight: '600', fontSize: 15 },
  itemGuid: { color: Colors.textMuted, fontSize: 11, marginTop: 2, fontFamily: 'monospace' },
  empty: { color: Colors.textMuted, textAlign: 'center', marginTop: 40 },
  modalSafe: { flex: 1, backgroundColor: Colors.background },
  modalHeader: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', padding: 20, borderBottomWidth: 1, borderBottomColor: Colors.border },
  modalTitle: { color: Colors.text, fontSize: 18, fontWeight: '700' },
  cerrar: { color: Colors.textMuted, fontSize: 20, padding: 4 },
  modalScroll: { padding: 20 },
  errorText: { color: Colors.danger, fontSize: 13, marginBottom: 8 },
});
