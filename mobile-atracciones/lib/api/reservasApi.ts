import apiClient from './client';

function newIdempotencyKey(): string {
  if (typeof crypto !== 'undefined' && crypto.randomUUID) return crypto.randomUUID();
  return `${Date.now()}-${Math.random().toString(36).slice(2)}`;
}

export interface LineaReserva { tck_guid: string; cantidad: number }
export interface ClienteInvitado {
  tipo_identificacion: string; numero_identificacion: string;
  nombres?: string; apellidos?: string; correo: string; telefono?: string;
}
export interface CrearReservaPayload {
  at_guid: string; hor_guid: string;
  lineas: LineaReserva[]; origen_canal?: string;
  fecha_visita?: string; cliente_invitado?: ClienteInvitado;
}

export async function crearReserva(data: CrearReservaPayload) {
  const key = newIdempotencyKey();
  const res = await apiClient.post('/reservas', data, {
    headers: { 'Idempotency-Key': key },
  });
  return res.data;
}

export async function confirmarPagoReserva(revGuid: string) {
  const key = newIdempotencyKey();
  const res = await apiClient.post(`/reservas/${revGuid}/pagos/confirmacion`, {}, {
    headers: { 'Idempotency-Key': key },
  });
  return res.data;
}

export async function listarMisReservas() {
  const res = await apiClient.get('/reservas/mis-reservas');
  return res.data;
}

export async function obtenerReserva(revGuid: string) {
  const res = await apiClient.get(`/reservas/${revGuid}`);
  return res.data;
}

export async function cancelarReserva(revGuid: string) {
  const res = await apiClient.put(`/reservas/${revGuid}/cancelar`);
  return res.data;
}
