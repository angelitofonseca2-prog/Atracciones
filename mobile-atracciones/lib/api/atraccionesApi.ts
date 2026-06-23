import apiClient from './client';

export interface FiltrosParams {
  ciudad?: string; tipo?: string; subtipo?: string; idioma?: string;
  calificacion_min?: number; disponible?: boolean; ordenar_por?: string;
  page?: number; limit?: number;
}

export async function listarAtracciones(params: FiltrosParams = {}) {
  const res = await apiClient.get('/atracciones', { params });
  return res.data;
}

export async function obtenerAtraccion(guid: string) {
  const res = await apiClient.get(`/atracciones/${guid}`);
  return res.data;
}

export async function obtenerFiltros() {
  const res = await apiClient.get('/atracciones/filtros');
  return res.data;
}

export async function obtenerHorariosDisponibles(atGuid: string) {
  const res = await apiClient.get(`/atracciones/${atGuid}/horarios`, {
    params: { disponibles: true },
  });
  return res.data;
}

export async function obtenerTicketsAtraccion(atGuid: string) {
  const res = await apiClient.get(`/atracciones/${atGuid}/tickets`);
  return res.data;
}

/** GET /atracciones/{atGuid}/horarios/{horGuid}/tickets — tickets exactos para ese horario */
export async function obtenerTicketsPorHorario(atGuid: string, horGuid: string) {
  const res = await apiClient.get(`/atracciones/${atGuid}/horarios/${horGuid}/tickets`);
  const raw = res.data as Record<string, unknown>;
  const d = raw?.data ?? raw;
  // El API puede devolver array directo, { items }, etc.
  if (Array.isArray(d)) return d;
  const dd = d as Record<string, unknown>;
  if (Array.isArray(dd?.items)) return dd.items;
  return [];
}

export async function listarResenias(atGuid: string) {
  const res = await apiClient.get(`/atracciones/${atGuid}/resenias`);
  return res.data;
}

/** POST /atracciones/{atGuid}/resenias — body: { rev_guid, rating, comentario } */
export async function crearResenia(atGuid: string, data: { rev_guid: string; rating: number; comentario: string }) {
  const res = await apiClient.post(`/atracciones/${atGuid}/resenias`, data);
  return res.data;
}
