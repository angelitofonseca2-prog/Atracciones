import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
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
import { mapPerfilAFormulario, mapPerfilAPago, usePerfilCliente } from '../hooks/usePerfilCliente'
import { useReserva } from '../hooks/useReserva'

const TIPOS_IDENTIFICACION = ['CEDULA', 'PASAPORTE', 'RUC', 'OTRO']

function loadPayPalScript(clientId, currency) {
  return new Promise((resolve, reject) => {
    if (typeof window === 'undefined') {
      reject(new Error('Sin window'))
      return
    }
    if (window.paypal) {
      resolve()
      return
    }
    const existing = document.querySelector('script[data-paypal-sdk]')
    if (existing) {
      existing.addEventListener('load', () => resolve(), { once: true })
      existing.addEventListener('error', () => reject(new Error('PayPal SDK')), { once: true })
      return
    }
    const s = document.createElement('script')
    s.setAttribute('data-paypal-sdk', '1')
    s.async = true
    s.src = `https://www.paypal.com/sdk/js?client-id=${encodeURIComponent(clientId)}&currency=${encodeURIComponent(currency)}`
    s.onload = () => resolve()
    s.onerror = () => reject(new Error('No se pudo cargar el SDK de PayPal'))
    document.body.appendChild(s)
  })
}

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

// ─── Pago con PayPal (orden y captura en servidor) ───────────────────────────
function PantallaPago({
  reserva,
  subtotal,
  iva,
  total,
  estaAutenticado,
  onPagoExitoso,
  errorPago,
  setErrorPago,
  datosFacturacionIniciales,
}) {
  const [form, setForm] = useState({
    nombre_receptor: '',
    apellido_receptor: '',
    correo_receptor: '',
    telefono_receptor: '',
    observacion: '',
  })

  useEffect(() => {
    if (datosFacturacionIniciales) {
      setForm((prev) => ({ ...prev, ...datosFacturacionIniciales }))
    }
  }, [datosFacturacionIniciales])
  const [errores, setErrores] = useState({})
  const [procesandoCaptura, setProcesandoCaptura] = useState(false)
  const formRef = useRef(form)
  formRef.current = form

  const set = (campo) => (e) => {
    setForm((p) => ({ ...p, [campo]: e.target.value }))
    if (errores[campo]) setErrores((p) => ({ ...p, [campo]: '' }))
  }

  const validar = () => {
    const f = formRef.current
    const e = {}
    if (!f.nombre_receptor.trim()) e.nombre_receptor = 'El nombre es obligatorio'
    if (!f.correo_receptor.trim()) e.correo_receptor = 'El correo electrónico es obligatorio'
    else if (!esEmailValido(f.correo_receptor)) e.correo_receptor = 'Ingresa un correo electrónico válido'
    return e
  }

  const clientId = import.meta.env.VITE_PAYPAL_CLIENT_ID
  const moneda = (reserva?.moneda || 'USD').toString().toUpperCase()
  const onPagoRef = useRef(onPagoExitoso)
  onPagoRef.current = onPagoExitoso

  useEffect(() => {
    if (!clientId || !reserva?.rev_guid) return undefined
    const el = document.getElementById('paypal-button-container')
    if (!el) return undefined
    let cancelled = false
    ;(async () => {
      try {
        await loadPayPalScript(clientId, moneda)
        if (cancelled || !window.paypal) return
        el.innerHTML = ''
        window.paypal
          .Buttons({
            createOrder: async () => {
              const errs = validar()
              if (Object.keys(errs).length) {
                setErrores(errs)
                throw new Error('Completa los datos de facturación antes de pagar.')
              }
              setErrorPago('')
              const body = {
                rev_guid: reserva.rev_guid,
              }
              if (!estaAutenticado && reserva.rev_codigo) {
                body.rev_codigo = reserva.rev_codigo
              }
              const resp = await reservasApi.crearOrdenPayPal(body)
              const orderId = resp?.data?.paypal_order_id
              if (!orderId) throw new Error('No se recibió orden de PayPal.')
              return orderId
            },
            onApprove: async (data) => {
              const errs = validar()
              if (Object.keys(errs).length) {
                setErrores(errs)
                throw new Error('Datos de facturación incompletos.')
              }
              const f = formRef.current
              const payload = {
                rev_guid: reserva.rev_guid,
                paypal_order_id: data.orderID,
                nombre_receptor: f.nombre_receptor.trim(),
                correo_receptor: f.correo_receptor.trim(),
              }
              if (!estaAutenticado && reserva.rev_codigo) {
                payload.rev_codigo = reserva.rev_codigo
              }
              if (f.apellido_receptor.trim()) payload.apellido_receptor = f.apellido_receptor.trim()
              if (f.telefono_receptor.trim()) payload.telefono_receptor = f.telefono_receptor.trim()
              if (f.observacion.trim()) payload.observacion = f.observacion.trim()
              setProcesandoCaptura(true)
              setErrorPago('')
              try {
                const resp = await reservasApi.capturarOrdenPayPal(payload)
                const factura = resp?.data
                onPagoRef.current(factura)
              } catch (err) {
                const msg =
                  err?.response?.data?.message ||
                  err?.response?.data?.details?.[0] ||
                  err?.message ||
                  'No se pudo completar el pago.'
                setErrorPago(msg)
                throw err
              } finally {
                setProcesandoCaptura(false)
              }
            },
            onError: (err) => {
              const msg = err?.message || 'Error en el checkout de PayPal.'
              setErrorPago(msg)
            },
          })
          .render(el)
      } catch (e) {
        if (!cancelled) setErrorPago(e?.message || 'No se pudo iniciar PayPal.')
      }
    })()
    return () => {
      cancelled = true
      el.innerHTML = ''
    }
  }, [clientId, reserva?.rev_guid, reserva?.rev_codigo, estaAutenticado, moneda])

  const revSubtotal = Number(reserva?.rev_subtotal ?? subtotal ?? 0)
  const revIva = Number(reserva?.rev_valor_iva ?? iva ?? 0)
  const revTotal = Number(reserva?.rev_total ?? total ?? 0)

  return (
    <section className="page-section">
      <div className="confirmacion-card fade-in">
        <div className="check-icon">💳</div>
        <h1>Pago con PayPal</h1>
        <p style={{ color: 'var(--text-muted)', marginBottom: '1.25rem' }}>
          Completa los datos de facturación y usa el botón de PayPal. El cargo se confirma en el servidor; no ingreses datos de tarjeta en esta web.
        </p>

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
          <span>${revTotal.toFixed(2)} {moneda}</span>
        </div>

        <form noValidate style={{ marginTop: '1.75rem', textAlign: 'left', width: '100%' }} onSubmit={(e) => e.preventDefault()}>
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
        </form>

        {!clientId && (
          <div className="info-message" style={{ marginTop: '1rem' }}>
            Falta configurar <code>VITE_PAYPAL_CLIENT_ID</code> en el entorno del frontend (clave pública de sandbox o live).
          </div>
        )}

        <ErrorMessage mensaje={errorPago} />

        <div id="paypal-button-container" style={{ marginTop: '1.25rem' }} />

        {procesandoCaptura && (
          <p className="text-muted text-sm" style={{ marginTop: '0.75rem' }}>
            <span className="spinner spinner-sm" /> Confirmando pago en el servidor…
          </p>
        )}
      </div>
    </section>
  )
}

