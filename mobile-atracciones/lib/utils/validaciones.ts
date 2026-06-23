const EMAIL_RE = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

export const esEmailValido = (valor: string) => EMAIL_RE.test(String(valor || '').trim());

export const esTelefonoValido = (valor: string) => {
  const v = String(valor || '').trim();
  if (!v) return true;
  return /^09\d{8}$/.test(v);
};

export const mensajeTelefono = () =>
  'El teléfono debe tener exactamente 10 dígitos y comenzar con 09 (ej. 0991234567).';

export const esNombreValido = (valor: string) => {
  const v = String(valor || '').trim();
  return v.length >= 2 && /^[A-Za-záéíóúÁÉÍÓÚüÜñÑ\s]+$/.test(v);
};

export const mensajeNombre = (campo = 'El campo') =>
  `${campo} debe tener al menos 2 caracteres y solo puede contener letras, espacios y tildes.`;

export const esIdentificacionValida = (tipo: string, valor: string) => {
  const v = String(valor || '').trim();
  if (!v) return false;
  switch ((tipo || '').toUpperCase()) {
    case 'CEDULA':
    case 'CC':
      return /^\d{10}$/.test(v);
    case 'RUC':
      return /^\d{13}$/.test(v) && v.endsWith('001');
    case 'PASAPORTE':
      return /^[A-Za-z0-9]{5,20}$/.test(v);
    default:
      return /^[A-Za-z0-9]{4,20}$/.test(v);
  }
};

export const mensajeIdentificacion = (tipo: string) => {
  switch ((tipo || '').toUpperCase()) {
    case 'CEDULA':
    case 'CC':
      return 'La cédula debe tener exactamente 10 dígitos.';
    case 'RUC':
      return 'El RUC debe tener exactamente 13 dígitos y terminar en 001.';
    case 'PASAPORTE':
      return 'El pasaporte debe tener entre 5 y 20 caracteres alfanuméricos.';
    default:
      return 'Identificación inválida (4-20 caracteres alfanuméricos).';
  }
};
