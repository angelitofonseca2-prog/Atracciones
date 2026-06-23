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
  actualizarCategoria, actualizarDestino, actualizarIdioma, actualizarIncluye,
  crearCategoria, crearDestino, crearIdioma, crearIncluye,
  eliminarCategoria, eliminarDestino, eliminarIdioma, eliminarIncluye,
  listarCategorias, listarDestinos, listarIdiomas, listarIncluye,
} from '@/lib/api/adminApi';
import { Colors } from '@/constants/Colors';

type Seccion = 'destinos' | 'categorias' | 'idiomas' | 'incluye';

const SECCIONES: { key: Seccion; label: string; singular: string }[] = [
  { key: 'destinos', label: 'Destinos', singular: 'Destino' },
  { key: 'categorias', label: 'Categorias', singular: 'Categoria' },
  { key: 'idiomas', label: 'Idiomas', singular: 'Idioma' },
  { key: 'incluye', label: 'Incluye', singular: 'Incluye' },
];

const getGuid = (item: Record<string, unknown>, s: Seccion): string => {
  if (s === 'destinos') return String(item.des_guid ?? item.guid ?? '');
  if (s === 'categorias') return String(item.cat_guid ?? item.guid ?? '');
  if (s === 'idiomas') return String(item.id_guid ?? item.guid ?? '');
  if (s === 'incluye') return String(item.incluye_guid ?? item.guid ?? '');
  return String(item.guid ?? '');
};

const getLabel = (item: Record<string, unknown>, s: Seccion): string => {
  if (s === 'destinos') {
    const n = String(item.nombre ?? '');
    const p = String(item.pais ?? '');
    return p ? `${n} — ${p}` : n;
  }
  if (s === 'idiomas' || s === 'incluye') return String(item.descripcion ?? item.nombre ?? '');
  return String(item.nombre ?? '');
};

interface FormDestino { nombre: string; pais: string; imagen_url: string }
interface FormCategoria { nombre: string }
interface FormDescripcion { descripcion: string }

