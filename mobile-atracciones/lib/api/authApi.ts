import apiClient from './client';

export interface LoginRequest { login: string; password: string }
export interface RegistroRequest {
  login: string; password: string;
  nombres: string; apellidos: string; correo: string;
  tipo_identificacion: string; numero_identificacion: string; telefono?: string;
}

export interface AuthData { token: string; login: string; roles: string[] }

export async function login(data: LoginRequest) {
  const res = await apiClient.post('/auth/login', {
    login: data.login,
    password: data.password,
  });
  return res.data;
}

export async function registro(data: RegistroRequest) {
  const body: Record<string, unknown> = {
    login: data.login,
    password: data.password,
    tipo_identificacion: data.tipo_identificacion,
    numero_identificacion: data.numero_identificacion,
    nombres: data.nombres,
    apellidos: data.apellidos,
    correo: data.correo,
  };
  if (data.telefono) body.telefono = data.telefono;
  const res = await apiClient.post('/auth/registro', body);
  return res.data;
}
