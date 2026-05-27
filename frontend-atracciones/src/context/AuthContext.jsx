import { createContext, useContext, useEffect, useMemo, useState } from 'react'

const AuthContext = createContext(null)

const normalizarRoles = (raw) => {
  if (Array.isArray(raw)) return raw.filter(Boolean).map((r) => String(r).trim())
  if (typeof raw === 'string' && raw.trim()) return [raw.trim()]
  return []
}

export function AuthProvider({ children }) {
  const estaExpirado = (tokenJWT) => {
    try {
      const base64Url = tokenJWT.split('.')[1]
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/')
      const padded = base64.padEnd(base64.length + ((4 - (base64.length % 4)) % 4), '=')
      const payload = JSON.parse(atob(padded))
      return typeof payload.exp === 'number' && payload.exp < Date.now() / 1000
    } catch {
      return false
    }
  }

  const [hydrated, setHydrated] = useState(false)
  const [usuario, setUsuario] = useState(() => {
    try {
      const usuarioGuardado = localStorage.getItem('usuario')
      if (!usuarioGuardado) return null
      const usuarioParseado = JSON.parse(usuarioGuardado)
      return {
        login: usuarioParseado?.login || '',
        roles: normalizarRoles(usuarioParseado?.roles ?? usuarioParseado?.rol),
      }
    } catch {
      return null
    }
  })
  const [token, setToken] = useState(() => {
    try {
      const tokenGuardado = localStorage.getItem('token')
      if (!tokenGuardado) return null
      if (estaExpirado(tokenGuardado)) return null
      return tokenGuardado
    } catch {
      return null
    }
  })

  useEffect(() => {
    const tokenGuardado = localStorage.getItem('token')
    const usuarioGuardado = localStorage.getItem('usuario')

    if (tokenGuardado) {
      if (estaExpirado(tokenGuardado)) {
        localStorage.removeItem('token')
        localStorage.removeItem('usuario')
        setToken(null)
        setUsuario(null)
        setHydrated(true)
        return
      }
      setToken(tokenGuardado)
    }
    if (usuarioGuardado) {
      try {
        const usuarioParseado = JSON.parse(usuarioGuardado)
        setUsuario({
          login: usuarioParseado?.login || '',
          roles: normalizarRoles(usuarioParseado?.roles ?? usuarioParseado?.rol),
        })
      } catch {
        localStorage.removeItem('usuario')
        setUsuario(null)
      }
    }
    setHydrated(true)
  }, [])

  const login = (nuevoToken, nuevoUsuario) => {
    const usuarioNormalizado = {
      login: nuevoUsuario?.login || '',
      roles: normalizarRoles(nuevoUsuario?.roles ?? nuevoUsuario?.rol),
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
      hydrated,
    }),
    [token, usuario, hydrated],
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
