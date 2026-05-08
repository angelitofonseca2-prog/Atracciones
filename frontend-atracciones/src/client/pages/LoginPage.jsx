import { useState } from 'react'
import { Link, Navigate, useLocation, useNavigate } from 'react-router-dom'
import ErrorMessage from '../../components/common/ErrorMessage'
import { emitirToast } from '../../components/common/Toast'
import { useAuthContext } from '../../context/AuthContext'
import { esEmailValido } from '../../utils/validaciones'
import { useAuth } from '../hooks/useAuth'

/**
 * El backend espera `{ login, password }` y `login` debe ser el correo
 * registrado. El input se llama "correo" para reflejar la realidad.
 */
function LoginPage() {
  const [form, setForm] = useState({ correo: '', password: '' })
  const [errores, setErrores] = useState({})
  const { estaAutenticado } = useAuthContext()
  const { cargando, error, iniciarSesion } = useAuth()
  const location = useLocation()
  const navigate = useNavigate()

  const destino = location.state?.from?.pathname || '/atracciones'

  if (estaAutenticado) return <Navigate to={destino} replace />

  const validar = () => {
    const e = {}
    if (!form.correo.trim()) e.correo = 'El correo electrónico es obligatorio'
    else if (!esEmailValido(form.correo)) e.correo = 'Ingresa un correo electrónico válido'
    if (!form.password) e.password = 'La contraseña es obligatoria'
    return e
  }

  const handleSubmit = async (event) => {
    event.preventDefault()
    const e = validar()
    if (Object.keys(e).length) { setErrores(e); return }
    setErrores({})
    try {
      await iniciarSesion(form.correo.trim(), form.password)
      emitirToast('Sesión iniciada correctamente.', 'success')
      navigate(destino, { replace: true })
    } catch {
      // El error queda guardado por el hook; el toast lo dispara el interceptor
      // global solo en 5xx. Para credenciales inválidas el mensaje se renderiza
      // en línea con ErrorMessage.
    }
  }

  const set = (campo) => (ev) => {
    setForm((prev) => ({ ...prev, [campo]: ev.target.value }))
    if (errores[campo]) setErrores((prev) => ({ ...prev, [campo]: '' }))
  }

  return (
    <section className="auth-page">
      <div className="auth-card fade-in">
        <h1>Iniciar sesión</h1>
        <p className="auth-subtitle">Bienvenido de vuelta. Ingresa tus datos.</p>

        <form onSubmit={handleSubmit} className="form-grid" noValidate>
          <div className="form-group">
            <label htmlFor="correo">Correo electrónico</label>
            <input
              id="correo"
              type="email"
              value={form.correo}
              onChange={set('correo')}
              className={errores.correo ? 'input-error' : ''}
              autoComplete="username"
              placeholder="correo@ejemplo.com"
            />
            {errores.correo && <span className="field-error">⚠ {errores.correo}</span>}
          </div>

          <div className="form-group">
            <label htmlFor="password">Contraseña</label>
            <input
              id="password"
              type="password"
              value={form.password}
              onChange={set('password')}
              className={errores.password ? 'input-error' : ''}
              autoComplete="current-password"
              placeholder="••••••••"
            />
            {errores.password && <span className="field-error">⚠ {errores.password}</span>}
          </div>

          <ErrorMessage mensaje={error} />

          <button type="submit" className="btn btn-full" disabled={cargando}>
            {cargando ? (
              <><span className="spinner spinner-sm" /> Entrando...</>
            ) : 'Entrar'}
          </button>
        </form>

        <p className="auth-footer">
          ¿No tienes cuenta? <Link to="/registro">Regístrate aquí</Link>
        </p>
      </div>
    </section>
  )
}

export default LoginPage
