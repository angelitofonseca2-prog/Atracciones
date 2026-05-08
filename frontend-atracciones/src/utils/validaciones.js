/**
 * Validaciones cliente comunes para el frontend.
 *
 * Estas validaciones complementan las del backend; no lo reemplazan.
 * El backend siempre tiene la última palabra.
 */

const EMAIL_RE = /^[^\s@]+@[^\s@]+\.[^\s@]+$/

/** Devuelve true si la cadena tiene formato de correo electrónico válido. */
export const esEmailValido = (valor) => EMAIL_RE.test(String(valor || '').trim())

/**
 * Teléfono ecuatoriano: exactamente 10 dígitos, debe iniciar con 09.
 * Si no se proporciona, se considera válido (campo opcional).
 */
export const esTelefonoValido = (valor) => {
  const v = String(valor || '').trim()
  if (!v) return true
  return /^09\d{8}$/.test(v)
}

export const mensajeTelefono = () =>
  'El teléfono debe tener exactamente 10 dígitos y comenzar con 09 (ej. 0991234567).'

/**
 * Nombre o apellido: solo letras, espacios y tildes; mínimo 2 caracteres.
 */
export const esNombreValido = (valor) => {
  const v = String(valor || '').trim()
  return v.length >= 2 && /^[A-Za-záéíóúÁÉÍÓÚüÜñÑ\s]+$/.test(v)
}

export const mensajeNombre = (campo = 'El campo') =>
  `${campo} debe tener al menos 2 caracteres y solo puede contener letras, espacios y tildes.`

/**
 * Validación de identificaciones según el tipo seleccionado.
 *  - CEDULA:    10 dígitos exactos, solo números.
 *  - RUC:       13 dígitos, solo números, debe terminar en 001.
 *  - PASAPORTE: alfanumérico, entre 5 y 20 caracteres.
 *  - OTRO:      alfanumérico, 4-20 caracteres.
 */
export const esIdentificacionValida = (tipo, valor) => {
  const v = String(valor || '').trim()
  if (!v) return false
  switch ((tipo || '').toUpperCase()) {
    case 'CEDULA':
    case 'CC':
      return /^\d{10}$/.test(v)
    case 'RUC':
      return /^\d{13}$/.test(v) && v.endsWith('001')
    case 'PASAPORTE':
      return /^[A-Za-z0-9]{5,20}$/.test(v)
    default:
      return /^[A-Za-z0-9]{4,20}$/.test(v)
  }
}

/** Devuelve un mensaje de error para identificación inválida según el tipo. */
export const mensajeIdentificacion = (tipo) => {
  switch ((tipo || '').toUpperCase()) {
    case 'CEDULA':
    case 'CC':
      return 'La cédula debe tener exactamente 10 dígitos.'
    case 'RUC':
      return 'El RUC debe tener exactamente 13 dígitos y terminar en 001 (ej. 1790000000001).'
    case 'PASAPORTE':
      return 'El pasaporte debe tener entre 5 y 20 caracteres alfanuméricos.'
    default:
      return 'Identificación inválida (4-20 caracteres alfanuméricos).'
  }
}
