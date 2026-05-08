import { useEffect, useMemo, useState } from 'react'
import { Link, useLocation, useNavigate, useParams } from 'react-router-dom'
import { obtenerHorariosDisponibles, obtenerTicketsAtraccion } from '../../api/atraccionesApi'
import * as reservasApi from '../../api/reservasApi'
import ErrorMessage from '../../components/common/ErrorMessage'
import Spinner from '../../components/common/Spinner'
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
import { useAtracciones } from '../hooks/useAtracciones'
import { useReserva } from '../hooks/useReserva'

const TIPOS_IDENTIFICACION = ['CEDULA', 'PASAPORTE', 'RUC', 'OTRO']

// ─── Confirmación final con factura ──────────────────────────────────────────
function ConfirmacionConFactura({ reserva, factura }) {
  return (
    <section className="page-section">
      <div className="confirmacion-card fade-in">
        <div className="check-icon">✅</div>
        <h1>Pago confirmado</h1>
        <p style={{ color: 'var(--text-muted)', marginBottom: '1rem' }}>
          Pago confirmado y factura generada correctamente.
        </p>

        {factura?.fac_numero && (
          <div className="confirmacion-row">
            <span>Número de factura</span>
            <span style={{ fontFamily: 'monospace' }}>{factura.fac_numero}</span>
          </div>
        )}
        {(reserva?.rev_codigo || factura?.rev_codigo) && (
          <div className="confirmacion-row">
            <span>Código de reserva</span>
            <span style={{ fontFamily: 'monospace' }}>{reserva?.rev_codigo || factura?.rev_codigo}</span>
          </div>
        )}
        {factura?.nombre_receptor && (
          <div className="confirmacion-row">
            <span>Receptor</span>
            <span>{factura.nombre_receptor}</span>
          </div>
        )}
        {factura?.total != null && (
          <div className="confirmacion-row">
            <span>Total pagado</span>
            <span>
              ${Number(factura.total).toFixed(2)}
              {factura.moneda ? ` ${factura.moneda}` : ''}
            </span>
          </div>
        )}
        {factura?.estado && (
          <div className="confirmacion-row">
            <span>Estado</span>
            <span>{factura.estado}</span>
          </div>
        )}

        <div className="inline-form" style={{ marginTop: '1.75rem' }}>
          <Link to="/mis-facturas" className="btn">Ver mis facturas</Link>
          <Link to="/mis-reservas" className="btn btn-outline">Mis reservas</Link>
        </div>
      </div>
    </section>
  )
}