// ─── Pantalla de elección auth / invitado ─────────────────────────────────────
function PantallaEleccion({ onRegistrarse, onIniciarSesion }) {
  return (
    <div className="auth-card fade-in" style={{ textAlign: 'center' }}>
      <h2>Cuenta necesaria para reservar</h2>
      <p className="text-muted" style={{ margin: '0.75rem 0 1.5rem' }}>
        Crea una cuenta o inicia sesión. Al volver, tus datos personales se completarán solos.
      </p>
      <div className="inline-form" style={{ justifyContent: 'center', flexDirection: 'column', gap: '0.75rem' }}>
        <button className="btn btn-full" type="button" onClick={onRegistrarse}>
          Crear cuenta y continuar
        </button>
        <button className="btn btn-outline btn-full" type="button" onClick={onIniciarSesion}>
          Ya tengo cuenta — Iniciar sesión
        </button>
      </div>
    </div>
  )
}

// ─── Datos personales (perfil autocompletado si hay sesión) ───────────────────
function FormularioDatosCliente({
  onConfirmar,
  onCancelar,
  valoresIniciales,
  cargandoPerfil,
  errorPerfil,
  creando,
}) {
  const [form, setForm] = useState({
    tipo_identificacion: '',
    numero_identificacion: '',
    nombres: '',
    apellidos: '',
    correo: '',
    telefono: '',
  })
  const [errores, setErrores] = useState({})

  useEffect(() => {
    if (!valoresIniciales) return
    setForm((prev) => ({
      ...prev,
      tipo_identificacion: valoresIniciales.tipo_identificacion ?? prev.tipo_identificacion,
      numero_identificacion: valoresIniciales.numero_identificacion ?? prev.numero_identificacion,
      nombres: valoresIniciales.nombres ?? prev.nombres,
      apellidos: valoresIniciales.apellidos ?? prev.apellidos,
      correo: valoresIniciales.correo ?? prev.correo,
      telefono: valoresIniciales.telefono ?? prev.telefono,
    }))
  }, [valoresIniciales])

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
      <h2>Datos personales</h2>
      <p className="text-muted" style={{ marginBottom: '1.25rem' }}>
        Revisa tus datos. Si iniciaste sesión, se cargan desde tu perfil.
      </p>
      {cargandoPerfil && <Spinner message="Cargando tu perfil..." />}
      <ErrorMessage mensaje={errorPerfil} />
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
        <button className="btn" type="button" onClick={handleConfirmar} disabled={creando || cargandoPerfil}>
          {creando ? (
            <><span className="spinner spinner-sm" /> Procesando reserva...</>
          ) : 'Confirmar y continuar al pago'}
        </button>
        <button className="btn btn-outline" type="button" onClick={onCancelar} disabled={creando}>
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

  // cargando | eleccion | formulario | datos | pago | confirmacion
  const [paso, setPaso] = useState('cargando')
  const [datosCliente, setDatosCliente] = useState(null)
  const [horGuid, setHorGuid] = useState('')
  const [cantidades, setCantidades] = useState({})
  const [intentoEnvio, setIntentoEnvio] = useState(false)
  const [tickets, setTickets] = useState([])
  const [horarios, setHorarios] = useState([])

  const horarioSeleccionado = useMemo(
    () => horarios.find((h) => h.hor_guid === horGuid) ?? null,
    [horarios, horGuid],
  )

  // Estado del pago
  const [reservaLocal, setReservaLocal] = useState(null)
  const [factura, setFactura] = useState(null)
  const [errorPago, setErrorPago] = useState('')

  const { detalle, cargarDetalle, cargando, error } = useAtracciones({})
  const { crearReserva, error: errorReserva, cargando: creando } = useReserva()
  const { perfil, cargando: cargandoPerfil, error: errorPerfil, cargarPerfil } = usePerfilCliente()

  const valoresFormularioPerfil = useMemo(
    () => mapPerfilAFormulario(perfil),
    [perfil],
  )

  const datosFacturacionIniciales = useMemo(
    () => (datosCliente ? mapPerfilAPago(datosCliente) : mapPerfilAPago(perfil)),
    [datosCliente, perfil],
  )

  useEffect(() => {
    cargarDetalle(guid)
      .then(() => {
        if (estaAutenticado) setPaso('formulario')
        else navigate('/registro', { state: { from: location } })
      })
      .catch(() => {
        if (estaAutenticado) setPaso('formulario')
        else navigate('/registro', { state: { from: location } })
      })
  }, [guid]) // eslint-disable-line react-hooks/exhaustive-deps

  useEffect(() => {
    if (estaAutenticado && paso === 'eleccion') setPaso('formulario')
  }, [estaAutenticado, paso])

  useEffect(() => {
    if (paso === 'datos' && estaAutenticado) {
      cargarPerfil().catch(() => {})
    }
  }, [paso, estaAutenticado, cargarPerfil])

  useEffect(() => {
    if (!guid) return
    obtenerTicketsAtraccion(guid)
      .then((data) => setTickets(Array.isArray(data) ? data : []))
      .catch(() => setTickets([]))
    obtenerHorariosDisponibles(guid)
      .then((data) => setHorarios(Array.isArray(data) ? data : []))
      .catch(() => setHorarios([]))
  }, [guid])

  const ticketsFiltrados = useMemo(() => {
    if (!horarioSeleccionado?.tck_guid) return []
    return tickets.filter((t) => t.tck_guid === horarioSeleccionado.tck_guid)
  }, [tickets, horarioSeleccionado])

  const lineas = useMemo(
    () =>
      Object.entries(cantidades)
        .filter(([, cantidad]) => Number(cantidad) > 0)
        .map(([tck_guid, cantidad]) => ({ tck_guid, cantidad: Number(cantidad) })),
    [cantidades],
  )

  const subtotal = useMemo(() => {
    return ticketsFiltrados.reduce((acc, ticket) => {
      const cantidad = Number(cantidades[ticket.tck_guid] || 0)
      return acc + cantidad * Number(ticket.precio || 0)
    }, 0)
  }, [cantidades, ticketsFiltrados])

  const iva = subtotal * 0.15
  const total = subtotal + iva
  const sinTickets = lineas.length === 0
  const sinHorario = !horGuid

  const handleRegistrarse = () => {
    navigate('/registro', { state: { from: location } })
  }

  const handleIniciarSesion = () => {
    navigate('/login', { state: { from: location } })
  }

  const handleSubmit = (event) => {
    event.preventDefault()
    setIntentoEnvio(true)
    if (sinHorario || sinTickets) return
    if (!estaAutenticado) {
      navigate('/registro', { state: { from: location } })
      return
    }
    setPaso('datos')
  }

  const handleConfirmarDatos = async (datos) => {
    setDatosCliente(datos)
    try {
      const reserva = await crearReserva(guid, horGuid, lineas, 'web', null)
      setReservaLocal(reserva)
      setPaso('pago')
    } catch {
      // errorReserva en el formulario de datos
    }
  }

  const handlePagoExitoso = useCallback((facturaData) => {
    setFactura(facturaData)
    setPaso('confirmacion')
  }, [])

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
        estaAutenticado={estaAutenticado}
        onPagoExitoso={handlePagoExitoso}
        errorPago={errorPago}
        setErrorPago={setErrorPago}
        datosFacturacionIniciales={datosFacturacionIniciales}
      />
    )
  }

  if (paso === 'datos') {
    return (
      <section className="page-section">
        <div style={{ marginBottom: '1.5rem' }}>
          <button
            type="button"
            className="text-muted text-sm"
            style={{ background: 'none', border: 'none', cursor: 'pointer', padding: 0 }}
            onClick={() => setPaso('formulario')}
          >
            ← Volver a entradas
          </button>
          <h1 style={{ marginTop: '0.5rem' }}>Reservar: {detalle?.nombre}</h1>
        </div>
        <FormularioDatosCliente
          valoresIniciales={valoresFormularioPerfil}
          cargandoPerfil={cargandoPerfil}
          errorPerfil={errorPerfil}
          creando={creando}
          onConfirmar={handleConfirmarDatos}
          onCancelar={() => setPaso('formulario')}
        />
        <ErrorMessage mensaje={errorReserva} />
      </section>
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
          onRegistrarse={handleRegistrarse}
          onIniciarSesion={handleIniciarSesion}
        />
      )}

      {paso === 'formulario' && (
        <>
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
                  onChange={(e) => { setHorGuid(e.target.value); setCantidades({}); setIntentoEnvio(false) }}
                  className={intentoEnvio && sinHorario ? 'input-error' : ''}
                >
                  <option value="">— Elige una fecha y hora —</option>
                  {horarios.map((horario, index) => (
                    <option key={horario.hor_guid || index} value={horario.hor_guid}>
                      {horario.fecha} {horario.hora_inicio}
                      {horario.ticket_titulo ? ` — ${horario.ticket_titulo}` : ''}
                      {(horario.cupos ?? horario.cupos_disponibles) != null
                        ? ` — ${horario.cupos ?? horario.cupos_disponibles} cupos`
                        : ''}
                    </option>
                  ))}
                </select>
                {intentoEnvio && sinHorario && (
                  <span className="field-error">⚠ Selecciona un horario para continuar</span>
                )}
              </div>

              <div className="form-group">
                <label>Cantidad de entradas *</label>
                {!horGuid ? (
                  <p className="text-muted text-sm" style={{ marginTop: '0.5rem' }}>
                    Selecciona primero el horario para ver las entradas disponibles.
                  </p>
                ) : ticketsFiltrados.length === 0 ? (
                  <p className="text-muted text-sm" style={{ marginTop: '0.5rem' }}>
                    No hay información de tarifa para este horario.
                  </p>
                ) : (
                  <div className="tickets-box">
                    {ticketsFiltrados.map((ticket) => (
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
                )}
                {intentoEnvio && sinTickets && (
                  <span className="field-error">⚠ Selecciona al menos una entrada</span>
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
