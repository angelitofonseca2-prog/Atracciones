import { useEffect, useState } from 'react'
import { adminApi } from '../../api/adminApi'

/**
 * Crea/edita horarios contra:
 *   - POST /admin/tickets/horarios   body: CrearHorarioRequest
 *       { tck_guid, fecha (yyyy-MM-dd), hora_inicio (HH:mm:ss),
 *         hora_fin? (HH:mm:ss), cupos_disponibles }
 *   - PUT  /admin/horarios/{guid}    body: ActualizarHorarioRequest
 *       Todos los campos opcionales; estado puede ser 'A' o 'I'.
 *
 * Selecciona tck_guid en cascada: atracción → tickets de la atracción.
 */

const a_HH_MM_SS = (valor) => {
  if (!valor) return ''
  // input type=time entrega "HH:mm" → backend exige "HH:mm:ss".
  if (valor.length === 5) return `${valor}:00`
  return valor
}

const a_HH_MM = (valor) => {
  if (!valor) return ''
  return String(valor).slice(0, 5)
}

function FormularioHorario({ inicial, onCrear, onActualizar, onCancelar }) {
  const esEdicion = Boolean(inicial)

  const [atracciones, setAtracciones] = useState([])
  const [tickets, setTickets] = useState([])
  const [cargandoAt, setCargandoAt] = useState(false)
  const [cargandoTck, setCargandoTck] = useState(false)
  const [errorCargaAt, setErrorCargaAt] = useState('')

  const [atGuid, setAtGuid] = useState('')
  const [form, setForm] = useState({
    tck_guid: '',
    fecha: '',
    hora_inicio: '',
    hora_fin: '',
    cupos_disponibles: '',
    estado: 'A',
  })
  const [errores, setErrores] = useState({})
  const [guardando, setGuardando] = useState(false)

  // Carga atracciones (solo necesarias en creación; en edición no se reasigna).
  useEffect(() => {
    if (esEdicion) return
    setCargandoAt(true)
    setErrorCargaAt('')
    adminApi.listarTodasAtraccionesAdmin()
      .then((data) => setAtracciones(Array.isArray(data) ? data : []))
      .catch((err) => {
        const status = err?.response?.status
        if (status === 401 || status === 403) {
          setErrorCargaAt('Sin permisos para cargar atracciones. Verifica que hayas iniciado sesión como administrador.')
        } else {
          setErrorCargaAt(err?.response?.data?.message || 'Error al cargar atracciones. Intenta recargar la página.')
        }
        setAtracciones([])
      })
      .finally(() => setCargandoAt(false))
  }, [esEdicion])

  // Cuando cambia la atracción → carga sus tickets.
  useEffect(() => {
    if (esEdicion || !atGuid) {
      setTickets([])
      if (!esEdicion) setForm((p) => ({ ...p, tck_guid: '' }))
      return
    }
    setCargandoTck(true)
    adminApi.listarTicketsDeAtraccionAdmin(atGuid)
      .then((data) => setTickets(Array.isArray(data) ? data : []))
      .catch(() => setTickets([]))
      .finally(() => setCargandoTck(false))
    setForm((p) => ({ ...p, tck_guid: '' }))
  }, [atGuid, esEdicion])

  // Precarga al editar.
  useEffect(() => {
    if (!inicial) return
    setForm({
      tck_guid: inicial.tck_guid ?? '',
      fecha: (inicial.fecha ?? '').slice(0, 10),
      hora_inicio: a_HH_MM(inicial.hora_inicio),
      hora_fin: a_HH_MM(inicial.hora_fin),
      cupos_disponibles: inicial.cupos_disponibles ?? '',
      estado: inicial.estado ?? 'A',
    })
  }, [inicial])

  const set = (campo) => (e) => {
    setForm((p) => ({ ...p, [campo]: e.target.value }))
    if (errores[campo]) setErrores((p) => ({ ...p, [campo]: '' }))
  }

  const validar = () => {
    const e = {}
    if (!esEdicion && !atGuid) e.atGuid = 'Selecciona una atracción'
    if (!esEdicion && !form.tck_guid) e.tck_guid = 'Selecciona un ticket'
    if (!form.fecha) e.fecha = 'La fecha es obligatoria'
    if (!form.hora_inicio) e.hora_inicio = 'La hora de inicio es obligatoria'
    if (form.cupos_disponibles === '' || Number(form.cupos_disponibles) < 1) {
      e.cupos_disponibles = 'Cupos mínimo: 1'
    }
    return e
  }

  const submit = async (event) => {
    event.preventDefault()
    const e = validar()
    if (Object.keys(e).length) { setErrores(e); return }
    setGuardando(true)
    try {
      if (esEdicion) {
        const payload = {
          fecha: form.fecha,
          hora_inicio: a_HH_MM_SS(form.hora_inicio),
          cupos_disponibles: Number(form.cupos_disponibles),
          estado: form.estado,
        }
        if (form.hora_fin) payload.hora_fin = a_HH_MM_SS(form.hora_fin)
        await onActualizar(inicial.hor_guid, payload)
      } else {
        const payload = {
          tck_guid: form.tck_guid,
          fecha: form.fecha,
          hora_inicio: a_HH_MM_SS(form.hora_inicio),
          cupos_disponibles: Number(form.cupos_disponibles),
        }
        if (form.hora_fin) payload.hora_fin = a_HH_MM_SS(form.hora_fin)
        await onCrear(payload)
      }
    } finally {
      setGuardando(false)
    }
  }

  const tituloTicketEdicion =
    inicial?.atraccion_nombre && inicial?.ticket_titulo
      ? `${inicial.atraccion_nombre} — ${inicial.ticket_titulo}`
      : inicial?.tck_guid

  return (
    <form className="admin-form" onSubmit={submit} noValidate>

      {/* Selector de atracción solo en creación */}
      {!esEdicion ? (
        <>
          <div className="form-group">
            <label htmlFor="fh-at">1. Atracción *</label>
            <select
              id="fh-at"
              value={atGuid}
              onChange={(e) => { setAtGuid(e.target.value); if (errores.atGuid) setErrores((p) => ({ ...p, atGuid: '' })) }}
              disabled={cargandoAt}
              className={errores.atGuid ? 'input-error' : ''}
            >
              <option value="">
                {cargandoAt ? 'Cargando atracciones...' : '— Selecciona una atracción —'}
              </option>
              {atracciones.map((item) => (
                <option key={item.at_guid} value={item.at_guid}>{item.nombre}</option>
              ))}
            </select>
            {errores.atGuid && <span className="field-error">⚠ {errores.atGuid}</span>}
            {!cargandoAt && errorCargaAt && (
              <span className="field-error">⚠ {errorCargaAt}</span>
            )}
            {!cargandoAt && !errorCargaAt && atracciones.length === 0 && (
              <span style={{ fontSize: '0.82rem', color: 'var(--text-muted)' }}>
                No hay atracciones registradas. Crea una primero.
              </span>
            )}
          </div>

          <div className="form-group">
            <label htmlFor="fh-tck">2. Ticket *</label>
            <select
              id="fh-tck"
              value={form.tck_guid}
              onChange={set('tck_guid')}
              disabled={!atGuid || cargandoTck}
              className={errores.tck_guid ? 'input-error' : ''}
            >
              <option value="">
                {!atGuid
                  ? 'Primero selecciona una atracción'
                  : cargandoTck
                    ? 'Cargando tickets...'
                    : tickets.length === 0
                      ? 'Sin tickets — crea uno primero'
                      : '— Selecciona un ticket —'}
              </option>
              {tickets.map((t) => (
                <option key={t.tck_guid} value={t.tck_guid}>
                  {t.titulo}{t.tipo_participante ? ` (${t.tipo_participante})` : ''}
                  {t.precio != null ? ` — $${Number(t.precio).toFixed(2)}` : ''}
                </option>
              ))}
            </select>
            {errores.tck_guid && <span className="field-error">⚠ {errores.tck_guid}</span>}
          </div>
        </>
      ) : (
        <div className="form-group" style={{ gridColumn: '1 / -1' }}>
          <label>Ticket</label>
          <input type="text" value={tituloTicketEdicion || '—'} disabled />
        </div>
      )}

      {/* Fecha */}
      <div className="form-group">
        <label htmlFor="fh-fecha">Fecha *</label>
        <input
          id="fh-fecha"
          type="date"
          value={form.fecha}
          onChange={set('fecha')}
          min={new Date().toISOString().slice(0, 10)}
          className={errores.fecha ? 'input-error' : ''}
        />
        {errores.fecha && <span className="field-error">⚠ {errores.fecha}</span>}
      </div>

      {/* Hora inicio */}
      <div className="form-group">
        <label htmlFor="fh-hi">Hora de inicio *</label>
        <input
          id="fh-hi"
          type="time"
          value={form.hora_inicio}
          onChange={set('hora_inicio')}
          className={errores.hora_inicio ? 'input-error' : ''}
        />
        {errores.hora_inicio && <span className="field-error">⚠ {errores.hora_inicio}</span>}
      </div>

      {/* Hora fin (opcional) */}
      <div className="form-group">
        <label htmlFor="fh-hf">Hora de fin <span className="text-muted">(opcional)</span></label>
        <input
          id="fh-hf"
          type="time"
          value={form.hora_fin}
          onChange={set('hora_fin')}
        />
      </div>

      {/* Cupos */}
      <div className="form-group">
        <label htmlFor="fh-cupos">Cupos disponibles *</label>
        <input
          id="fh-cupos"
          type="number"
          min="1"
          value={form.cupos_disponibles}
          onChange={set('cupos_disponibles')}
          placeholder="ej. 20"
          className={errores.cupos_disponibles ? 'input-error' : ''}
        />
        {errores.cupos_disponibles && <span className="field-error">⚠ {errores.cupos_disponibles}</span>}
      </div>

      {esEdicion && (
        <div className="form-group">
          <label htmlFor="fh-estado">Estado</label>
          <select id="fh-estado" value={form.estado} onChange={set('estado')}>
            <option value="A">Activo</option>
            <option value="I">Inactivo</option>
          </select>
        </div>
      )}

      <div className="inline-form">
        <button className="btn" type="submit" disabled={guardando}>
          {guardando ? <><span className="spinner spinner-sm" /> Guardando...</> : (esEdicion ? 'Actualizar horario' : 'Guardar horario')}
        </button>
        <button className="btn btn-outline" type="button" onClick={onCancelar} disabled={guardando}>
          Cancelar
        </button>
      </div>
    </form>
  )
}

export default FormularioHorario
