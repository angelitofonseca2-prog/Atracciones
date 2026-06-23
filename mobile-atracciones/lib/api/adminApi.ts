import apiClient from './client';

// ── Atracciones ──────────────────────────────────────────────────────────────
export const listarAtraccionesAdmin = () =>
  apiClient.get('/admin/atracciones').then((r) => r.data);

export const crearAtraccion = (data: Record<string, unknown>) =>
  apiClient.post('/admin/atracciones', data).then((r) => r.data);

export const actualizarAtraccion = (guid: string, data: Record<string, unknown>) =>
  apiClient.put(`/admin/atracciones/${guid}`, data).then((r) => r.data);

export const eliminarAtraccion = (guid: string) =>
  apiClient.delete(`/admin/atracciones/${guid}`).then((r) => r.data);

// ── Tickets ───────────────────────────────────────────────────────────────────
export const listarTicketsAdmin = () =>
  apiClient.get('/admin/tickets').then((r) => r.data);

export const listarTicketsDeAtraccion = (atGuid: string) =>
  apiClient.get(`/admin/atracciones/${atGuid}/tickets`).then((r) => r.data);

export const crearTicket = (data: Record<string, unknown>) =>
  apiClient.post('/admin/tickets', data).then((r) => r.data);

export const actualizarTicket = (guid: string, data: Record<string, unknown>) =>
  apiClient.put(`/admin/tickets/${guid}`, data).then((r) => r.data);

// ── Horarios ──────────────────────────────────────────────────────────────────
export const listarHorariosAdmin = () =>
  apiClient.get('/admin/horarios').then((r) => r.data);

export const crearHorario = (data: Record<string, unknown>) =>
  apiClient.post('/admin/tickets/horarios', data).then((r) => r.data);

export const actualizarHorario = (guid: string, data: Record<string, unknown>) =>
  apiClient.put(`/admin/horarios/${guid}`, data).then((r) => r.data);

// ── Reservas admin ────────────────────────────────────────────────────────────
export const listarReservasAdmin = (params?: Record<string, unknown>) =>
  apiClient.get('/admin/reservas', { params }).then((r) => r.data);

// ── Usuarios ──────────────────────────────────────────────────────────────────
export const listarUsuarios = () =>
  apiClient.get('/admin/usuarios').then((r) => r.data);

// ── Catálogos ─────────────────────────────────────────────────────────────────
export const listarDestinos = () =>
  apiClient.get('/admin/destinos').then((r) => r.data);

export const crearDestino = (data: Record<string, unknown>) =>
  apiClient.post('/admin/destinos', data).then((r) => r.data);

export const eliminarDestino = (id: number) =>
  apiClient.delete(`/admin/destinos/${id}`).then((r) => r.data);

export const listarCategorias = () =>
  apiClient.get('/admin/categorias').then((r) => r.data);

export const crearCategoria = (data: Record<string, unknown>) =>
  apiClient.post('/admin/categorias', data).then((r) => r.data);

export const listarIdiomas = () =>
  apiClient.get('/admin/idiomas').then((r) => r.data);

export const crearIdioma = (data: Record<string, unknown>) =>
  apiClient.post('/admin/idiomas', data).then((r) => r.data);

export const listarIncluye = () =>
  apiClient.get('/admin/incluye').then((r) => r.data);

export const crearIncluye = (data: Record<string, unknown>) =>
  apiClient.post('/admin/incluye', data).then((r) => r.data);