// ─── Pantalla de pago simulado ────────────────────────────────────────────────
function PantallaPago({ reserva, subtotal, iva, total, onConfirmar, errorPago, procesando }) {
  const [form, setForm] = useState({
    nombre_receptor: '',
    apellido_receptor: '',
    correo_receptor: '',
    telefono_receptor: '',
    observacion: '',
  })
  const [errores, setErrores] = useState({})

  const set = (campo) => (e) => {
    setForm((p) => ({ ...p, [campo]: e.target.value }))
    if (errores[campo]) setErrores((p) => ({ ...p, [campo]: '' }))
  }

  const validar = () => {
    const e = {}
    if (!form.nombre_receptor.trim()) e.nombre_receptor = 'El nombre es obligatorio'
    if (!form.correo_receptor.trim()) e.correo_receptor = 'El correo electrónico es obligatorio'
    else if (!esEmailValido(form.correo_receptor)) e.correo_receptor = 'Ingresa un correo electrónico válido'
    return e
  }

  const handleConfirmar = (e) => {
    e.preventDefault()
    const errs = validar()
    if (Object.keys(errs).length) { setErrores(errs); return }
    const payload = {
      nombre_receptor: form.nombre_receptor.trim(),
      correo_receptor: form.correo_receptor.trim(),
    }
    if (form.apellido_receptor.trim()) payload.apellido_receptor = form.apellido_receptor.trim()
    if (form.telefono_receptor.trim()) payload.telefono_receptor = form.telefono_receptor.trim()
    if (form.observacion.trim()) payload.observacion = form.observacion.trim()
    onConfirmar(payload)
  }

  const revSubtotal = Number(reserva?.rev_subtotal ?? subtotal ?? 0)
  const revIva = Number(reserva?.rev_valor_iva ?? iva ?? 0)
  const revTotal = Number(reserva?.rev_total ?? total ?? 0)

  return (
    <section className="page-section">
      <div className="confirmacion-card fade-in">
        <div className="check-icon">💳</div>
        <h1>Pago simulado</h1>
        <p style={{ color: 'var(--text-muted)', marginBottom: '1.25rem' }}>
          Confirma los datos de facturación para finalizar tu reserva.
        </p>

        {/* Resumen de la reserva */}
        <div className="confirmacion-row">
          <span>Código de reserva</span>
          <span style={{ fontFamily: 'monospace' }}>{reserva?.rev_codigo || '—'}</span>
        </div>
        {reserva?.atraccion_nombre && (
          <div className="confirmacion-row">
            <span>Atracción</span>
            <span>{reserva.atraccion_nombre}</span>
          </div>
        )}
        {reserva?.hor_fecha && (
          <div className="confirmacion-row">
            <span>Fecha</span>
            <span>{reserva.hor_fecha}</span>
          </div>
        )}
        <div className="confirmacion-row">
          <span>Subtotal</span>
          <span>${revSubtotal.toFixed(2)}</span>
        </div>
        <div className="confirmacion-row">
          <span>IVA 15%</span>
          <span>${revIva.toFixed(2)}</span>
        </div>
        <div className="confirmacion-row" style={{ fontWeight: 700 }}>
          <span>Total</span>
          <span>${revTotal.toFixed(2)}</span>
        </div>

        {/* Formulario de facturación */}
        <form onSubmit={handleConfirmar} noValidate style={{ marginTop: '1.75rem', textAlign: 'left', width: '100%' }}>
          <div className="form-grid">
            <div className="form-group">
              <label htmlFor="pago-nombre">Nombre *</label>
              <input
                id="pago-nombre"
                type="text"
                value={form.nombre_receptor}
                onChange={set('nombre_receptor')}
                placeholder="Tu nombre"
                className={errores.nombre_receptor ? 'input-error' : ''}
              />
              {errores.nombre_receptor && <span className="field-error">⚠ {errores.nombre_receptor}</span>}
            </div>

            <div className="form-group">
              <label htmlFor="pago-apellido">Apellido</label>
              <input
                id="pago-apellido"
                type="text"
                value={form.apellido_receptor}
                onChange={set('apellido_receptor')}
                placeholder="Tu apellido"
              />
            </div>

            <div className="form-group" style={{ gridColumn: '1 / -1' }}>
              <label htmlFor="pago-correo">Correo electrónico *</label>
              <input
                id="pago-correo"
                type="email"
                value={form.correo_receptor}
                onChange={set('correo_receptor')}
                placeholder="correo@ejemplo.com"
                className={errores.correo_receptor ? 'input-error' : ''}
              />
              {errores.correo_receptor && <span className="field-error">⚠ {errores.correo_receptor}</span>}
            </div>

            <div className="form-group">
              <label htmlFor="pago-tel">Teléfono</label>
              <input
                id="pago-tel"
                type="tel"
                value={form.telefono_receptor}
                onChange={set('telefono_receptor')}
                placeholder="ej. 0991234567"
              />
            </div>

            <div className="form-group" style={{ gridColumn: '1 / -1' }}>
              <label htmlFor="pago-obs">Observación</label>
              <textarea
                id="pago-obs"
                value={form.observacion}
                onChange={set('observacion')}
                rows={2}
                placeholder="Opcional..."
              />
            </div>
          </div>

          <ErrorMessage mensaje={errorPago} />

          <div style={{ marginTop: '1rem' }}>
            <button type="submit" className="btn btn-full" disabled={procesando}>
              {procesando
                ? <><span className="spinner spinner-sm" /> Procesando pago...</>
                : 'Confirmar pago'}
            </button>
          </div>
        </form>
      </div>
    </section>
  )
}

