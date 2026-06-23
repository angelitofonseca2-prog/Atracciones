import React from 'react';
import { StyleSheet, Text, TouchableOpacity, View } from 'react-native';
import { Image } from 'expo-image';
import { router } from 'expo-router';
import { Colors } from '@/constants/Colors';

export interface Atraccion {
  at_guid?: string; Id?: string; Nombre?: string; nombre?: string;
  ciudad?: string; Ciudad?: string; pais?: string; Pais?: string;
  precio_desde?: number; PrecioDesde?: number;
  calificacion?: number; Calificacion?: number;
  imagen_principal?: string; ImagenPrincipal?: string;
  tipo_nombre?: string; TipoNombre?: string;
  descripcion_corta?: string; DescripcionCorta?: string;
}

function campo<T>(a: Atraccion, k1: keyof Atraccion, k2: keyof Atraccion): T {
  return (a[k1] ?? a[k2]) as T;
}

export default function TarjetaAtraccion({ item }: { item: Atraccion }) {
  const guid = campo<string>(item, 'at_guid', 'Id');
  const nombre = campo<string>(item, 'nombre', 'Nombre');
  const ciudad = campo<string>(item, 'ciudad', 'Ciudad');
  const precio = campo<number>(item, 'precio_desde', 'PrecioDesde');
  const imagen = campo<string>(item, 'imagen_principal', 'ImagenPrincipal');
  const calificacion = campo<number>(item, 'calificacion', 'Calificacion');
  const tipo = campo<string>(item, 'tipo_nombre', 'TipoNombre');

  return (
    <TouchableOpacity style={styles.card} activeOpacity={0.85} onPress={() => router.push(`/atracciones/${guid}`)}>
      <Image source={{ uri: imagen }} style={styles.image} contentFit="cover" transition={300} />
      <View style={styles.body}>
        {tipo ? <Text style={styles.tipo}>{tipo}</Text> : null}
        <Text style={styles.nombre} numberOfLines={2}>{nombre}</Text>
        <Text style={styles.ciudad}>📍 {ciudad}</Text>
        <View style={styles.footer}>
          {precio != null && <Text style={styles.precio}>Desde ${Number(precio).toFixed(2)}</Text>}
          {calificacion != null && <Text style={styles.cal}>⭐ {Number(calificacion).toFixed(1)}</Text>}
        </View>
      </View>
    </TouchableOpacity>
  );
}

const styles = StyleSheet.create({
  card: { backgroundColor: Colors.card, borderRadius: 14, overflow: 'hidden', marginBottom: 16, elevation: 2, shadowColor: '#000', shadowOpacity: 0.2, shadowRadius: 6, shadowOffset: { width: 0, height: 2 } },
  image: { width: '100%', height: 180 },
  body: { padding: 14 },
  tipo: { color: Colors.primary, fontSize: 11, fontWeight: '600', textTransform: 'uppercase', marginBottom: 4 },
  nombre: { color: Colors.text, fontSize: 17, fontWeight: '700', marginBottom: 6 },
  ciudad: { color: Colors.textMuted, fontSize: 13, marginBottom: 10 },
  footer: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center' },
  precio: { color: Colors.primary, fontWeight: '700', fontSize: 15 },
  cal: { color: Colors.textMuted, fontSize: 13 },
});
