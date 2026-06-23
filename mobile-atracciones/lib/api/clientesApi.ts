import apiClient from './client';

export async function obtenerPerfilCliente() {
  const res = await apiClient.get('/clientes/perfil');
  return res.data;
}

export async function actualizarPerfilCliente(data: Record<string, unknown>) {
  const res = await apiClient.put('/clientes/perfil', data);
  return res.data;
}
