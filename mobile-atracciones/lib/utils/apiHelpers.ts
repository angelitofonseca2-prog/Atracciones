/**
 * El API siempre devuelve { status, message, data: T }.
 * Esta función extrae el `data` independientemente de la estructura.
 */
export function extractData<T = unknown>(response: unknown): T {
  if (!response) return [] as unknown as T;
  const r = response as Record<string, unknown>;
  // axios wraps in res.data → { status, message, data: T }
  if (r.data !== undefined && r.status !== undefined) return r.data as T;
  // Si es directamente el array o el objeto
  return response as T;
}

export function extractArray<T = unknown>(response: unknown): T[] {
  const data = extractData<unknown>(response);
  if (Array.isArray(data)) return data as T[];
  const d = data as Record<string, unknown>;
  if (Array.isArray(d?.items)) return d.items as T[];
  if (Array.isArray(d?.atracciones)) return d.atracciones as T[];
  if (Array.isArray(d?.reservas)) return d.reservas as T[];
  if (Array.isArray(d?.facturas)) return d.facturas as T[];
  if (Array.isArray(d?.horarios)) return d.horarios as T[];
  if (Array.isArray(d?.tickets)) return d.tickets as T[];
  return [];
}
