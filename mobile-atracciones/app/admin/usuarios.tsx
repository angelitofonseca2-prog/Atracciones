import React, { useCallback, useEffect, useState } from 'react';
import { FlatList, RefreshControl, StyleSheet, Text, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import Spinner from '@/components/ui/Spinner';
import { listarUsuarios } from '@/lib/api/adminApi';
import { Colors } from '@/constants/Colors';

interface Usuario { usu_guid?: string; nombre?: string; correo?: string; roles?: string[]; }

export default function AdminUsuariosScreen() {
  const [items, setItems] = useState<Usuario[]>([]);
  const [cargando, setCargando] = useState(true);
  const [refrescando, setRefrescando] = useState(false);

  const cargar = useCallback(async () => {
    try {
      const res = await listarUsuarios();
      const d = res?.data ?? res;
      setItems(Array.isArray(d) ? d : d?.items ?? d?.usuarios ?? []);
    } catch {} finally { setCargando(false); setRefrescando(false); }
  }, []);

  useEffect(() => { cargar(); }, [cargar]);

  if (cargando) return <Spinner texto="Cargando usuarios..." />;

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      <FlatList
        data={items}
        keyExtractor={(u) => String(u.usu_guid ?? Math.random())}
        contentContainerStyle={styles.list}
        refreshControl={<RefreshControl refreshing={refrescando} onRefresh={() => { setRefrescando(true); cargar(); }} tintColor={Colors.primary} />}
        renderItem={({ item: u }) => (
          <View style={styles.card}>
            <View style={styles.avatar}>
              <Text style={styles.avatarText}>{u.nombre?.charAt(0)?.toUpperCase() ?? '?'}</Text>
            </View>
            <View style={styles.info}>
              <Text style={styles.nombre}>{u.nombre ?? '—'}</Text>
              <Text style={styles.correo}>{u.correo ?? '—'}</Text>
              {u.roles?.length ? <Text style={styles.roles}>{u.roles.join(', ')}</Text> : null}
            </View>
          </View>
        )}
        ListEmptyComponent={<Text style={styles.empty}>No hay usuarios</Text>}
      />
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: Colors.background },
  list: { padding: 16 },
  card: { backgroundColor: Colors.surface, borderRadius: 14, padding: 14, marginBottom: 10, flexDirection: 'row', alignItems: 'center', gap: 14 },
  avatar: { width: 44, height: 44, borderRadius: 22, backgroundColor: Colors.primary, alignItems: 'center', justifyContent: 'center' },
  avatarText: { color: '#fff', fontWeight: '700', fontSize: 18 },
  info: { flex: 1 },
  nombre: { color: Colors.text, fontWeight: '700', fontSize: 15, marginBottom: 2 },
  correo: { color: Colors.textMuted, fontSize: 13 },
  roles: { color: Colors.primary, fontSize: 11, marginTop: 2 },
  empty: { color: Colors.textMuted, textAlign: 'center', marginTop: 40 },
});
