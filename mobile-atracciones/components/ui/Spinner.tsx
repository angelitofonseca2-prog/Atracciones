import React from 'react';
import { ActivityIndicator, StyleSheet, Text, View } from 'react-native';
import { Colors } from '@/constants/Colors';

export default function Spinner({ texto = 'Cargando...' }: { texto?: string }) {
  return (
    <View style={styles.container}>
      <ActivityIndicator color={Colors.primary} size="large" />
      {texto ? <Text style={styles.text}>{texto}</Text> : null}
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, alignItems: 'center', justifyContent: 'center', gap: 12, padding: 40 },
  text: { color: Colors.textMuted, fontSize: 14 },
});
