import React from 'react';
import { ActivityIndicator, StyleSheet, Text, TouchableOpacity, TouchableOpacityProps } from 'react-native';
import { Colors } from '@/constants/Colors';

interface ButtonProps extends TouchableOpacityProps {
  title: string;
  variant?: 'primary' | 'outline' | 'danger' | 'ghost';
  loading?: boolean;
  size?: 'sm' | 'md' | 'lg';
}

export default function Button({ title, variant = 'primary', loading, size = 'md', style, disabled, ...props }: ButtonProps) {
  const isDisabled = disabled || loading;

  const containerStyle = [
    styles.base,
    size === 'sm' && styles.sm,
    size === 'lg' && styles.lg,
    variant === 'primary' && styles.primary,
    variant === 'outline' && styles.outline,
    variant === 'danger' && styles.danger,
    variant === 'ghost' && styles.ghost,
    isDisabled && styles.disabled,
    style,
  ];

  const textStyle = [
    styles.text,
    size === 'sm' && styles.textSm,
    size === 'lg' && styles.textLg,
    variant === 'outline' && styles.textOutline,
    variant === 'ghost' && styles.textGhost,
    variant === 'danger' && styles.textDanger,
  ];

  return (
    <TouchableOpacity style={containerStyle} disabled={isDisabled} activeOpacity={0.8} {...props}>
      {loading ? (
        <ActivityIndicator color={variant === 'outline' ? Colors.primary : '#fff'} size="small" />
      ) : (
        <Text style={textStyle}>{title}</Text>
      )}
    </TouchableOpacity>
  );
}

const styles = StyleSheet.create({
  base: { borderRadius: 8, alignItems: 'center', justifyContent: 'center', paddingVertical: 12, paddingHorizontal: 20, flexDirection: 'row', gap: 8 },
  sm: { paddingVertical: 8, paddingHorizontal: 14 },
  lg: { paddingVertical: 16, paddingHorizontal: 28 },
  primary: { backgroundColor: Colors.primary },
  outline: { backgroundColor: 'transparent', borderWidth: 1.5, borderColor: Colors.primary },
  danger: { backgroundColor: 'transparent', borderWidth: 1.5, borderColor: Colors.danger },
  ghost: { backgroundColor: Colors.surface },
  disabled: { opacity: 0.5 },
  text: { color: '#fff', fontWeight: '600', fontSize: 15 },
  textSm: { fontSize: 13 },
  textLg: { fontSize: 17 },
  textOutline: { color: Colors.primary },
  textGhost: { color: Colors.text },
  textDanger: { color: Colors.danger },
});
