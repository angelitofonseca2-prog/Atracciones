import apiClient from './client';

export interface LoginRequest { correo: string; contrasena: string }
export interface RegistroRequest {
  nombres: string; apellidos: string; correo: string; contrasena: string;
  tipo_identificacion: string; numero_identificacion: string; telefono?: string;
}

export async function login(data: LoginRequest) {
  const res = await apiClient.post('/auth/login', data);
  return res.data;
}

export async function registro(data: RegistroRequest) {
  const res = await apiClient.post('/auth/registro', data);
  return res.data;
}
