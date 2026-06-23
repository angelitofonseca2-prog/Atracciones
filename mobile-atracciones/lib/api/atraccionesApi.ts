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

export async function obtenerFiltros(ciudad?: string) {
  const params = ciudad ? { ciudad } : {};
  const res = await apiClient.get('/atracciones/filtros', { params });
  return res.data;
}

export async function listarResenias(atGuid: string) {
  const res = await apiClient.get(`/atracciones/${atGuid}/resenias`);
  return res.data;
}

export async function crearResenia(atGuid: string, data: { calificacion: number; comentario: string }) {
  const res = await apiClient.post(`/atracciones/${atGuid}/resenias`, data);
  return res.data;
}
