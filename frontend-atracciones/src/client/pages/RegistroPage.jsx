import { useState } from 'react'
import { Link, Navigate, useLocation, useNavigate } from 'react-router-dom'
import * as authApi from '../../api/authApi'
import ErrorMessage from '../../components/common/ErrorMessage'
import { emitirToast } from '../../components/common/Toast'
import { useAuthContext } from '../../context/AuthContext'
import {
  esEmailValido,
  esIdentificacionValida,
  esNombreValido,
  esTelefonoValido,
  mensajeIdentificacion,
  mensajeTelefono,
  mensajeNombre,
} from '../../utils/validaciones'

/**
 * Tipos aceptados por el backend (RegistroClienteRequest.TipoIdentificacion ≤ 20 chars).
 * Coincide con catalogo de personas (CEDULA, RUC, PASAPORTE, OTRO).
 */
const TIPOS_ID = ['CEDULA', 'RUC', 'PASAPORTE', 'OTRO']

function RegistroPage() {
  const navigate = useNavigate()
  const location = useLocation()
  const { estaAutenticado, login } = useAuthContext()
  const destino = location.state?.from?.pathname || '/atracciones'

  // Login = correo (el backend exige `correo` y permite reutilizar como `login`).
  const [form, setForm] = useState({
    correo: '',
    password: '',
    confirmar_password: '',
    nombres: '',
    apellidos: '',
    tipo_identificacion: 'CEDULA',
    numero_identificacion: '',
    telefono: '',
  })
  const [errores, setErrores] = useState({})
  const [cargando, setCargando] = useState(false)
  const [errorGlobal, setErrorGlobal] = useState('')

  if (estaAutenticado) return <Navigate to="/atracciones" replace />

  const set = (campo) => (e) => {
    setForm((prev) => ({ ...prev, [campo]: e.target.value }))
    if (errores[campo]) setErrores((prev) => ({ ...prev, [campo]: '' }))
  }

  const validar = () => {
    const e = {}
    if (!esEmailValido(form.correo)) e.correo = 'Ingresa un correo electrónico válido'
    if (!esNombreValido(form.nombres)) e.nombres = mensajeNombre('Los nombres')
    if (!esNombreValido(form.apellidos)) e.apellidos = mensajeNombre('Los apellidos')
    if (!esIdentificacionValida(form.tipo_identificacion, form.numero_identificacion)) {
      e.numero_identificacion = mensajeIdentificacion(form.tipo_identificacion)
    }
    if (form.telefono && !esTelefonoValido(form.telefono)) {
      e.telefono = mensajeTelefono()
    }
    if (form.password.length < 6) e.password = 'La contraseña debe tener al menos 6 caracteres'
    if (form.password !== form.confirmar_password) {
      e.confirmar_password = 'Las contraseñas no coinciden'
    }
    return e
  }

  const submit = async (event) => {
    event.preventDefault()
    const e = validar()
    if (Object.keys(e).length) { setErrores(e); return }
    setErrores({})
    setCargando(true)
    setErrorGlobal('')
    try {
      // El backend `RegistroClienteRequest` exige todos los campos:
      // El servicio crea cliente y devuelve token autenticado. NO debemos
      // hacer una segunda llamada a /admin/clientes desde el flujo público.
      const correo = form.correo.trim()
      const data = await authApi.registro({
        login: correo,
        password: form.password,
        tipo_identificacion: form.tipo_identificacion,
        numero_identificacion: form.numero_identificacion.trim(),
        nombres: form.nombres.trim(),
        apellidos: form.apellidos.trim(),
        correo,
        telefono: form.telefono.trim() || undefined,
      })

      const token = data?.data?.token
      const usuarioLogin = data?.data?.login || correo
      const roles = data?.data?.roles || []
      login(token, { login: usuarioLogin, roles })
      emitirToast('Cuenta creada correctamente. ¡Bienvenido!', 'success')
      navigate(destino, { replace: true })
    } catch (err) {
      const mensaje =
        err?.response?.data?.details?.[0] ||
        err?.response?.data?.message ||
        'No se pudo completar el registro. Intenta de nuevo.'
      setErrorGlobal(mensaje)
    } finally {
      setCargando(false)
    }
  }

  const campo = (id, label, tipo = 'text', placeholder = '', opcional = false) => (
    <div className="form-group">
      <label htmlFor={id}>{label}{opcional && <span className="text-muted"> (opcional)</span>}</label>
      <input
        id={id}
        type={tipo}
        value={form[id]}
        onChange={set(id)}
        placeholder={placeholder}
        className={errores[id] ? 'input-error' : ''}
        autoComplete={tipo === 'password' ? 'new-password' : id}
      />
      {errores[id] && <span className="field-error">⚠ {errores[id]}</span>}
    </div>
  )

  return (
    <section className="auth-page" style={{ paddingTop: '2rem', paddingBottom: '2rem' }}>
      <div className="auth-card fade-in" style={{ maxWidth: 560 }}>
        <h1>Crear cuenta</h1>
        <p className="auth-subtitle">Completa tus datos para registrarte.</p>

        <form onSubmit={submit} className="form-grid" noValidate>
          {campo('correo', 'Correo electrónico', 'email', 'correo@ejemplo.com')}

          <div className="form-grid form-grid-2">
            {campo('nombres', 'Nombres', 'text', 'Juan Carlos')}
            {campo('apellidos', 'Apellidos', 'text', 'Pérez López')}
          </div>

          <div className="form-grid form-grid-2">
            <div className="form-group">
              <label htmlFor="tipo_identificacion">Tipo de identificación</label>
              <select
                id="tipo_identificacion"
                value={form.tipo_identificacion}
                onChange={set('tipo_identificacion')}
              >
                {TIPOS_ID.map((t) => <option key={t} value={t}>{t}</option>)}
              </select>
            </div>
            {campo('numero_identificacion', 'Número de identificación', 'text', '1234567890')}
          </div>

          {campo('telefono', 'Teléfono', 'tel', '+593 99 000 0000', true)}

          <hr className="divider" />

          {campo('password', 'Contraseña', 'password', '••••••••')}
          {campo('confirmar_password', 'Confirmar contraseña', 'password', '••••••••')}

          <ErrorMessage mensaje={errorGlobal} />

          <button type="submit" className="btn btn-full" disabled={cargando}>
            {cargando ? (
              <><span className="spinner spinner-sm" /> Registrando...</>
            ) : 'Crear cuenta'}
          </button>
        </form>

        <p className="auth-footer">
          ¿Ya tienes cuenta? <Link to="/login">Inicia sesión</Link>
        </p>
      </div>
    </section>
  )
}

export default RegistroPage