// ─── Pantalla de elección auth / invitado ─────────────────────────────────────
function PantallaEleccion({ onIniciarSesion, onContinuarComoInvitado }) {
  return (
    <div className="auth-card fade-in" style={{ textAlign: 'center' }}>
      <h2>¿Cómo deseas continuar?</h2>
      <p className="text-muted" style={{ margin: '0.75rem 0 1.5rem' }}>
        Puedes iniciar sesión con tu cuenta o reservar como invitado sin registrarte.
      </p>
      <div className="inline-form" style={{ justifyContent: 'center', flexDirection: 'column', gap: '0.75rem' }}>
        <button className="btn btn-full" type="button" onClick={onIniciarSesion}>
          Iniciar sesión
        </button>
        <button className="btn btn-outline btn-full" type="button" onClick={onContinuarComoInvitado}>
          Continuar sin cuenta / Reservar como invitado
        </button>
      </div>
    </div>
  )
}

// ─── Formulario de datos de invitado ─────────────────────────────────────────
function FormularioInvitado({ onConfirmar, onCancelar }) {
  const [form, setForm] = useState({
    tipo_identificacion: '',
    numero_identificacion: '',
    nombres: '',
    apellidos: '',
    correo: '',
    telefono: '',
  })
  const [errores, setErrores] = useState({})

  const set = (campo) => (e) => {
    setForm((p) => ({ ...p, [campo]: e.target.value }))
    if (errores[campo]) setErrores((p) => ({ ...p, [campo]: '' }))
  }

  const validar = () => {
    const e = {}
    if (!form.tipo_identificacion) e.tipo_identificacion = 'Selecciona el tipo de identificación'
    if (!esIdentificacionValida(form.tipo_identificacion, form.numero_identificacion)) {
      e.numero_identificacion = mensajeIdentificacion(form.tipo_identificacion)
    }
    if (!form.correo.trim()) e.correo = 'El correo electrónico es obligatorio'
    else if (!esEmailValido(form.correo)) e.correo = 'Debes ingresar un correo electrónico válido'
    if (form.nombres.trim() && !esNombreValido(form.nombres)) {
      e.nombres = mensajeNombre('Los nombres')
    }
    if (form.apellidos.trim() && !esNombreValido(form.apellidos)) {
      e.apellidos = mensajeNombre('Los apellidos')
    }
    if (form.telefono && !esTelefonoValido(form.telefono)) {
      e.telefono = mensajeTelefono()
    }
    return e
  }

  const handleConfirmar = () => {
    const e = validar()
    if (Object.keys(e).length) { setErrores(e); return }
    const datos = {
      tipo_identificacion: form.tipo_identificacion,
      numero_identificacion: form.numero_identificacion.trim(),
      correo: form.correo.trim(),
    }
    if (form.nombres.trim()) datos.nombres = form.nombres.trim()
    if (form.apellidos.trim()) datos.apellidos = form.apellidos.trim()
    if (form.telefono.trim()) datos.telefono = form.telefono.trim()
    onConfirmar(datos)
  }

  return (
    <div className="auth-card fade-in">
      <h2>Datos del cliente</h2>
      <p className="text-muted" style={{ marginBottom: '1.25rem' }}>
        Completa tus datos para continuar con la reserva.
      </p>
      <div className="form-grid">
        <div className="form-group">
          <label htmlFor="inv-tipo">Tipo de identificación *</label>
          <select
            id="inv-tipo"
            value={form.tipo_identificacion}
            onChange={set('tipo_identificacion')}
            className={errores.tipo_identificacion ? 'input-error' : ''}
          >
            <option value="">Selecciona...</option>
            {TIPOS_IDENTIFICACION.map((t) => <option key={t} value={t}>{t}</option>)}
          </select>
          {errores.tipo_identificacion && <span className="field-error">⚠ {errores.tipo_identificacion}</span>}
        </div>

        <div className="form-group">
          <label htmlFor="inv-num">Número de identificación *</label>
          <input
            id="inv-num"
            type="text"
            value={form.numero_identificacion}
            onChange={set('numero_identificacion')}
            placeholder="ej. 1234567890"
            className={errores.numero_identificacion ? 'input-error' : ''}
          />
          {errores.numero_identificacion && <span className="field-error">⚠ {errores.numero_identificacion}</span>}
        </div>

        <div className="form-group">
          <label htmlFor="inv-nombres">Nombres</label>
          <input
            id="inv-nombres"
            type="text"
            value={form.nombres}
            onChange={set('nombres')}
            placeholder="Tu nombre"
          />
        </div>

        <div className="form-group">
          <label htmlFor="inv-apellidos">Apellidos</label>
          <input
            id="inv-apellidos"
            type="text"
            value={form.apellidos}
            onChange={set('apellidos')}
            placeholder="Tus apellidos"
          />
        </div>

        <div className="form-group" style={{ gridColumn: '1 / -1' }}>
          <label htmlFor="inv-correo">Correo electrónico *</label>
          <input
            id="inv-correo"
            type="email"
            value={form.correo}
            onChange={set('correo')}
            placeholder="correo@ejemplo.com"
            className={errores.correo ? 'input-error' : ''}
          />
          {errores.correo && <span className="field-error">⚠ {errores.correo}</span>}
        </div>

        <div className="form-group">
          <label htmlFor="inv-tel">Teléfono</label>
          <input
            id="inv-tel"
            type="tel"
            value={form.telefono}
            onChange={set('telefono')}
            placeholder="ej. 0991234567"
            className={errores.telefono ? 'input-error' : ''}
          />
          {errores.telefono && <span className="field-error">⚠ {errores.telefono}</span>}
        </div>
      </div>

      <div className="inline-form" style={{ marginTop: '1.25rem' }}>
        <button className="btn" type="button" onClick={handleConfirmar}>
          Continuar con la reserva
        </button>
        <button className="btn btn-outline" type="button" onClick={onCancelar}>
          Atrás
        </button>
      </div>
    </div>
  )
}

