import { createContext, useContext, useEffect, useMemo, useState } from 'react'

const AuthContext = createContext(null)

export function AuthProvider({ children }) {
  const [usuario, setUsuario] = useState(null)
  const [token, setToken] = useState(null)

  const estaExpirado = (tokenJWT) => {
    try {
      // JWT usa base64url (- y _ en lugar de + y /) sin padding.
      // atob solo acepta base64 estándar, así que se normaliza antes de decodificar.
      const base64Url = tokenJWT.split('.')[1]
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/')
      const padded = base64.padEnd(base64.length + ((4 - (base64.length % 4)) % 4), '=')
      const payload = JSON.parse(atob(padded))
      return typeof payload.exp === 'number' && payload.exp < Date.now() / 1000
    } catch {
      // Si no se puede decodificar el token, dejarlo pasar
      // (el servidor lo rechazará con 401 si es inválido)
      return false
    }
  }

  useEffect(() => {
    const tokenGuardado = localStorage.getItem('token')
    const usuarioGuardado = localStorage.getItem('usuario')

    if (tokenGuardado) {
      if (estaExpirado(tokenGuardado)) {
        localStorage.removeItem('token')
        localStorage.removeItem('usuario')
        return
      }
      setToken(tokenGuardado)
    }
    if (usuarioGuardado) {
      try {
        const usuarioParseado = JSON.parse(usuarioGuardado)
        setUsuario({
          login: usuarioParseado?.login || '',
          roles:
            usuarioParseado?.roles ||
            (usuarioParseado?.rol ? [usuarioParseado.rol] : []),
        })
      } catch {
        localStorage.removeItem('usuario')
      }
    }
  }, [])

  const login = (nuevoToken, nuevoUsuario) => {
    const usuarioNormalizado = {
      login: nuevoUsuario?.login || '',
      roles: nuevoUsuario?.roles || [],
    }
    setToken(nuevoToken)
    setUsuario(usuarioNormalizado)
    localStorage.setItem('token', nuevoToken)
    localStorage.setItem('usuario', JSON.stringify(usuarioNormalizado))
  }

  const logout = () => {
    setToken(null)
    setUsuario(null)
    localStorage.removeItem('token')
    localStorage.removeItem('usuario')
  }

  const value = useMemo(
    () => ({
      usuario,
      token,
      login,
      logout,
      estaAutenticado: Boolean(token),
    }),
    [token, usuario],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuthContext() {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuthContext debe usarse dentro de AuthProvider')
  }
  return context
}
