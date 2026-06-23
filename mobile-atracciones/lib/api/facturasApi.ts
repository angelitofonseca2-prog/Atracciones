import apiClient from './client';

export async function listarMisFacturas() {
  const res = await apiClient.get('/facturas/mis-facturas');
  return res.data;
}