// ─── Página principal ─────────────────────────────────────────────────────────
function ReservaPage() {
  const { guid } = useParams()
  const navigate = useNavigate()
  const location = useLocation()
  const { estaAutenticado } = useAuthContext()

  // cargando | eleccion | invitado | formulario | pago | confirmacion
  const [paso, setPaso] = useState('cargando')
  const [clienteInvitado, setClienteInvitado] = useState(null)
  const [horGuid, setHorGuid] = useState('')
  const [cantidades, setCantidades] = useState({})
  const [intentoEnvio, setIntentoEnvio] = useState(false)
  const [tickets, setTickets] = useState([])
  const [horarios, setHorarios] = useState([])

  // Estado del pago
  const [reservaLocal, setReservaLocal] = useState(null)
  const [factura, setFactura] = useState(null)
  const [errorPago, setErrorPago] = useState('')
  const [procesandoPago, setProcesandoPago] = useState(false)

  const { detalle, cargarDetalle, cargando, error } = useAtracciones({})
  const { crearReserva, error: errorReserva, cargando: creando } = useReserva()

  useEffect(() => {
    cargarDetalle(guid)
      .then(() => setPaso(estaAutenticado ? 'formulario' : 'eleccion'))
      .catch(() => setPaso(estaAutenticado ? 'formulario' : 'eleccion'))
  }, [guid]) // eslint-disable-line react-hooks/exhaustive-deps

  useEffect(() => {
    if (!guid) return
    obtenerTicketsAtraccion(guid)
      .then((data) => setTickets(Array.isArray(data) ? data : []))
      .catch(() => setTickets([]))
    obtenerHorariosDisponibles(guid)
      .then((data) => setHorarios(Array.isArray(data) ? data : []))
      .catch(() => setHorarios([]))
  }, [guid])

  const lineas = useMemo(
    () =>
      Object.entries(cantidades)
        .filter(([, cantidad]) => Number(cantidad) > 0)
        .map(([tck_guid, cantidad]) => ({ tck_guid, cantidad: Number(cantidad) })),
    [cantidades],
  )

  const subtotal = useMemo(() => {
    return tickets.reduce((acc, ticket) => {
      const cantidad = Number(cantidades[ticket.tck_guid] || 0)
      return acc + cantidad * Number(ticket.precio || 0)
    }, 0)
  }, [cantidades, tickets])

  const iva = subtotal * 0.15
  const total = subtotal + iva
  const sinTickets = lineas.length === 0
  const sinHorario = !horGuid

  const handleIniciarSesion = () => {
    navigate('/login', { state: { from: location } })
  }

  const handleContinuarComoInvitado = () => {
    setPaso('invitado')
  }

  const handleDatosInvitado = (datos) => {
    setClienteInvitado(datos)
    setPaso('formulario')
  }

  const handleSubmit = async (event) => {
    event.preventDefault()
    setIntentoEnvio(true)
    if (sinHorario || sinTickets) return
    try {
      const reserva = await crearReserva(guid, horGuid, lineas, 'web', clienteInvitado)
      setReservaLocal(reserva)
      setPaso('pago')
    } catch {
      // errorReserva ya se gestiona en el hook y se muestra en el formulario
    }
  }

  const handleConfirmarPago = async (datosFacturacion) => {
    if (!reservaLocal?.rev_guid) {
      setErrorPago('No se encontró el identificador de la reserva. Intenta nuevamente.')
      return
    }
    setProcesandoPago(true)
    setErrorPago('')
    try {
      const resp = await reservasApi.confirmarPago(reservaLocal.rev_guid, datosFacturacion)
      const facturaData = resp?.data ?? resp
      setFactura(facturaData)
      setPaso('confirmacion')
    } catch (err) {
      const status = err?.response?.status
      if (status === 409) {
        setErrorPago('Esta reserva ya tiene un pago confirmado.')
      } else {
        setErrorPago(
          err?.response?.data?.message ||
          err?.response?.data?.details?.[0] ||
          'No se pudo confirmar el pago. La reserva sigue activa — intenta de nuevo o ve a Mis Reservas.'
        )
      }
    } finally {
      setProcesandoPago(false)
    }
  }

  if (cargando && paso === 'cargando') return <Spinner message="Cargando atracción..." />

  if (paso === 'confirmacion') {
    return <ConfirmacionConFactura reserva={reservaLocal} factura={factura} />
  }

  if (paso === 'pago') {
    return (
      <PantallaPago
        reserva={reservaLocal}
        subtotal={subtotal}
        iva={iva}
        total={total}
        onConfirmar={handleConfirmarPago}
        errorPago={errorPago}
        procesando={procesandoPago}
      />
    )
  }

  const sinHorarios = !cargando && detalle && horarios.length === 0

  return (
    <section className="page-section">
      <div style={{ marginBottom: '1.5rem' }}>
        <Link to={`/atracciones/${guid}`} className="text-muted text-sm">← Volver al detalle</Link>
        <h1 style={{ marginTop: '0.5rem' }}>Reservar: {detalle?.nombre}</h1>
      </div>

      <ErrorMessage mensaje={error} />

      {paso === 'eleccion' && (
        <PantallaEleccion
          onIniciarSesion={handleIniciarSesion}
          onContinuarComoInvitado={handleContinuarComoInvitado}
        />
      )}

      {paso === 'invitado' && (
        <FormularioInvitado
          onConfirmar={handleDatosInvitado}
          onCancelar={() => setPaso('eleccion')}
        />
      )}

      {paso === 'formulario' && (
        <>
          {clienteInvitado && (
            <div className="info-message" style={{ marginBottom: '1rem' }}>
              Reservando como invitado: <strong>{clienteInvitado.correo}</strong>.{' '}
              <button
                type="button"
                className="text-sm"
                style={{ background: 'none', border: 'none', color: 'var(--accent)', cursor: 'pointer', textDecoration: 'underline' }}
                onClick={() => setPaso('invitado')}
              >
                Cambiar datos
              </button>
            </div>
          )}

          {sinHorarios && (
            <div className="info-message">
              No hay horarios disponibles en los próximos 7 días. Vuelve pronto.
            </div>
          )}

          {!sinHorarios && (
            <form className="reserva-form" onSubmit={handleSubmit} noValidate>

              <div className="form-group">
                <label htmlFor="horario">Selecciona un horario *</label>
                <select
                  id="horario"
                  value={horGuid}
                  onChange={(e) => { setHorGuid(e.target.value); setIntentoEnvio(false) }}
                  className={intentoEnvio && sinHorario ? 'input-error' : ''}
                >
                  <option value="">— Elige una fecha y hora —</option>
                  {horarios.map((horario, index) => (
                    <option key={horario.hor_guid || index} value={horario.hor_guid}>
                      {horario.fecha} {horario.hora_inicio}
                      {horario.cupos_disponibles != null ? ` — ${horario.cupos_disponibles} cupos disponibles` : ''}
                    </option>
                  ))}
                </select>
                {intentoEnvio && sinHorario && (
                  <span className="field-error">⚠ Selecciona un horario para continuar</span>
                )}
              </div>

              <div className="form-group">
                <label>Cantidad de tickets *</label>
                <div className="tickets-box">
                  {tickets.map((ticket) => (
                    <div className="ticket-row" key={ticket.tck_guid}>
                      <div className="ticket-row-info">
                        <strong>{ticket.titulo}</strong>
                        <span>${Number(ticket.precio).toFixed(2)} por persona</span>
                      </div>
                      <div className="ticket-qty">
                        <button
                          type="button"
                          className="btn btn-outline btn-sm"
                          onClick={() => setCantidades((prev) => ({
                            ...prev,
                            [ticket.tck_guid]: Math.max(0, (Number(prev[ticket.tck_guid] || 0) - 1))
                          }))}
                        >−</button>
                        <input
                          type="number"
                          min="0"
                          value={cantidades[ticket.tck_guid] || 0}
                          onChange={(e) =>
                            setCantidades((prev) => ({ ...prev, [ticket.tck_guid]: e.target.value }))
                          }
                        />
                        <button
                          type="button"
                          className="btn btn-outline btn-sm"
                          onClick={() => setCantidades((prev) => ({
                            ...prev,
                            [ticket.tck_guid]: (Number(prev[ticket.tck_guid] || 0) + 1)
                          }))}
                        >+</button>
                      </div>
                    </div>
                  ))}
                </div>
                {intentoEnvio && sinTickets && (
                  <span className="field-error">⚠ Selecciona al menos un ticket</span>
                )}
              </div>

              <div className="totales-box">
                <p><span>Subtotal</span><span>${subtotal.toFixed(2)}</span></p>
                <p><span>IVA 15%</span><span>${iva.toFixed(2)}</span></p>
                <p className="total"><span>Total</span><span>${total.toFixed(2)}</span></p>
              </div>

              <ErrorMessage mensaje={errorReserva} />

              <button
                type="submit"
                className="btn btn-full"
                disabled={creando}
              >
                {creando ? (
                  <><span className="spinner spinner-sm" /> Procesando reserva...</>
                ) : 'Confirmar reserva'}
              </button>
            </form>
          )}
        </>
      )}
    </section>
  )
}

export default ReservaPage
