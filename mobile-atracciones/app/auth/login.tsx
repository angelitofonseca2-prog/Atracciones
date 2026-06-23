import React, { useState } from 'react';
import { Alert, KeyboardAvoidingView, Platform, ScrollView, StyleSheet, Text, View } from 'react-native';
import { Link, router } from 'expo-router';
import { SafeAreaView } from 'react-native-safe-area-context';
import Button from '@/components/ui/Button';
import Input from '@/components/ui/Input';
import { useAuth } from '@/lib/context/AuthContext';
import { Colors } from '@/constants/Colors';

export default function LoginScreen() {
  const { iniciarSesion } = useAuth();
  const [correo, setCorreo] = useState('');
  const [contrasena, setContrasena] = useState('');
  const [cargando, setCargando] = useState(false);
  const [errores, setErrores] = useState<{ correo?: string; contrasena?: string }>({});

  const validar = () => {
    const e: typeof errores = {};
    if (!correo.trim()) e.correo = 'El correo es requerido';
    else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(correo)) e.correo = 'Correo inválido';
    if (!contrasena) e.contrasena = 'La contraseña es requerida';
    return e;
  };

  const handleLogin = async () => {
    const e = validar();
    if (Object.keys(e).length) { setErrores(e); return; }
    setCargando(true);
    try {
      await iniciarSesion({ login: correo.trim(), password: contrasena });
      router.replace('/(tabs)');
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message
        ?? 'Credenciales incorrectas. Verifica tu correo y contraseña.';
      Alert.alert('Error al iniciar sesión', msg);
    } finally {
      setCargando(false);
    }
  };

  return (
    <SafeAreaView style={styles.safe}>
      <KeyboardAvoidingView behavior={Platform.OS === 'ios' ? 'padding' : undefined} style={{ flex: 1 }}>
        <ScrollView contentContainerStyle={styles.scroll} keyboardShouldPersistTaps="handled">
          <View style={styles.header}>
            <Text style={styles.logo}>🗺 Atracciones</Text>
            <Text style={styles.title}>Bienvenido de nuevo</Text>
            <Text style={styles.subtitle}>Inicia sesión para gestionar tus reservas</Text>
          </View>

          <View style={styles.form}>
            <Input
              label="Correo electrónico"
              value={correo}
              onChangeText={(v) => { setCorreo(v); setErrores((p) => ({ ...p, correo: '' })); }}
              keyboardType="email-address"
              autoCapitalize="none"
              autoCorrect={false}
              placeholder="correo@ejemplo.com"
              error={errores.correo}
            />
            <Input
              label="Contraseña"
              value={contrasena}
              onChangeText={(v) => { setContrasena(v); setErrores((p) => ({ ...p, contrasena: '' })); }}
              secureTextEntry
              placeholder="••••••••"
              error={errores.contrasena}
            />
            <Button title="Iniciar sesión" onPress={handleLogin} loading={cargando} size="lg" style={{ marginTop: 8 }} />
          </View>

          <View style={styles.footer}>
            <Text style={styles.footerText}>¿No tienes cuenta? </Text>
            <Link href="/auth/registro" asChild>
              <Text style={styles.link}>Regístrate</Text>
            </Link>
          </View>
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: Colors.background },
  scroll: { flexGrow: 1, padding: 24, justifyContent: 'center' },
  header: { alignItems: 'center', marginBottom: 40 },
  logo: { fontSize: 36, marginBottom: 16 },
  title: { color: Colors.text, fontSize: 26, fontWeight: '700', marginBottom: 8 },
  subtitle: { color: Colors.textMuted, fontSize: 14, textAlign: 'center' },
  form: { gap: 4 },
  footer: { flexDirection: 'row', justifyContent: 'center', marginTop: 32 },
  footerText: { color: Colors.textMuted, fontSize: 14 },
  link: { color: Colors.primary, fontSize: 14, fontWeight: '600' },
});
