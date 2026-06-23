import apiClient from './client';

function newIdempotencyKey(): string {
  if (typeof crypto !== 'undefined' && crypto.randomUUID) return crypto.randomUUID();
  return `${Date.now()}-${Math.random().toString(36).slice(2)}`;
}

export interface LineaReserva { tck_guid: string; cantidad: number }

export interface CrearReservaPayload {
  at_guid: string;
  hor_guid: string;
  lineas: LineaReserva[];
  origen_canal?: string;
  fecha_visita?: string;
}

export interface DatosReceptor {
  nombre_receptor: string;
  correo_receptor: string;
  apellido_receptor?: string;
  telefono_receptor?: string;
  observacion?: string;
}

export async function crearReserva(data: CrearReservaPayload) {
  const key = newIdempotencyKey();
  const res = await apiClient.post('/reservas', data, {
    headers: { 'Idempotency-Key': key },
  });
  return res.data;
}

export async function confirmarPagoReserva(revGuid: string, datos: DatosReceptor) {
  const key = newIdempotencyKey();
  const body = {
    rev_guid: revGuid,
    nombre_receptor: datos.nombre_receptor,
    correo_receptor: datos.correo_receptor,
    ...(datos.apellido_receptor ? { apellido_receptor: datos.apellido_receptor } : {}),
    ...(datos.telefono_receptor ? { telefono_receptor: datos.telefono_receptor } : {}),
    ...(datos.observacion ? { observacion: datos.observacion } : {}),
  };
  const res = await apiClient.post(`/reservas/${revGuid}/pagos/confirmacion`, body, {
    headers: { 'Idempotency-Key': key },
  });
  return res.data;
}

/** GET /reservas — lista las reservas del cliente autenticado */
export async function listarMisReservas() {
  const res = await apiClient.get('/reservas');
  return res.data;
}

export async function obtenerReserva(revGuid: string) {
  const res = await apiClient.get(`/reservas/${revGuid}`);
  return res.data;
}

export async function cancelarReserva(revGuid: string, motivo = 'Cancelado por el cliente') {
  const res = await apiClient.put(`/reservas/${revGuid}/cancelar`, { motivo });
  return res.data;
}
