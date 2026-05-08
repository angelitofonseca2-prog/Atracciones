import { useEffect, useState } from 'react'
import { adminApi } from '../../api/adminApi'
import Spinner from '../../components/common/Spinner'

const TIPOS_PARTICIPANTE = ['Adulto', 'Niño', 'Grupo', 'Estudiante', 'Senior']

/**
 * Crea/edita tickets contra:
 *   - POST /admin/tickets   body: CrearTicketRequest
 *   - PUT  /admin/tickets/{guid} body: ActualizarTicketRequest
 *
 * Nombres de campos del contrato (snake_case):
 *   at_guid, titulo, tipo_participante, precio, capacidad_maxima, cupos_disponibles,
 *   estado (solo en update; 'A'|'I').
 */
function FormularioTicket({ inicial, onGuardar, onCancelar }) {
  const [form, setForm] = useState({
    at_guid: '',
    titulo: '',
    precio: '',
    tipo_participante: 'Adulto',
    capacidad_maxima: '',
    cupos_disponibles: '',
  })
  const [errores, setErrores] = useState({})
  const [atracciones, setAtracciones] = useState([])
  const [cargando, setCargando] = useState(false)
  const [guardando, setGuardando] = useState(false)
  const [errorCarga, setErrorCarga] = useState('')

  useEffect(() => {
    setCargando(true)
    setErrorCarga('')
    adminApi
      .listarTodasAtraccionesAdmin()
      .then((data) => setAtracciones(Array.isArray(data) ? data : []))
      .catch((err) => {
        const status = err?.response?.status
        if (status === 401 || status === 403) {
          setErrorCarga('Sin permisos para cargar atracciones. Verifica que hayas iniciado sesión como administrador.')
        } else {
          setErrorCarga(err?.response?.data?.message || 'Error al cargar atracciones. Intenta recargar la página.')
        }
        setAtracciones([])
      })
      .finally(() => setCargando(false))
  }, [])

  useEffect(() => {
    if (!inicial) return
    setForm({
      at_guid: inicial.at_guid ?? '',
      titulo: inicial.titulo ?? '',
      precio: inicial.precio ?? '',
      tipo_participante: inicial.tipo_participante ?? 'Adulto',
      capacidad_maxima: inicial.capacidad_maxima ?? '',
      cupos_disponibles: inicial.cupos_disponibles ?? '',
    })
  }, [inicial])

  const set = (campo) => (e) => {
    setForm((p) => ({ ...p, [campo]: e.target.value }))
    if (errores[campo]) setErrores((p) => ({ ...p, [campo]: '' }))
  }

  const validar = () => {
    const e = {}
    if (!form.at_guid) e.at_guid = 'Selecciona una atracción'
    if (!form.titulo.trim()) e.titulo = 'El título es obligatorio'
    if (form.precio === '' || Number(form.precio) < 0) e.precio = 'Ingresa un precio válido (≥ 0)'
    if (!form.capacidad_maxima || Number(form.capacidad_maxima) < 1) e.capacidad_maxima = 'Capacidad mínima: 1'
    if (form.cupos_disponibles === '' || Number(form.cupos_disponibles) < 0) e.cupos_disponibles = 'Cupos debe ser ≥ 0'
    return e
  }

  const submit = async (event) => {
    event.preventDefault()
    const e = validar()
    if (Object.keys(e).length) { setErrores(e); return }
    setGuardando(true)
    try {
      const payload = {
        titulo: form.titulo.trim(),
        precio: Number(form.precio),
        tipo_participante: form.tipo_participante,
        capacidad_maxima: Number(form.capacidad_maxima),
        cupos_disponibles: Number(form.cupos_disponibles),
      }
      // En creación se exige at_guid; en edición no se reasigna la atracción.
      if (!inicial) payload.at_guid = form.at_guid
      await onGuardar(payload)
    } finally {
      setGuardando(false)
    }
  }

  return (
    <form className="admin-form" onSubmit={submit} noValidate>

      {/* Atracción */}
      <div className="form-group">
        <label htmlFor="ft-atraccion">Atracción *</label>
        {cargando ? (
          <Spinner message="Cargando atracciones..." />
        ) : (
          <select
            id="ft-atraccion"
            value={form.at_guid}
            onChange={set('at_guid')}
            disabled={Boolean(inicial)}
            className={errores.at_guid ? 'input-error' : ''}
          >
            <option value="">— Selecciona una atracción —</option>
            {atracciones.map((item) => (
              <option key={item.at_guid} value={item.at_guid}>{item.nombre}</option>
            ))}
          </select>
        )}
        {errores.at_guid && <span className="field-error">⚠ {errores.at_guid}</span>}
        {!cargando && errorCarga && (
          <span className="field-error">⚠ {errorCarga}</span>
        )}
        {!cargando && !errorCarga && atracciones.length === 0 && (
          <span className="field-error" style={{ color: 'var(--text-muted)' }}>
            No hay atracciones registradas. Crea una primero.
          </span>
        )}
      </div>

      {/* Título */}
      <div className="form-group">
        <label htmlFor="ft-titulo">Título del ticket *</label>
        <input
          id="ft-titulo"
          type="text"
          value={form.titulo}
          onChange={set('titulo')}
          placeholder="ej. Adulto — entrada general"
          maxLength={200}
          className={errores.titulo ? 'input-error' : ''}
        />
        {errores.titulo && <span className="field-error">⚠ {errores.titulo}</span>}
      </div>

      {/* Tipo de participante */}
      <div className="form-group">
        <label htmlFor="ft-tipo">Tipo de participante</label>
        <select id="ft-tipo" value={form.tipo_participante} onChange={set('tipo_participante')}>
          {TIPOS_PARTICIPANTE.map((t) => <option key={t} value={t}>{t}</option>)}
        </select>
      </div>

      {/* Precio */}
      <div className="form-group">
        <label htmlFor="ft-precio">Precio ($) *</label>
        <input
          id="ft-precio"
          type="number"
          min="0"
          step="0.01"
          value={form.precio}
          onChange={set('precio')}
          placeholder="0.00"
          className={errores.precio ? 'input-error' : ''}
        />
        {errores.precio && <span className="field-error">⚠ {errores.precio}</span>}
      </div>

      {/* Capacidad máxima */}
      <div className="form-group">
        <label htmlFor="ft-cap">Capacidad máxima *</label>
        <input
          id="ft-cap"
          type="number"
          min="1"
          value={form.capacidad_maxima}
          onChange={set('capacidad_maxima')}
          placeholder="ej. 20"
          className={errores.capacidad_maxima ? 'input-error' : ''}
        />
        {errores.capacidad_maxima && <span className="field-error">⚠ {errores.capacidad_maxima}</span>}
      </div>

      {/* Cupos disponibles */}
      <div className="form-group">
        <label htmlFor="ft-cupos">Cupos disponibles *</label>
        <input
          id="ft-cupos"
          type="number"
          min="0"
          value={form.cupos_disponibles}
          onChange={set('cupos_disponibles')}
          placeholder="ej. 15"
          className={errores.cupos_disponibles ? 'input-error' : ''}
        />
        {errores.cupos_disponibles && <span className="field-error">⚠ {errores.cupos_disponibles}</span>}
      </div>

      <div className="inline-form">
        <button className="btn" type="submit" disabled={guardando || cargando}>
          {guardando ? <><span className="spinner spinner-sm" /> Guardando...</> : 'Guardar'}
        </button>
        <button className="btn btn-outline" type="button" onClick={onCancelar} disabled={guardando}>
          Cancelar
        </button>
      </div>
    </form>
  )
}

export default FormularioTicket
