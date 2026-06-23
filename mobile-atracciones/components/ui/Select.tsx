import React, { useState } from 'react';
import { FlatList, Modal, StyleSheet, Text, TouchableOpacity, View } from 'react-native';
import { Colors } from '@/constants/Colors';

interface SelectProps {
  label?: string;
  value: string;
  onChange: (v: string) => void;
  options: { label: string; value: string }[];
  placeholder?: string;
  error?: string;
}

export default function Select({ label, value, onChange, options, placeholder = 'Selecciona...', error }: SelectProps) {
  const [open, setOpen] = useState(false);
  const selected = options.find((o) => o.value === value);

  return (
    <View style={styles.container}>
      {label && <Text style={styles.label}>{label}</Text>}
      <TouchableOpacity
        style={[styles.trigger, error ? styles.triggerError : null]}
        onPress={() => setOpen(true)}
        activeOpacity={0.8}
      >
        <Text style={selected ? styles.selectedText : styles.placeholder}>
          {selected ? selected.label : placeholder}
        </Text>
        <Text style={styles.arrow}>▾</Text>
      </TouchableOpacity>
      {error ? <Text style={styles.errorText}>{error}</Text> : null}

      <Modal visible={open} transparent animationType="fade" onRequestClose={() => setOpen(false)}>
        <TouchableOpacity style={styles.overlay} onPress={() => setOpen(false)} activeOpacity={1}>
          <View style={styles.sheet}>
            {label && <Text style={styles.sheetTitle}>{label}</Text>}
            <FlatList
              data={options}
              keyExtractor={(o) => o.value}
              renderItem={({ item }) => (
                <TouchableOpacity
                  style={[styles.option, item.value === value && styles.optionSelected]}
                  onPress={() => { onChange(item.value); setOpen(false); }}
                >
                  <Text style={[styles.optionText, item.value === value && styles.optionTextSelected]}>
                    {item.label}
                  </Text>
                </TouchableOpacity>
              )}
            />
          </View>
        </TouchableOpacity>
      </Modal>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { marginBottom: 14 },
  label: { color: Colors.textMuted, fontSize: 13, marginBottom: 6, fontWeight: '500' },
  trigger: {
    backgroundColor: Colors.surface, borderRadius: 8, borderWidth: 1, borderColor: Colors.border,
    paddingHorizontal: 14, paddingVertical: 12, flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center',
  },
  triggerError: { borderColor: Colors.danger },
  selectedText: { color: Colors.text, fontSize: 15 },
  placeholder: { color: Colors.textMuted, fontSize: 15 },
  arrow: { color: Colors.textMuted, fontSize: 14 },
  errorText: { color: Colors.danger, fontSize: 12, marginTop: 4 },
  overlay: { flex: 1, backgroundColor: '#00000088', justifyContent: 'flex-end' },
  sheet: { backgroundColor: Colors.surface, borderTopLeftRadius: 20, borderTopRightRadius: 20, padding: 20, maxHeight: '60%' },
  sheetTitle: { color: Colors.text, fontWeight: '700', fontSize: 16, marginBottom: 16 },
  option: { paddingVertical: 14, paddingHorizontal: 8, borderBottomWidth: 1, borderBottomColor: Colors.border },
  optionSelected: { backgroundColor: `${Colors.primary}22` },
  optionText: { color: Colors.text, fontSize: 15 },
  optionTextSelected: { color: Colors.primary, fontWeight: '600' },
});
