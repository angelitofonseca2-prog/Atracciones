import apiClient from './client';

export async function listarMisFacturas() {
  const res = await apiClient.get('/facturas/mis-facturas');
  return res.data;
}

export async function obtenerFactura(facGuid: string) {
  const res = await apiClient.get(`/facturas/${facGuid}`);
  return res.data;
}
