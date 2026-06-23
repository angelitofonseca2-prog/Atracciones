import React from 'react';
import { ScrollView, StyleSheet, Text, TouchableOpacity, View } from 'react-native';
import { Colors } from '@/constants/Colors';

interface ChipsSelectorProps<T> {
  titulo: string;
  subtitulo?: string;
  items: T[];
  selected: string[];
  onChange: (guids: string[]) => void;
  getId: (item: T) => string;
  getLabel: (item: T) => string;
  vacio?: string;
  error?: string;
}

export default function ChipsSelector<T>({
  titulo,
  subtitulo,
  items,
  selected,
  onChange,
  getId,
  getLabel,
  vacio = 'No hay elementos disponibles.',
  error,
}: ChipsSelectorProps<T>) {
  const toggle = (id: string) => {
    if (selected.includes(id)) {
      onChange(selected.filter((g) => g !== id));
    } else {
      onChange([...selected, id]);
    }
  };

  return (
    <View style={styles.container}>
      <Text style={styles.titulo}>{titulo}</Text>
      {subtitulo && <Text style={styles.subtitulo}>{subtitulo}</Text>}
      {items.length === 0 ? (
        <Text style={styles.vacio}>{vacio}</Text>
      ) : (
        <ScrollView horizontal showsHorizontalScrollIndicator={false} style={styles.scroll} contentContainerStyle={styles.chips}>
          {items.map((item) => {
            const id = getId(item);
            if (!id) return null;
            const activo = selected.includes(id);
            return (
              <TouchableOpacity
                key={id}
                style={[styles.chip, activo && styles.chipActivo]}
                onPress={() => toggle(id)}
                activeOpacity={0.75}
              >
                <Text style={[styles.chipText, activo && styles.chipTextActivo]}>
                  {activo ? '✓ ' : ''}{getLabel(item)}
                </Text>
              </TouchableOpacity>
            );
          })}
        </ScrollView>
      )}
      {error ? <Text style={styles.errorText}>{error}</Text> : null}
    </View>
  );
}

const styles = StyleSheet.create({
  container: { marginBottom: 16 },
  titulo: { color: Colors.text, fontWeight: '700', fontSize: 14, marginBottom: 4 },
  subtitulo: { color: Colors.textMuted, fontSize: 12, marginBottom: 8 },
  vacio: { color: Colors.textMuted, fontSize: 13, fontStyle: 'italic' },
  scroll: { maxHeight: 120 },
  chips: { flexDirection: 'row', flexWrap: 'wrap', gap: 8, paddingBottom: 4 },
  chip: {
    paddingHorizontal: 14, paddingVertical: 8, borderRadius: 20,
    backgroundColor: Colors.surface, borderWidth: 1, borderColor: Colors.border,
  },
  chipActivo: { backgroundColor: Colors.primary, borderColor: Colors.primary },
  chipText: { color: Colors.textMuted, fontSize: 13 },
  chipTextActivo: { color: '#fff', fontWeight: '600' },
  errorText: { color: Colors.danger, fontSize: 12, marginTop: 4 },
});
