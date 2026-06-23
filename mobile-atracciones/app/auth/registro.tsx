import React, { useState } from 'react';
import { Alert, KeyboardAvoidingView, Platform, ScrollView, StyleSheet, Text, View } from 'react-native';
import { Link, router } from 'expo-router';
import { SafeAreaView } from 'react-native-safe-area-context';
import Button from '@/components/ui/Button';
import Input from '@/components/ui/Input';
import Select from '@/components/ui/Select';
import { registro } from '@/lib/api/authApi';
import { esEmailValido, esIdentificacionValida, esNombreValido, esTelefonoValido, mensajeIdentificacion, mensajeNombre, mensajeTelefono } from '@/lib/utils/validaciones';
import { Colors } from '@/constants/Colors';

const TIPOS_ID = [
  { label: 'Cédula', value: 'CEDULA' },
  { label: 'Pasaporte', value: 'PASAPORTE' },
  { label: 'RUC', value: 'RUC' },
  { label: 'Otro', value: 'OTRO' },
];

export default function RegistroScreen() {
  const [form, setForm] = useState({
    nombres: '', apellidos: '', correo: '', contrasena: '', confirmar: '',
    tipo_identificacion: 'CEDULA', numero_identificacion: '', telefono: '',
  });
  const [errores, setErrores] = useState<Record<string, string>>({});
  const [cargando, setCargando] = useState(false);

  const set = (campo: string) => (valor: string) => {
    setForm((p) => ({ ...p, [campo]: valor }));
    setErrores((p) => ({ ...p, [campo]: '' }));
  };

  const validar = () => {
    const e: Record<string, string> = {};
    if (!esNombreValido(form.nombres)) e.nombres = mensajeNombre('El nombre');
    if (!esNombreValido(form.apellidos)) e.apellidos = mensajeNombre('Los apellidos');
    if (!esEmailValido(form.correo)) e.correo = 'Correo inválido';
    if (form.contrasena.length < 6) e.contrasena = 'Mínimo 6 caracteres';
    if (form.contrasena !== form.confirmar) e.confirmar = 'Las contraseñas no coinciden';
    if (!esIdentificacionValida(form.tipo_identificacion, form.numero_identificacion))
      e.numero_identificacion = mensajeIdentificacion(form.tipo_identificacion);
    if (form.telefono && !esTelefonoValido(form.telefono)) e.telefono = mensajeTelefono();
    return e;
  };

  const handleRegistro = async () => {
    const e = validar();
    if (Object.keys(e).length) { setErrores(e); return; }
    setCargando(true);
    try {
      await registro({
        login: form.correo.trim(), password: form.contrasena,
        nombres: form.nombres.trim(), apellidos: form.apellidos.trim(),
        correo: form.correo.trim(),
        tipo_identificacion: form.tipo_identificacion,
        numero_identificacion: form.numero_identificacion.trim(),
        ...(form.telefono ? { telefono: form.telefono.trim() } : {}),
      });
      Alert.alert('¡Cuenta creada!', 'Tu cuenta fue creada. Inicia sesión.', [
        { text: 'Ir al login', onPress: () => router.replace('/auth/login') },
      ]);
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message
        ?? 'No se pudo crear la cuenta. Inténtalo de nuevo.';
      Alert.alert('Error', msg);
    } finally {
      setCargando(false);
    }
  };

  return (
    <SafeAreaView style={styles.safe}>
      <KeyboardAvoidingView behavior={Platform.OS === 'ios' ? 'padding' : undefined} style={{ flex: 1 }}>
        <ScrollView contentContainerStyle={styles.scroll} keyboardShouldPersistTaps="handled">
          <Text style={styles.title}>Crear cuenta</Text>
          <Text style={styles.subtitle}>Únete para reservar experiencias increíbles</Text>

          <View style={styles.form}>
            <Input label="Nombres *" value={form.nombres} onChangeText={set('nombres')} placeholder="Ej. Juan" error={errores.nombres} />
            <Input label="Apellidos *" value={form.apellidos} onChangeText={set('apellidos')} placeholder="Ej. Pérez" error={errores.apellidos} />
            <Input label="Correo electrónico *" value={form.correo} onChangeText={set('correo')} keyboardType="email-address" autoCapitalize="none" placeholder="correo@ejemplo.com" error={errores.correo} />
            <Select label="Tipo de identificación *" value={form.tipo_identificacion} onChange={set('tipo_identificacion')} options={TIPOS_ID} error={errores.tipo_identificacion} />
            <Input label="Número de identificación *" value={form.numero_identificacion} onChangeText={set('numero_identificacion')} keyboardType="numeric" placeholder="Ej. 1712345678" error={errores.numero_identificacion} />
            <Input label="Teléfono" value={form.telefono} onChangeText={set('telefono')} keyboardType="phone-pad" placeholder="0991234567" error={errores.telefono} />
            <Input label="Contraseña *" value={form.contrasena} onChangeText={set('contrasena')} secureTextEntry placeholder="Mínimo 6 caracteres" error={errores.contrasena} />
            <Input label="Confirmar contraseña *" value={form.confirmar} onChangeText={set('confirmar')} secureTextEntry placeholder="Repite tu contraseña" error={errores.confirmar} />
          </View>

          <Button title="Crear cuenta" onPress={handleRegistro} loading={cargando} size="lg" style={{ marginTop: 8 }} />

          <View style={styles.footer}>
            <Text style={styles.footerText}>¿Ya tienes cuenta? </Text>
            <Link href="/auth/login" asChild>
              <Text style={styles.link}>Inicia sesión</Text>
            </Link>
          </View>
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: Colors.background },
  scroll: { flexGrow: 1, padding: 24 },
  title: { color: Colors.text, fontSize: 26, fontWeight: '700', marginBottom: 6 },
  subtitle: { color: Colors.textMuted, fontSize: 14, marginBottom: 28 },
  form: { gap: 4 },
  footer: { flexDirection: 'row', justifyContent: 'center', marginTop: 28, marginBottom: 16 },
  footerText: { color: Colors.textMuted, fontSize: 14 },
  link: { color: Colors.primary, fontSize: 14, fontWeight: '600' },
});
