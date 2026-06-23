import React, { useCallback, useEffect, useState } from 'react';
import { Alert, KeyboardAvoidingView, Platform, ScrollView, StyleSheet, Text, View } from 'react-native';
import { router } from 'expo-router';
import { SafeAreaView } from 'react-native-safe-area-context';
import Button from '@/components/ui/Button';
import Input from '@/components/ui/Input';
import Spinner from '@/components/ui/Spinner';
import { actualizarPerfilCliente, obtenerPerfilCliente } from '@/lib/api/clientesApi';
import { useAuth } from '@/lib/context/AuthContext';
import { Colors } from '@/constants/Colors';

export default function PerfilScreen() {
  const { user } = useAuth();
  const [perfil, setPerfil] = useState<Record<string, string>>({});
  const [cargando, setCargando] = useState(true);
  const [guardando, setGuardando] = useState(false);
  const [editando, setEditando] = useState(false);
  const [form, setForm] = useState<Record<string, string>>({});

  const cargar = useCallback(async () => {
    try {
      const res = await obtenerPerfilCliente();
      const d = (res?.data ?? res) as Record<string, string>;
      setPerfil(d);
      setForm(d);
    } catch {
      Alert.alert('Error', 'No se pudo cargar tu perfil');
    } finally {
      setCargando(false);
    }
  }, []);

  useEffect(() => {
    if (!user) { router.replace('/auth/login'); return; }
    cargar();
  }, [user, cargar]);

  const onGuardar = async () => {
    setGuardando(true);
    try {
      await actualizarPerfilCliente(form as Record<string, unknown>);
      setPerfil(form);
      setEditando(false);
      Alert.alert('Perfil actualizado');
    } catch {
      Alert.alert('Error', 'No se pudo actualizar el perfil');
    } finally {
      setGuardando(false);
    }
  };

  if (!user) return null;
  if (cargando) return <Spinner texto="Cargando perfil..." />;

  const set = (k: string) => (v: string) => setForm((p) => ({ ...p, [k]: v }));

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      <KeyboardAvoidingView behavior={Platform.OS === 'ios' ? 'padding' : undefined} style={{ flex: 1 }}>
        <ScrollView contentContainerStyle={styles.scroll} keyboardShouldPersistTaps="handled">
          {/* Avatar */}
          <View style={styles.avatarSection}>
            <View style={styles.avatar}>
              <Text style={styles.avatarText}>{(perfil.nombres ?? user.nombre)?.charAt(0)?.toUpperCase() ?? '?'}</Text>
            </View>
            <Text style={styles.nombre}>{[perfil.nombres, perfil.apellidos].filter(Boolean).join(' ') || user.nombre}</Text>
            <Text style={styles.correo}>{perfil.correo ?? user.correo}</Text>
          </View>

          <View style={styles.card}>
            {editando ? (
              <>
                <Input label="Nombres" value={form.nombres ?? ''} onChangeText={set('nombres')} />
                <Input label="Apellidos" value={form.apellidos ?? ''} onChangeText={set('apellidos')} />
                <Input label="Teléfono" value={form.telefono ?? ''} onChangeText={set('telefono')} keyboardType="phone-pad" />
                <View style={styles.botonesRow}>
                  <Button title="Cancelar" onPress={() => { setEditando(false); setForm(perfil); }} variant="ghost" style={{ flex: 1 }} />
                  <Button title="Guardar" onPress={onGuardar} loading={guardando} style={{ flex: 2 }} />
                </View>
              </>
            ) : (
              <>
                <InfoRow label="Nombres" valor={perfil.nombres ?? '—'} />
                <InfoRow label="Apellidos" valor={perfil.apellidos ?? '—'} />
                <InfoRow label="Correo" valor={perfil.correo ?? user.correo} />
                <InfoRow label="Teléfono" valor={perfil.telefono ?? '—'} />
                <InfoRow label="Tipo ID" valor={perfil.tipo_identificacion ?? '—'} />
                <InfoRow label="Número ID" valor={perfil.numero_identificacion ?? '—'} />
                <Button title="Editar perfil" onPress={() => setEditando(true)} variant="outline" style={{ marginTop: 8 }} />
              </>
            )}
          </View>
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}

function InfoRow({ label, valor }: { label: string; valor: string }) {
  return (
    <View style={rowStyles.row}>
      <Text style={rowStyles.label}>{label}</Text>
      <Text style={rowStyles.valor}>{valor}</Text>
    </View>
  );
}

const rowStyles = StyleSheet.create({
  row: { flexDirection: 'row', justifyContent: 'space-between', marginBottom: 12, borderBottomWidth: 1, borderBottomColor: Colors.border, paddingBottom: 12 },
  label: { color: Colors.textMuted, fontSize: 14 },
  valor: { color: Colors.text, fontSize: 14, fontWeight: '500', maxWidth: '60%', textAlign: 'right' },
});

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: Colors.background },
  scroll: { padding: 20 },
  avatarSection: { alignItems: 'center', marginBottom: 28 },
  avatar: { width: 80, height: 80, borderRadius: 40, backgroundColor: Colors.primary, alignItems: 'center', justifyContent: 'center', marginBottom: 12 },
  avatarText: { fontSize: 32, color: '#fff', fontWeight: '700' },
  nombre: { color: Colors.text, fontSize: 20, fontWeight: '700', marginBottom: 4 },
  correo: { color: Colors.textMuted, fontSize: 14 },
  card: { backgroundColor: Colors.surface, borderRadius: 16, padding: 16 },
  botonesRow: { flexDirection: 'row', gap: 10, marginTop: 8 },
});
