import React, { createContext, useCallback, useContext, useEffect, useState } from 'react';
import * as SecureStore from 'expo-secure-store';
import { router } from 'expo-router';
import { login as apiLogin, LoginRequest } from '../api/authApi';
import { setUnauthorizedHandler, TOKEN_KEY } from '../api/client';

interface User {
  guid: string;
  nombre: string;
  correo: string;
  roles: string[];
}

interface AuthContextValue {
  user: User | null;
  token: string | null;
  cargando: boolean;
  esAdmin: boolean;
  iniciarSesion: (data: LoginRequest) => Promise<void>;
  cerrarSesion: () => void;
}

const AuthContext = createContext<AuthContextValue | null>(null);

function decodeJwtPayload(token: string): Record<string, unknown> {
  try {
    const base64 = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
    const padding = '='.repeat((4 - (base64.length % 4)) % 4);
    const json = atob(base64 + padding);
    return JSON.parse(json);
  } catch {
    return {};
  }
}

function payloadToUser(payload: Record<string, unknown>): User {
  const roles: string[] = [];
  const r = payload['roles'] ?? payload['role'];
  if (Array.isArray(r)) roles.push(...r.map(String));
  else if (r) roles.push(String(r));
  return {
    guid: String(payload['sub'] ?? payload['usu_guid'] ?? ''),
    nombre: String(payload['nombre'] ?? payload['name'] ?? payload['correo'] ?? ''),
    correo: String(payload['correo'] ?? payload['email'] ?? ''),
    roles,
  };
}

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [cargando, setCargando] = useState(true);

  const cerrarSesion = useCallback(() => {
    SecureStore.deleteItemAsync(TOKEN_KEY).catch(() => {});
    setUser(null);
    setToken(null);
    router.replace('/auth/login');
  }, []);

  // Restaurar sesión al arrancar
  useEffect(() => {
    SecureStore.getItemAsync(TOKEN_KEY)
      .then((t) => {
        if (t) {
          const payload = decodeJwtPayload(t);
          const exp = payload['exp'] as number | undefined;
          if (exp && exp * 1000 < Date.now()) {
            SecureStore.deleteItemAsync(TOKEN_KEY);
            return;
          }
          setToken(t);
          setUser(payloadToUser(payload));
        }
      })
      .finally(() => setCargando(false));
  }, []);

  // Handler de 401 global
  useEffect(() => {
    setUnauthorizedHandler(cerrarSesion);
  }, [cerrarSesion]);

  const iniciarSesion = async (data: LoginRequest) => {
    const res = await apiLogin(data);
    const jwt: string = res?.data?.token ?? res?.token ?? '';
    if (!jwt) throw new Error('No se recibió token');
    await SecureStore.setItemAsync(TOKEN_KEY, jwt);
    const payload = decodeJwtPayload(jwt);
    setToken(jwt);
    setUser(payloadToUser(payload));
  };

  const esAdmin = user?.roles.some((r) =>
    ['ADMIN', 'ADMINISTRADOR'].includes(r.toUpperCase()),
  ) ?? false;

  return (
    <AuthContext.Provider value={{ user, token, cargando, esAdmin, iniciarSesion, cerrarSesion }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth debe usarse dentro de AuthProvider');
  return ctx;
}
