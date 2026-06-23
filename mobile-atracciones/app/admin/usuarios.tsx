import React, { useCallback, useEffect, useState } from 'react';
import {
  FlatList, RefreshControl, StyleSheet, Text,
  TouchableOpacity, View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import Spinner from '@/components/ui/Spinner';
import { listarUsuarios } from '@/lib/api/adminApi';
import { Colors } from '@/constants/Colors';

const LIMIT = 10;

interface Usuario {
  usu_guid?: string; usr_guid?: string;
  login?: string;
  estado?: string;
  roles?: string[]; rol?: string;
  fecha_registro?: string;
}

export default function AdminUsuariosScreen() {
  const [items, setItems] = useState<Usuario[]>([]);
  const [cargando, setCargando] = useState(true);
  const [refrescando, setRefrescando] = useState(false);
  const [error, setError] = useState('');
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  const cargar = useCallback(async (p = 1) => {
    if (p === 1) setCargando(true);
    setError('');
    try {
      const res = await listarUsuarios({ page: p, limit: LIMIT });
      const data = Array.isArray(res.data) ? res.data : [];
      setItems(data);
      setPage(p);
      const pag = res.pagination as Record<string, unknown> | null;
      const total = Number(pag?.total ?? data.length);
      const limit = Number(pag?.limit ?? LIMIT);
      setTotalPages(Math.max(1, Math.ceil(total / limit)));
    } catch (e: unknown) {
      const status = (e as { response?: { status?: number } })?.response?.status;
      const msg = (e as { response?: { data?: { message?: string } } })?.response?.data?.message;
      if (status === 403) setError('No tienes permisos de administrador para ver usuarios.');
      else if (status === 401) setError('Sesion no valida. Vuelve a iniciar sesion.');
      else if (status === 404) setError('Ruta de usuarios no encontrada. Verifica la configuracion del gateway.');
      else setError(msg ?? 'No se pudieron cargar los usuarios.');
    } finally {
      setCargando(false);
      setRefrescando(false);
    }
  }, []);

  useEffect(() => { cargar(1); }, [cargar]);

  const roles = (u: Usuario) => {
    if (Array.isArray(u.roles) && u.roles.length) return u.roles.join(', ');
    if (u.rol) return u.rol;
    return '—';
  };

  if (cargando) return <Spinner texto="Cargando usuarios..." />;

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      {error ? (
        <View style={styles.errorBox}>
          <Text style={styles.errorText}>{error}</Text>
          <TouchableOpacity onPress={() => cargar(1)} style={styles.retryBtn}>
            <Text style={styles.retryText}>Reintentar</Text>
          </TouchableOpacity>
        </View>
      ) : (
        <FlatList
          data={items}
          keyExtractor={(u) => String(u.usu_guid ?? u.usr_guid ?? u.login ?? Math.random())}
          contentContainerStyle={styles.list}
          refreshControl={
            <RefreshControl
              refreshing={refrescando}
              onRefresh={() => { setRefrescando(true); cargar(1); }}
              tintColor={Colors.primary}
            />
          }
          renderItem={({ item: u }) => (
            <View style={styles.card}>
              <View style={styles.avatar}>
                <Text style={styles.avatarText}>
                  {u.login?.charAt(0)?.toUpperCase() ?? '?'}
                </Text>
              </View>
              <View style={styles.info}>
                <Text style={styles.login}>{u.login ?? '—'}</Text>
                <Text style={styles.roles}>{roles(u)}</Text>
                <Text style={[styles.estado, u.estado === 'A' ? styles.activo : styles.inactivo]}>
                  {u.estado === 'A' ? 'Activo' : u.estado ?? '—'}
                </Text>
              </View>
            </View>
          )}
          ListFooterComponent={
            totalPages > 1 ? (
              <View style={styles.paginacion}>
                <TouchableOpacity
                  style={[styles.pageBtn, page <= 1 && styles.pageBtnDisabled]}
                  onPress={() => page > 1 && cargar(page - 1)}
                  disabled={page <= 1}
                >
                  <Text style={styles.pageBtnText}>Anterior</Text>
                </TouchableOpacity>
                <Text style={styles.pageInfo}>Pag. {page} / {totalPages}</Text>
                <TouchableOpacity
                  style={[styles.pageBtn, page >= totalPages && styles.pageBtnDisabled]}
                  onPress={() => page < totalPages && cargar(page + 1)}
                  disabled={page >= totalPages}
                >
                  <Text style={styles.pageBtnText}>Siguiente</Text>
                </TouchableOpacity>
              </View>
            ) : null
          }
          ListEmptyComponent={<Text style={styles.empty}>No hay usuarios registrados</Text>}
        />
      )}
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: Colors.background },
  list: { padding: 16 },
  card: {
    backgroundColor: Colors.surface, borderRadius: 14, padding: 14,
    marginBottom: 10, flexDirection: 'row', alignItems: 'center', gap: 14,
  },
  avatar: {
    width: 44, height: 44, borderRadius: 22,
    backgroundColor: Colors.primary, alignItems: 'center', justifyContent: 'center',
  },
  avatarText: { color: '#fff', fontWeight: '700', fontSize: 18 },
  info: { flex: 1 },
  login: { color: Colors.text, fontWeight: '700', fontSize: 15, marginBottom: 2 },
  roles: { color: Colors.primary, fontSize: 12, marginBottom: 2 },
  estado: { fontSize: 11, fontWeight: '600' },
  activo: { color: Colors.success },
  inactivo: { color: Colors.textMuted },
  empty: { color: Colors.textMuted, textAlign: 'center', marginTop: 40 },
  errorBox: { flex: 1, justifyContent: 'center', alignItems: 'center', padding: 24 },
  errorText: { color: Colors.danger, textAlign: 'center', marginBottom: 16, lineHeight: 22 },
  retryBtn: {
    paddingHorizontal: 20, paddingVertical: 10,
    borderRadius: 8, borderWidth: 1, borderColor: Colors.primary,
  },
  retryText: { color: Colors.primary, fontWeight: '600' },
  paginacion: {
    flexDirection: 'row', justifyContent: 'space-between',
    alignItems: 'center', padding: 16, marginTop: 4,
  },
  pageBtn: {
    paddingHorizontal: 16, paddingVertical: 8,
    borderRadius: 8, backgroundColor: Colors.surface,
    borderWidth: 1, borderColor: Colors.border,
  },
  pageBtnDisabled: { opacity: 0.4 },
  pageBtnText: { color: Colors.text, fontSize: 13, fontWeight: '600' },
  pageInfo: { color: Colors.textMuted, fontSize: 13 },
});
