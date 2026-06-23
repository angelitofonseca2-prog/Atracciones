import React, { useCallback, useEffect, useState } from 'react';
import {
  Alert, FlatList, Modal, RefreshControl,
  ScrollView, StyleSheet, Text, TouchableOpacity, View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import Button from '@/components/ui/Button';
import ChipsSelector from '@/components/ui/ChipsSelector';
import Input from '@/components/ui/Input';
import Select from '@/components/ui/Select';
import Spinner from '@/components/ui/Spinner';
import {
  actualizarAtraccion, crearAtraccion, crearImagen, eliminarAtraccion,
  listarAtraccionesAdmin, listarCategorias, listarDestinos,
  listarIdiomas, listarImagenes, listarIncluye,
} from '@/lib/api/adminApi';
import { Colors } from '@/constants/Colors';

interface AtraccionAdmin {
  at_guid?: string; id?: string;
  nombre?: string; ciudad?: string; precio_desde?: number; estado?: string;
}

interface Catalogo { id: string; label: string; extra?: string }

interface FormState {
  destino_guid: string;
  num_establecimiento: string;
  nombre: string;
  descripcion: string;
  direccion: string;
  duracion_minutos: string;
  punto_encuentro: string;
  precio_referencia: string;
  disponible: boolean;
  categoria_guids: string[];
  idioma_guids: string[];
  incluye_guids: string[];
  imagen_guids_existentes: string[];
  imagenes_nuevas: { url: string; descripcion?: string }[];
  nueva_imagen_url: string;
  nueva_imagen_desc: string;
}

const FORM_VACIO: FormState = {
  destino_guid: '', num_establecimiento: '', nombre: '',
  descripcion: '', direccion: '', duracion_minutos: '',
  punto_encuentro: '', precio_referencia: '', disponible: true,
  categoria_guids: [], idioma_guids: [], incluye_guids: [],
  imagen_guids_existentes: [], imagenes_nuevas: [],
  nueva_imagen_url: '', nueva_imagen_desc: '',
};

export default function AdminAtraccionesScreen() {
  const [items, setItems] = useState<AtraccionAdmin[]>([]);
  const [cargando, setCargando] = useState(true);
  const [refrescando, setRefrescando] = useState(false);
  const [modal, setModal] = useState(false);
  const [form, setForm] = useState<FormState>(FORM_VACIO);
  const [errores, setErrores] = useState<Record<string, string>>({});
  const [editandoGuid, setEditandoGuid] = useState<string | null>(null);
  const [guardando, setGuardando] = useState(false);

  // Catálogos
  const [destinos, setDestinos] = useState<Catalogo[]>([]);
  const [categorias, setCategorias] = useState<{ id: string; label: string }[]>([]);
  const [idiomas, setIdiomas] = useState<{ id: string; label: string }[]>([]);
  const [incluye, setIncluye] = useState<{ id: string; label: string }[]>([]);
  const [imagenes, setImagenes] = useState<{ id: string; label: string; url: string }[]>([]);
  const [cargandoCatalogos, setCargandoCatalogos] = useState(false);

  const cargar = useCallback(async () => {
    try {
      const res = await listarAtraccionesAdmin();
      const raw = res as Record<string, unknown>;
      const d = raw?.data ?? raw;
      setItems(Array.isArray(d) ? d : []);
    } catch { Alert.alert('Error', 'No se pudo cargar atracciones'); }
    finally { setCargando(false); setRefrescando(false); }
  }, []);

  useEffect(() => { cargar(); }, [cargar]);

  const cargarCatalogos = async () => {
    setCargandoCatalogos(true);
    try {
      const [dRes, cRes, iRes, inRes, imgRes] = await Promise.allSettled([
        listarDestinos(), listarCategorias(), listarIdiomas(), listarIncluye(), listarImagenes(),
      ]);
      const ext = (r: unknown): unknown[] => {
        const raw = r as Record<string, unknown>;
        const d = raw?.data ?? r;
        return Array.isArray(d) ? d : [];
      };

      if (dRes.status === 'fulfilled') {
        setDestinos(ext(dRes.value).map((d: unknown) => {
          const x = d as Record<string, unknown>;
          const guid = String(x.des_guid ?? x.guid ?? '');
          const label = [x.nombre, x.pais].filter(Boolean).join(' — ');
          return { id: guid, label };
        }));
      }
      if (cRes.status === 'fulfilled') {
        setCategorias(ext(cRes.value).map((c: unknown) => {
          const x = c as Record<string, unknown>;
          return { id: String(x.cat_guid ?? x.guid ?? ''), label: String(x.nombre ?? '') };
        }));
      }
      if (iRes.status === 'fulfilled') {
        setIdiomas(ext(iRes.value).map((i: unknown) => {
          const x = i as Record<string, unknown>;
          return { id: String(x.id_guid ?? x.guid ?? ''), label: String(x.descripcion ?? x.nombre ?? '') };
        }));
      }
      if (inRes.status === 'fulfilled') {
        setIncluye(ext(inRes.value).map((i: unknown) => {
          const x = i as Record<string, unknown>;
          return { id: String(x.incluye_guid ?? x.guid ?? ''), label: String(x.descripcion ?? x.nombre ?? '') };
        }));
      }
      if (imgRes.status === 'fulfilled') {
        setImagenes(ext(imgRes.value).map((i: unknown) => {
          const x = i as Record<string, unknown>;
          const url = String(x.url ?? '');
          let label = String(x.descripcion ?? '');
          if (!label && url) { try { label = new URL(url).hostname; } catch { label = url.slice(0, 30); } }
          return { id: String(x.img_guid ?? x.guid ?? ''), label, url };
        }));
      }
    } catch { /* no bloquear */ }
    finally { setCargandoCatalogos(false); }
  };

  const set = (k: keyof FormState) => (v: string) =>
    setForm((p) => ({ ...p, [k]: v }));

  const abrirCrear = async () => {
    setForm(FORM_VACIO);
    setErrores({});
    setEditandoGuid(null);
    setModal(true);
    await cargarCatalogos();
  };

  const abrirEditar = async (a: AtraccionAdmin) => {
    const guid = a.at_guid ?? a.id ?? null;
    setEditandoGuid(guid);
    setErrores({});
    setModal(true);
    // Cargar catálogos antes de intentar precargar el formulario
    await cargarCatalogos();
    // Intentar obtener el detalle completo para tener los guids de arrays
    let item: Record<string, unknown> = a as Record<string, unknown>;
    if (guid) {
      try {
        const raw = await (await import('@/lib/api/adminApi')).obtenerAtraccionAdmin(guid);
        if (raw && typeof raw === 'object') item = raw as Record<string, unknown>;
      } catch { /* usar el item del listado como fallback */ }
    }
    const toArr = (v: unknown): string[] => Array.isArray(v) ? v.map(String) : [];
    setForm({
      destino_guid: String(item.destino_guid ?? item.DestinoGuid ?? ''),
      num_establecimiento: String(item.num_establecimiento ?? item.NumEstablecimiento ?? ''),
      nombre: String(item.nombre ?? item.Nombre ?? ''),
      descripcion: String(item.descripcion ?? item.Descripcion ?? ''),
      direccion: String(item.direccion ?? item.Direccion ?? ''),
      duracion_minutos: String(item.duracion_minutos ?? item.DuracionMinutos ?? ''),
      punto_encuentro: String(item.punto_encuentro ?? item.PuntoEncuentro ?? ''),
      precio_referencia: String(item.precio_referencia ?? item.PrecioReferencia ?? ''),
      disponible: Boolean(item.disponible ?? item.Disponible ?? true),
      categoria_guids: toArr(item.categoria_guids ?? item.CategoriaGuids),
      idioma_guids: toArr(item.idioma_guids ?? item.IdiomaGuids),
      incluye_guids: toArr(item.incluye_guids ?? item.IncluyeGuids),
      imagen_guids_existentes: toArr(item.imagen_guids ?? item.ImagenGuids),
      imagenes_nuevas: [],
      nueva_imagen_url: '',
      nueva_imagen_desc: '',
    });
  };

  const agregarImagenUrl = () => {
    const url = form.nueva_imagen_url.trim();
    if (!url) { setErrores((p) => ({ ...p, nueva_imagen_url: 'Ingresa una URL' })); return; }
    try { new URL(url); } catch {
      setErrores((p) => ({ ...p, nueva_imagen_url: 'URL no válida (incluye https://)' }));
      return;
    }
    if (form.imagenes_nuevas.some((x) => x.url === url)) {
      setErrores((p) => ({ ...p, nueva_imagen_url: 'Esa URL ya fue añadida' }));
      return;
    }
    setForm((p) => ({
      ...p,
      imagenes_nuevas: [...p.imagenes_nuevas, { url, descripcion: p.nueva_imagen_desc.trim() || undefined }],
      nueva_imagen_url: '',
      nueva_imagen_desc: '',
    }));
    setErrores((p) => ({ ...p, nueva_imagen_url: '' }));
  };

  const validar = () => {
    const e: Record<string, string> = {};
    if (!form.destino_guid) e.destino_guid = 'Selecciona un destino';
    if (!form.nombre.trim()) e.nombre = 'El nombre es obligatorio';
    if (form.duracion_minutos && Number(form.duracion_minutos) <= 0)
      e.duracion_minutos = 'Debe ser mayor a 0';
    if (form.precio_referencia && Number(form.precio_referencia) < 0)
      e.precio_referencia = 'Debe ser positivo';
    if (form.categoria_guids.length === 0) e.categoria_guids = 'Selecciona al menos una categoría';
    if (form.idioma_guids.length === 0) e.idioma_guids = 'Selecciona al menos un idioma';
    if (form.incluye_guids.length === 0) e.incluye_guids = 'Selecciona al menos un elemento incluido';
    const totalImg = form.imagen_guids_existentes.length + form.imagenes_nuevas.length;
    if (totalImg === 0) e.imagenes = 'Agrega al menos una imagen';
    return e;
  };

  const onGuardar = async () => {
    const e = validar();
    if (Object.keys(e).length) { setErrores(e); return; }
    setGuardando(true);
    try {
      // 1) Crear imágenes nuevas → obtener img_guids
      const guidsCreados: string[] = [];
      for (const img of form.imagenes_nuevas) {
        const creada = await crearImagen({ url: img.url, descripcion: img.descripcion });
        const guid = String(creada?.img_guid ?? creada?.guid ?? '');
        if (!guid) throw new Error('La API no devolvió img_guid.');
        guidsCreados.push(guid);
      }
      // 2) Payload según contrato
      const payload: Record<string, unknown> = {
        destino_guid: form.destino_guid,
        nombre: form.nombre.trim(),
        categoria_guids: form.categoria_guids,
        idioma_guids: form.idioma_guids,
        incluye_guids: form.incluye_guids,
        imagen_guids: [...form.imagen_guids_existentes, ...guidsCreados],
      };
      if (form.num_establecimiento.trim()) payload.num_establecimiento = form.num_establecimiento.trim();
      if (form.descripcion.trim()) payload.descripcion = form.descripcion.trim();
      if (form.direccion.trim()) payload.direccion = form.direccion.trim();
      if (form.duracion_minutos) payload.duracion_minutos = Number(form.duracion_minutos);
      if (form.punto_encuentro.trim()) payload.punto_encuentro = form.punto_encuentro.trim();
      if (form.precio_referencia) payload.precio_referencia = Number(form.precio_referencia);
      if (editandoGuid) payload.disponible = form.disponible;

      if (editandoGuid) await actualizarAtraccion(editandoGuid, payload);
      else await crearAtraccion(payload);
      setModal(false);
      await cargar();
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string; details?: string[] } } })
        ?.response?.data?.message
        ?? (err as Error)?.message
        ?? 'Error al guardar';
      setErrores((p) => ({ ...p, _global: msg }));
    } finally { setGuardando(false); }
  };

  const onEliminar = (a: AtraccionAdmin) => {
    Alert.alert('Eliminar', `¿Eliminar "${a.nombre}"?`, [
      { text: 'Cancelar', style: 'cancel' },
      {
        text: 'Eliminar', style: 'destructive', onPress: async () => {
          try {
            await eliminarAtraccion((a.at_guid ?? a.id)!);
            await cargar();
          } catch { Alert.alert('Error', 'No se pudo eliminar'); }
        },
      },
    ]);
  };

  if (cargando) return <Spinner texto="Cargando atracciones..." />;

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      <FlatList
        data={items}
        keyExtractor={(i) => String(i.at_guid ?? i.id ?? Math.random())}
        contentContainerStyle={styles.list}
        refreshControl={<RefreshControl refreshing={refrescando} onRefresh={() => { setRefrescando(true); cargar(); }} tintColor={Colors.primary} />}
        renderItem={({ item: a }) => (
          <View style={styles.card}>
            <View style={styles.cardInfo}>
              <Text style={styles.nombre}>{a.nombre}</Text>
              {a.ciudad && <Text style={styles.sub}>{a.ciudad}</Text>}
              {a.precio_desde != null && <Text style={styles.precio}>Desde ${Number(a.precio_desde).toFixed(2)}</Text>}
            </View>
            <View style={styles.acciones}>
              <TouchableOpacity onPress={() => abrirEditar(a)} style={styles.btnEdit}><Text style={styles.btnEditText}>✎</Text></TouchableOpacity>
              <TouchableOpacity onPress={() => onEliminar(a)} style={styles.btnDel}><Text style={styles.btnDelText}>✕</Text></TouchableOpacity>
            </View>
          </View>
        )}
        ListHeaderComponent={<Button title="+ Nueva Atraccion" onPress={abrirCrear} style={{ marginBottom: 16 }} />}
        ListEmptyComponent={<Text style={styles.empty}>No hay atracciones</Text>}
      />

      <Modal visible={modal} animationType="slide" presentationStyle="pageSheet" onRequestClose={() => setModal(false)}>
        <SafeAreaView style={styles.modalSafe}>
          <View style={styles.modalHeader}>
            <Text style={styles.modalTitle}>{editandoGuid ? 'Editar' : 'Nueva'} Atraccion</Text>
            <TouchableOpacity onPress={() => setModal(false)}><Text style={styles.cerrar}>✕</Text></TouchableOpacity>
          </View>
          {cargandoCatalogos ? (
            <Spinner texto="Cargando catalogos..." />
          ) : (
            <ScrollView contentContainerStyle={styles.modalScroll} keyboardShouldPersistTaps="handled">

              <Select
                label="Destino *"
                value={form.destino_guid}
                onChange={(v) => { setForm((p) => ({ ...p, destino_guid: v })); setErrores((p) => ({ ...p, destino_guid: '' })); }}
                options={destinos.map((d) => ({ value: d.id, label: d.label }))}
                placeholder="Selecciona un destino"
                error={errores.destino_guid}
              />
              {destinos.length === 0 && <Text style={styles.hint}>No hay destinos. Crea uno en Catalogos.</Text>}

              <Input label="Nombre *" value={form.nombre} onChangeText={(v) => { set('nombre')(v); setErrores((p) => ({ ...p, nombre: '' })); }} placeholder="Nombre de la atraccion" error={errores.nombre} />
              <Input label="Num. establecimiento" value={form.num_establecimiento} onChangeText={set('num_establecimiento')} placeholder="EST-001" />
              <Input label="Descripcion" value={form.descripcion} onChangeText={set('descripcion')} multiline numberOfLines={3} style={{ height: 80, textAlignVertical: 'top' }} />
              <Input label="Direccion" value={form.direccion} onChangeText={set('direccion')} placeholder="Calle y numero" />
              <Input label="Duracion (minutos)" value={form.duracion_minutos} onChangeText={set('duracion_minutos')} keyboardType="numeric" placeholder="120" error={errores.duracion_minutos} />
              <Input label="Punto de encuentro" value={form.punto_encuentro} onChangeText={set('punto_encuentro')} placeholder="Donde se reunen los participantes" />
              <Input label="Precio de referencia ($)" value={form.precio_referencia} onChangeText={set('precio_referencia')} keyboardType="numeric" placeholder="0.00" error={errores.precio_referencia} />

              {editandoGuid && (
                <Select
                  label="Disponible"
                  value={form.disponible ? 'true' : 'false'}
                  onChange={(v) => setForm((p) => ({ ...p, disponible: v === 'true' }))}
                  options={[{ value: 'true', label: 'Si' }, { value: 'false', label: 'No' }]}
                />
              )}

              {/* Categorías */}
              <ChipsSelector
                titulo="Categorias *"
                subtitulo="Selecciona al menos una."
                items={categorias}
                selected={form.categoria_guids}
                onChange={(guids) => { setForm((p) => ({ ...p, categoria_guids: guids })); setErrores((p) => ({ ...p, categoria_guids: '' })); }}
                getId={(c) => c.id}
                getLabel={(c) => c.label}
                vacio="No hay categorias. Crea una en Catalogos."
                error={errores.categoria_guids}
              />

              {/* Idiomas */}
              <ChipsSelector
                titulo="Idiomas disponibles *"
                subtitulo="Selecciona al menos uno."
                items={idiomas}
                selected={form.idioma_guids}
                onChange={(guids) => { setForm((p) => ({ ...p, idioma_guids: guids })); setErrores((p) => ({ ...p, idioma_guids: '' })); }}
                getId={(i) => i.id}
                getLabel={(i) => i.label}
                vacio="No hay idiomas. Crea uno en Catalogos."
                error={errores.idioma_guids}
              />

              {/* Incluye */}
              <ChipsSelector
                titulo="Elementos incluidos *"
                subtitulo="Selecciona al menos uno."
                items={incluye}
                selected={form.incluye_guids}
                onChange={(guids) => { setForm((p) => ({ ...p, incluye_guids: guids })); setErrores((p) => ({ ...p, incluye_guids: '' })); }}
                getId={(i) => i.id}
                getLabel={(i) => i.label}
                vacio="No hay elementos. Crea uno en Catalogos."
                error={errores.incluye_guids}
              />

              {/* Imágenes existentes */}
              <Text style={styles.seccionLabel}>Imagenes *</Text>
              {imagenes.length > 0 && (
                <ChipsSelector
                  titulo=""
                  subtitulo="Selecciona imagenes ya registradas."
                  items={imagenes}
                  selected={form.imagen_guids_existentes}
                  onChange={(guids) => { setForm((p) => ({ ...p, imagen_guids_existentes: guids })); setErrores((p) => ({ ...p, imagenes: '' })); }}
                  getId={(i) => i.id}
                  getLabel={(i) => i.label}
                />
              )}

              {/* Imágenes nuevas */}
              {form.imagenes_nuevas.length > 0 && (
                <View style={styles.imagNuevas}>
                  {form.imagenes_nuevas.map((img) => (
                    <View key={img.url} style={styles.imagNuevaRow}>
                      <Text style={styles.imagNuevaUrl} numberOfLines={1}>{img.url}</Text>
                      <TouchableOpacity onPress={() => setForm((p) => ({ ...p, imagenes_nuevas: p.imagenes_nuevas.filter((x) => x.url !== img.url) }))}>
                        <Text style={styles.imagNuevaQuitar}>✕</Text>
                      </TouchableOpacity>
                    </View>
                  ))}
                </View>
              )}
              <Input label="Nueva URL de imagen" value={form.nueva_imagen_url} onChangeText={(v) => { set('nueva_imagen_url')(v); setErrores((p) => ({ ...p, nueva_imagen_url: '' })); }} placeholder="https://..." error={errores.nueva_imagen_url} />
              <Input label="Descripcion de imagen (opcional)" value={form.nueva_imagen_desc} onChangeText={set('nueva_imagen_desc')} placeholder="Galeria principal" />
              <Button title="Agregar imagen" onPress={agregarImagenUrl} variant="outline" style={{ marginBottom: 8 }} />
              {errores.imagenes && <Text style={styles.errorText}>{errores.imagenes}</Text>}

              {errores._global && <Text style={[styles.errorText, { marginBottom: 12 }]}>{errores._global}</Text>}

              <Button title={editandoGuid ? 'Guardar cambios' : 'Crear atraccion'} onPress={onGuardar} loading={guardando} size="lg" />
            </ScrollView>
          )}
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
  nombre: { color: Colors.text, fontWeight: '700', fontSize: 15, marginBottom: 2 },
  sub: { color: Colors.textMuted, fontSize: 13 },
  precio: { color: Colors.primary, fontSize: 12, marginTop: 2 },
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
  hint: { color: Colors.textMuted, fontSize: 12, marginBottom: 12, fontStyle: 'italic' },
  seccionLabel: { color: Colors.text, fontWeight: '700', fontSize: 14, marginBottom: 8, marginTop: 4 },
  imagNuevas: { marginBottom: 8 },
  imagNuevaRow: { flexDirection: 'row', alignItems: 'center', backgroundColor: Colors.surface, borderRadius: 8, padding: 8, marginBottom: 4, borderWidth: 1, borderColor: Colors.primary },
  imagNuevaUrl: { flex: 1, color: Colors.text, fontSize: 12 },
  imagNuevaQuitar: { color: Colors.danger, fontSize: 16, paddingHorizontal: 8 },
  errorText: { color: Colors.danger, fontSize: 13, marginBottom: 8 },
});