export default function AdminCatalogosScreen() {
  const [seccion, setSeccion] = useState<Seccion>('destinos');
  const [datos, setDatos] = useState<Record<string, unknown>[]>([]);
  const [cargando, setCargando] = useState(true);
  const [refrescando, setRefrescando] = useState(false);
  const [modal, setModal] = useState(false);
  const [guardando, setGuardando] = useState(false);
  const [errores, setErrores] = useState<Record<string, string>>({});
  const [editandoGuid, setEditandoGuid] = useState<string | null>(null);

  const [fDest, setFDest] = useState<FormDestino>({ nombre: '', pais: '', imagen_url: '' });
  const [fCat, setFCat] = useState<FormCategoria>({ nombre: '' });
  const [fDesc, setFDesc] = useState<FormDescripcion>({ descripcion: '' });

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

  const abrirCrear = () => {
    setEditandoGuid(null);
    setFDest({ nombre: '', pais: '', imagen_url: '' });
    setFCat({ nombre: '' });
    setFDesc({ descripcion: '' });
    setErrores({});
    setModal(true);
  };

  const abrirEditar = (item: Record<string, unknown>) => {
    setEditandoGuid(getGuid(item, seccion));
    if (seccion === 'destinos') {
      setFDest({
        nombre: String(item.nombre ?? ''),
        pais: String(item.pais ?? ''),
        imagen_url: String(item.imagen_url ?? ''),
      });
    } else if (seccion === 'categorias') {
      setFCat({ nombre: String(item.nombre ?? '') });
    } else {
      setFDesc({ descripcion: String(item.descripcion ?? item.nombre ?? '') });
    }
    setErrores({});
    setModal(true);
  };

  const onEliminar = (item: Record<string, unknown>) => {
    const label = getLabel(item, seccion);
    const guid = getGuid(item, seccion);
    Alert.alert(`Eliminar ${SECCIONES.find((s) => s.key === seccion)?.singular}`, `¿Eliminar "${label}"?`, [
      { text: 'Cancelar', style: 'cancel' },
      {
        text: 'Eliminar', style: 'destructive', onPress: async () => {
          try {
            if (seccion === 'destinos') await eliminarDestino(guid);
            else if (seccion === 'categorias') await eliminarCategoria(guid);
            else if (seccion === 'idiomas') await eliminarIdioma(guid);
            else await eliminarIncluye(guid);
            await cargar();
          } catch (e: unknown) {
            const msg = (e as { response?: { data?: { message?: string } } })?.response?.data?.message ?? 'No se pudo eliminar';
            Alert.alert('Error', msg);
          }
        },
      },
    ]);
  };

  const validar = () => {
    const e: Record<string, string> = {};
    if (seccion === 'destinos') {
      if (!fDest.nombre.trim()) e.nombre = 'El nombre es obligatorio';
      if (!fDest.pais.trim()) e.pais = 'El pais es obligatorio';
    } else if (seccion === 'categorias') {
      if (!fCat.nombre.trim()) e.nombre = 'El nombre es obligatorio';
    } else {
      if (!fDesc.descripcion.trim()) e.descripcion = 'La descripcion es obligatoria';
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
        if (editandoGuid) await actualizarDestino(editandoGuid, p);
        else await crearDestino(p);
      } else if (seccion === 'categorias') {
        const p = { nombre: fCat.nombre.trim() };
        if (editandoGuid) await actualizarCategoria(editandoGuid, p);
        else await crearCategoria(p);
      } else if (seccion === 'idiomas') {
        const p = { descripcion: fDesc.descripcion.trim() };
        if (editandoGuid) await actualizarIdioma(editandoGuid, p);
        else await crearIdioma(p);
      } else {
        const p = { descripcion: fDesc.descripcion.trim() };
        if (editandoGuid) await actualizarIncluye(editandoGuid, p);
        else await crearIncluye(p);
      }
      setModal(false);
      await cargar();
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message
        ?? (err as Error)?.message ?? 'Error al guardar';
      setErrores((p) => ({ ...p, _global: msg }));
    } finally { setGuardando(false); }
  };

  const singLabel = SECCIONES.find((s) => s.key === seccion)?.singular ?? '';

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      {/* Tabs */}
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
              <View style={styles.cardInfo}>
                <Text style={styles.itemLabel}>{getLabel(item, seccion)}</Text>
                <Text style={styles.itemGuid}>{getGuid(item, seccion).slice(0, 8)}…</Text>
              </View>
              <View style={styles.acciones}>
                <TouchableOpacity onPress={() => abrirEditar(item)} style={styles.btnEdit}>
                  <Text style={styles.btnEditText}>✎</Text>
                </TouchableOpacity>
                <TouchableOpacity onPress={() => onEliminar(item)} style={styles.btnDel}>
                  <Text style={styles.btnDelText}>✕</Text>
                </TouchableOpacity>
              </View>
            </View>
          )}
          ListHeaderComponent={<Button title={`+ Nuevo ${singLabel}`} onPress={abrirCrear} style={{ marginBottom: 16 }} />}
          ListEmptyComponent={<Text style={styles.empty}>No hay {seccion}</Text>}
        />
      )}

      <Modal visible={modal} animationType="slide" presentationStyle="formSheet" onRequestClose={() => setModal(false)}>
        <SafeAreaView style={styles.modalSafe}>
          <View style={styles.modalHeader}>
            <Text style={styles.modalTitle}>{editandoGuid ? `Editar ${singLabel}` : `Nuevo ${singLabel}`}</Text>
            <TouchableOpacity onPress={() => setModal(false)}><Text style={styles.cerrar}>✕</Text></TouchableOpacity>
          </View>
          <ScrollView contentContainerStyle={styles.modalScroll} keyboardShouldPersistTaps="handled">
            {seccion === 'destinos' && (
              <>
                <Input label="Nombre *" value={fDest.nombre} onChangeText={(v) => { setFDest((p) => ({ ...p, nombre: v })); setErrores((p) => ({ ...p, nombre: '' })); }} placeholder="Ciudad o lugar" error={errores.nombre} />
                <Input label="Pais *" value={fDest.pais} onChangeText={(v) => { setFDest((p) => ({ ...p, pais: v })); setErrores((p) => ({ ...p, pais: '' })); }} placeholder="Ej. Ecuador" error={errores.pais} />
                <Input label="URL de imagen (opcional)" value={fDest.imagen_url} onChangeText={(v) => setFDest((p) => ({ ...p, imagen_url: v }))} placeholder="https://..." />
              </>
            )}
            {seccion === 'categorias' && (
              <Input label="Nombre *" value={fCat.nombre} onChangeText={(v) => { setFCat({ nombre: v }); setErrores((p) => ({ ...p, nombre: '' })); }} placeholder="Ej. Aventura" error={errores.nombre} />
            )}
            {(seccion === 'idiomas' || seccion === 'incluye') && (
              <Input label="Descripcion *" value={fDesc.descripcion} onChangeText={(v) => { setFDesc({ descripcion: v }); setErrores((p) => ({ ...p, descripcion: '' })); }} placeholder={seccion === 'idiomas' ? 'Ej. Espanol' : 'Ej. Transporte incluido'} error={errores.descripcion} />
            )}
            {errores._global && <Text style={styles.errorText}>{errores._global}</Text>}
            <Button title={editandoGuid ? 'Guardar cambios' : 'Crear'} onPress={onGuardar} loading={guardando} size="lg" />
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
  card: { backgroundColor: Colors.surface, borderRadius: 12, padding: 14, marginBottom: 10, flexDirection: 'row', alignItems: 'center' },
  cardInfo: { flex: 1 },
  itemLabel: { color: Colors.text, fontWeight: '600', fontSize: 15 },
  itemGuid: { color: Colors.textMuted, fontSize: 11, marginTop: 2 },
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
  errorText: { color: Colors.danger, fontSize: 13, marginBottom: 8 },
});
