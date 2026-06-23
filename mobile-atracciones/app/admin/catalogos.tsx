import React, { useCallback, useEffect, useState } from 'react';
import { Alert, FlatList, Modal, ScrollView, StyleSheet, Text, TouchableOpacity, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import Button from '@/components/ui/Button';
import Input from '@/components/ui/Input';
import Spinner from '@/components/ui/Spinner';
import { crearCategoria, crearDestino, crearIdioma, crearIncluye, eliminarDestino, listarCategorias, listarDestinos, listarIdiomas, listarIncluye } from '@/lib/api/adminApi';
import { Colors } from '@/constants/Colors';

type Seccion = 'destinos' | 'categorias' | 'idiomas' | 'incluye';

export default function AdminCatalogosScreen() {
  const [seccion, setSeccion] = useState<Seccion>('destinos');
  const [items, setItems] = useState<Record<string, unknown>[]>([]);
  const [cargando, setCargando] = useState(true);
  const [modal, setModal] = useState(false);
  const [nombre, setNombre] = useState('');
  const [guardando, setGuardando] = useState(false);

  const cargar = useCallback(async () => {
    setCargando(true);
    try {
      let res;
      if (seccion === 'destinos') res = await listarDestinos();
      else if (seccion === 'categorias') res = await listarCategorias();
      else if (seccion === 'idiomas') res = await listarIdiomas();
      else res = await listarIncluye();
      const d = res?.data ?? res;
      setItems(Array.isArray(d) ? d : d?.items ?? []);
    } catch {} finally { setCargando(false); }
  }, [seccion]);

  useEffect(() => { cargar(); }, [cargar]);

  const onCrear = async () => {
    if (!nombre.trim()) { Alert.alert('El nombre es requerido'); return; }
    setGuardando(true);
    try {
      if (seccion === 'destinos') await crearDestino({ nombre });
      else if (seccion === 'categorias') await crearCategoria({ nombre });
      else if (seccion === 'idiomas') await crearIdioma({ nombre });
      else await crearIncluye({ nombre });
      setModal(false);
      setNombre('');
      await cargar();
    } catch (err: unknown) {
      Alert.alert('Error', (err as { response?: { data?: { message?: string } } })?.response?.data?.message ?? 'Error al crear');
    } finally { setGuardando(false); }
  };

  const onEliminar = (id: number) => {
    if (seccion !== 'destinos') { Alert.alert('Solo destinos se puede eliminar por ahora'); return; }
    Alert.alert('Eliminar', '¿Eliminar este elemento?', [
      { text: 'Cancelar', style: 'cancel' },
      { text: 'Eliminar', style: 'destructive', onPress: async () => { try { await eliminarDestino(id); await cargar(); } catch { Alert.alert('Error'); } } },
    ]);
  };

  const SECCIONES: Seccion[] = ['destinos', 'categorias', 'idiomas', 'incluye'];

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      {/* Tabs */}
      <ScrollView horizontal showsHorizontalScrollIndicator={false} style={styles.tabs} contentContainerStyle={styles.tabsContent}>
        {SECCIONES.map((s) => (
          <TouchableOpacity key={s} style={[styles.tab, seccion === s && styles.tabActive]} onPress={() => setSeccion(s)}>
            <Text style={[styles.tabText, seccion === s && styles.tabTextActive]}>{s.charAt(0).toUpperCase() + s.slice(1)}</Text>
          </TouchableOpacity>
        ))}
      </ScrollView>

      {cargando ? <Spinner /> : (
        <FlatList
          data={items}
          keyExtractor={(i) => String(i.id ?? i.des_guid ?? Math.random())}
          contentContainerStyle={styles.list}
          renderItem={({ item }) => (
            <View style={styles.card}>
              <Text style={styles.nombre}>{String(item.nombre ?? item.Nombre ?? '—')}</Text>
              <TouchableOpacity onPress={() => onEliminar(item.id as number)} style={styles.btnDel}>
                <Text style={styles.btnDelText}>✕</Text>
              </TouchableOpacity>
            </View>
          )}
          ListHeaderComponent={<Button title={`+ Nuevo en ${seccion}`} onPress={() => { setNombre(''); setModal(true); }} style={{ marginBottom: 16 }} />}
          ListEmptyComponent={<Text style={styles.empty}>No hay elementos</Text>}
        />
      )}

      <Modal visible={modal} animationType="slide" presentationStyle="formSheet" onRequestClose={() => setModal(false)}>
        <SafeAreaView style={styles.modalSafe}>
          <View style={styles.modalHeader}>
            <Text style={styles.modalTitle}>Nuevo en {seccion}</Text>
            <TouchableOpacity onPress={() => setModal(false)}><Text style={styles.cerrar}>✕</Text></TouchableOpacity>
          </View>
          <View style={styles.modalScroll}>
            <Input label="Nombre *" value={nombre} onChangeText={setNombre} placeholder="Nombre..." autoFocus />
            <Button title="Crear" onPress={onCrear} loading={guardando} size="lg" style={{ marginTop: 8 }} />
          </View>
        </SafeAreaView>
      </Modal>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: Colors.background },
  tabs: { maxHeight: 56, backgroundColor: Colors.surface, borderBottomWidth: 1, borderBottomColor: Colors.border },
  tabsContent: { paddingHorizontal: 16, alignItems: 'center', gap: 8 },
  tab: { paddingHorizontal: 16, paddingVertical: 8, borderRadius: 20 },
  tabActive: { backgroundColor: Colors.primary },
  tabText: { color: Colors.textMuted, fontWeight: '600', fontSize: 14 },
  tabTextActive: { color: '#fff' },
  list: { padding: 16 },
  card: { backgroundColor: Colors.surface, borderRadius: 14, padding: 14, marginBottom: 10, flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between' },
  nombre: { color: Colors.text, fontSize: 15, flex: 1 },
  btnDel: { width: 32, height: 32, borderRadius: 8, backgroundColor: `${Colors.danger}33`, alignItems: 'center', justifyContent: 'center' },
  btnDelText: { color: Colors.danger, fontSize: 14 },
  empty: { color: Colors.textMuted, textAlign: 'center', marginTop: 40 },
  modalSafe: { flex: 1, backgroundColor: Colors.background },
  modalHeader: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', padding: 20, borderBottomWidth: 1, borderBottomColor: Colors.border },
  modalTitle: { color: Colors.text, fontSize: 18, fontWeight: '700' },
  cerrar: { color: Colors.textMuted, fontSize: 20, padding: 4 },
  modalScroll: { padding: 20 },
});
