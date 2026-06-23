import apiClient from './client';

const extract = (r: unknown): unknown => {
  const raw = r as Record<string, unknown>;
  return raw?.data ?? r;
};

const extractArr = (r: unknown): unknown[] => {
  const d = extract(r);
  return Array.isArray(d) ? d : [];
};

// ── Atracciones ───────────────────────────────────────────────────────────────
export const listarAtraccionesAdmin = () =>
  apiClient.get('/admin/atracciones').then((r) => r.data);

export const listarTodasAtraccionesAdmin = async (): Promise<Record<string, unknown>[]> => {
  const LIMIT = 50;
  let page = 1;
  let todas: Record<string, unknown>[] = [];
  let totalPages = 1;
  do {
    const r = await apiClient.get('/admin/atracciones', { params: { page, limit: LIMIT } });
    const raw = r.data as Record<string, unknown>;
    const data = extractArr(raw.data ?? raw);
    const pagination = (raw.pagination ?? {}) as Record<string, unknown>;
    todas = [...todas, ...(data as Record<string, unknown>[])];
    totalPages = Number(pagination.total_pages ?? pagination.totalPages ?? 1);
    page++;
  } while (page <= totalPages);
  return todas;
};

export const obtenerAtraccionAdmin = (guid: string) =>
  apiClient.get(`/admin/atracciones/${guid}`).then((r) => extract(r.data));

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

export const actualizarDestino = (guid: string, data: Record<string, unknown>) =>
  apiClient.put(`/admin/destinos/${guid}`, data).then((r) => r.data);

export const eliminarDestino = (guid: string) =>
  apiClient.delete(`/admin/destinos/${guid}`).then((r) => r.data);

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

// ── Imágenes (/admin/imagenes) ────────────────────────────────────────────────
export const listarImagenes = () =>
  apiClient.get('/admin/imagenes').then((r) => r.data);

export const crearImagen = (data: { url: string; descripcion?: string }) =>
  apiClient.post('/admin/imagenes', data).then((r) => {
    const raw = r.data as Record<string, unknown>;
    return (raw?.data ?? raw) as Record<string, unknown>;
  });
