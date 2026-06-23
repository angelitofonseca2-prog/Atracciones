import React from 'react';
import { StyleSheet, Text, View } from 'react-native';
import { estadoColor, estadoLabel } from '@/lib/utils/estadoReserva';

interface BadgeProps { estado: string }

export default function Badge({ estado }: BadgeProps) {
  const color = estadoColor(estado);
  return (
    <View style={[styles.badge, { borderColor: color, backgroundColor: `${color}22` }]}>
      <Text style={[styles.text, { color }]}>{estadoLabel(estado)}</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  badge: { borderRadius: 20, borderWidth: 1, paddingHorizontal: 10, paddingVertical: 3, alignSelf: 'flex-start' },
  text: { fontSize: 12, fontWeight: '600' },
});
