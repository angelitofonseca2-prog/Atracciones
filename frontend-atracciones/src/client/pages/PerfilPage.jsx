import { useEffect, useState } from 'react'
import { apiClient } from '../../api/atraccionesApi'
import ErrorMessage from '../../components/common/ErrorMessage'
import Spinner from '../../components/common/Spinner'
import { emitirToast } from '../../components/common/Toast'
import {
  esEmailValido,
  esTelefonoValido,
} from '../../utils/validaciones'

/**
 * Perfil del cliente autenticado.
 *
 * Endpoints públicos (cliente):
 *   - GET  /clientes/perfil
 *   - PUT  /clientes/perfil  body: { nombres, apellidos, correo, telefono? }
 *
 * Los campos de identificación son inmutables desde el cliente.
 */
function PerfilPage() {
  const [perfil, setPerfil] = useState(null)
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState('')
  const [guardando, setGuardando] = useState(false)
  const [errores, setErrores] = useState({})
  const [form, setForm] = useState({
    nombres: '',
    apellidos: '',
    correo: '',
    telefono: '',
  })

  useEffect(() => {
    setCargando(true)
    apiClient
      .get('/clientes/perfil')
      .then((response) => {
        const data = response.data?.data ?? response.data ?? {}
        setPerfil(data)
        setForm({
          nombres: data.nombres ?? '',
          apellidos: data.apellidos ?? '',
          correo: data.correo ?? '',
          telefono: data.telefono ?? '',
        })
      })
      .catch((err) => {
        setError(err?.response?.data?.message || 'No se pudo cargar el perfil.')
      })
      .finally(() => setCargando(false))
  }, [])

  const set = (campo) => (e) => {
    setForm((prev) => ({ ...prev, [campo]: e.target.value }))
    if (errores[campo]) setErrores((p) => ({ ...p, [campo]: '' }))
  }

  const validar = () => {
    const e = {}
    if (!form.nombres.trim()) e.nombres = 'Los nombres son obligatorios'
    if (!form.apellidos.trim()) e.apellidos = 'Los apellidos son obligatorios'
    if (!form.correo.trim()) e.correo = 'El correo es obligatorio'
    else if (!esEmailValido(form.correo)) e.correo = 'Correo inválido'
    if (form.telefono && !esTelefonoValido(form.telefono)) e.telefono = 'Teléfono inválido'
    return e
  }

  const handleSubmit = async (event) => {
    event.preventDefault()
    const errs = validar()
    if (Object.keys(errs).length) { setErrores(errs); return }
    setGuardando(true)
    setError('')
    try {
      const payload = {
        nombres: form.nombres.trim(),
        apellidos: form.apellidos.trim(),
        correo: form.correo.trim(),
      }
      if (form.telefono.trim()) payload.telefono = form.telefono.trim()
      await apiClient.put('/clientes/perfil', payload)
      emitirToast('Perfil actualizado correctamente.', 'success')
      setPerfil((prev) => ({ ...(prev || {}), ...payload }))
    } catch (err) {
      setError(err?.response?.data?.message || 'No se pudo actualizar el perfil.')
    } finally {
      setGuardando(false)
    }
  }

  if (cargando) return <Spinner message="Cargando perfil..." />

  return (
    <section className="page-section">
      <h1>Mi Perfil</h1>
      <ErrorMessage mensaje={error} />

      {perfil && (
        <div className="reserva-card" style={{ marginBottom: '1.5rem' }}>
          <p>
            <strong>Tipo ID:</strong> {perfil.tipo_identificacion ?? '—'}
          </p>
          <p>
            <strong>Número ID:</strong> {perfil.numero_identificacion ?? '—'}
          </p>
        </div>
      )}

      <form className="reserva-form" onSubmit={handleSubmit} noValidate>
        <h2>Actualizar mis datos</h2>

        <div className="form-grid form-grid-2">
          <div className="form-group">
            <label htmlFor="perfil-nombres">Nombres *</label>
            <input
              id="perfil-nombres"
              type="text"
              value={form.nombres}
              onChange={set('nombres')}
              className={errores.nombres ? 'input-error' : ''}
            />
            {errores.nombres && <span className="field-error">⚠ {errores.nombres}</span>}
          </div>
          <div className="form-group">
            <label htmlFor="perfil-apellidos">Apellidos *</label>
            <input
              id="perfil-apellidos"
              type="text"
              value={form.apellidos}
              onChange={set('apellidos')}
              className={errores.apellidos ? 'input-error' : ''}
            />
            {errores.apellidos && <span className="field-error">⚠ {errores.apellidos}</span>}
          </div>
        </div>

        <div className="form-group">
          <label htmlFor="perfil-correo">Correo electrónico *</label>
          <input
            id="perfil-correo"
            type="email"
            value={form.correo}
            onChange={set('correo')}
            className={errores.correo ? 'input-error' : ''}
          />
          {errores.correo && <span className="field-error">⚠ {errores.correo}</span>}
        </div>

        <div className="form-group">
          <label htmlFor="perfil-telefono">Teléfono <span className="text-muted">(opcional)</span></label>
          <input
            id="perfil-telefono"
            type="tel"
            value={form.telefono}
            onChange={set('telefono')}
            className={errores.telefono ? 'input-error' : ''}
          />
          {errores.telefono && <span className="field-error">⚠ {errores.telefono}</span>}
        </div>

        <button type="submit" className="btn" disabled={guardando}>
          {guardando ? <><span className="spinner spinner-sm" /> Guardando...</> : 'Guardar cambios'}
        </button>
      </form>
    </section>
  )
}

export default PerfilPage
