import { Fragment, useEffect, useState } from 'react'
import * as reseniasApi from '../../api/reseniasApi'
import * as reservasApi from '../../api/reservasApi'
import ErrorMessage from '../../components/common/ErrorMessage'
import Spinner from '../../components/common/Spinner'
import { emitirToast } from '../../components/common/Toast'
import { estadoBadgeClass, estadoLabel } from '../../utils/estadoReserva'
import { useMisReservas } from '../hooks/useMisReservas'

// ──────────────────────────────────────────────
// Modal de cancelación
// ──────────────────────────────────────────────
function ModalCancelar({ reserva, onCerrar, onExito }) {
  const [motivo, setMotivo] = useState('')
  const [cargando, setCargando] = useState(false)
  const [error, setError] = useState('')

  const handleCancelar = async () => {
    if (!motivo.trim()) { setError('Indica el motivo de la cancelación'); return }
    setCargando(true)
    setError('')
    try {
      await reservasApi.cancelarReserva(reserva.rev_guid, motivo.trim())
      emitirToast('Reserva cancelada correctamente.', 'success')
      onExito()
    } catch (err) {
      if (err?.response?.status === 409) {
        setError('Esta reserva ya está cancelada o no se puede cancelar.')
      } else if (err?.response?.status === 403) {
        setError('No tienes permiso para cancelar esta reserva.')
      } else {
        setError(err?.response?.data?.message || 'No se pudo cancelar la reserva.')
      }
    } finally {
      setCargando(false)
    }
  }

  return (
    <div className="modal-overlay" onClick={onCerrar}>
      <div className="modal-card" onClick={(e) => e.stopPropagation()}>
        <h2>Cancelar reserva</h2>
        <p>Código: <strong>{reserva.rev_codigo}</strong></p>
        <div className="form-group" style={{ marginTop: '0.75rem' }}>
          <label htmlFor="motivo-cancel">Motivo de cancelación</label>
          <textarea
            id="motivo-cancel"
            value={motivo}
            onChange={(e) => setMotivo(e.target.value)}
            rows={3}
            placeholder="Describe el motivo..."
          />
        </div>
        <ErrorMessage mensaje={error} />
        <div className="modal-actions" style={{ marginTop: '1rem' }}>
          <button className="btn btn-outline" onClick={onCerrar} disabled={cargando}>Volver</button>
          <button className="btn btn-danger" onClick={handleCancelar} disabled={cargando}>
            {cargando ? <><span className="spinner spinner-sm" /> Cancelando...</> : 'Confirmar cancelación'}
          </button>
        </div>
      </div>
    </div>
  )
}

// ──────────────────────────────────────────────
// Formulario inline de reseña
// ──────────────────────────────────────────────
function FormResenia({ reserva, onCerrar }) {
  const [rating, setRating] = useState(5)
  const [comentario, setComentario] = useState('')
  const [cargando, setCargando] = useState(false)
  const [error, setError] = useState('')
  const [enviado, setEnviado] = useState(false)

  const handleEnviar = async () => {
    setCargando(true)
    setError('')
    try {
      await reseniasApi.crearResenia({ rev_guid: reserva.rev_guid, rating, comentario })
      setEnviado(true)
    } catch (err) {
      if (err?.response?.status === 409) {
        setError('Ya registraste una reseña para esta reserva.')
      } else {
        setError(err?.response?.data?.message || err?.response?.data?.details?.[0] || 'No se pudo enviar la reseña.')
      }
    } finally {
      setCargando(false)
    }
  }

  if (enviado) {
    return (
      <div className="success-message" style={{ margin: '0.5rem 0' }}>
        <span>✓</span><span>Reseña registrada correctamente. Gracias por tu opinión.</span>
        <button className="btn btn-outline btn-sm" onClick={onCerrar} style={{ marginLeft: 'auto' }}>Cerrar</button>
      </div>
    )
  }

  return (
    <div className="reserva-card" style={{ margin: '0.5rem 0' }}>
      <h4 style={{ marginBottom: '0.75rem' }}>Reseña — {reserva.atraccion_nombre}</h4>
      <div className="form-group">
        <label>Calificación</label>
        <div style={{ display: 'flex', gap: '0.25rem', alignItems: 'center', marginTop: '0.25rem' }}>
          {[1, 2, 3, 4, 5].map((n) => (
            <button
              key={n}
              type="button"
              onClick={() => setRating(n)}
              style={{
                background: 'none', border: 'none', cursor: 'pointer',
                fontSize: '1.6rem', padding: 0,
                color: n <= rating ? '#f5c518' : 'rgba(255,255,255,0.25)',
              }}
            >★</button>
          ))}
          <span className="text-muted text-sm" style={{ marginLeft: '0.5rem' }}>{rating}/5</span>
        </div>
      </div>
      <div className="form-group" style={{ marginTop: '0.5rem' }}>
        <label htmlFor="resenia-comentario">Comentario</label>
        <textarea
          id="resenia-comentario"
          value={comentario}
          onChange={(e) => setComentario(e.target.value)}
          rows={3}
          placeholder="Comparte tu experiencia..."
        />
      </div>
      <ErrorMessage mensaje={error} />
      <div className="inline-form" style={{ marginTop: '0.75rem' }}>
        <button className="btn" onClick={handleEnviar} disabled={cargando}>
          {cargando ? <><span className="spinner spinner-sm" /> Enviando...</> : 'Enviar reseña'}
        </button>
        <button className="btn btn-outline" onClick={onCerrar}>Cancelar</button>
      </div>
    </div>
  )
}

// ──────────────────────────────────────────────
// Página principal
// ──────────────────────────────────────────────
function MisReservasPage() {
  const [busqueda, setBusqueda] = useState('')
  const { reservas, cargando, error, cargarReservas, buscarReserva } = useMisReservas()
  const [modalCancelar, setModalCancelar] = useState(null)
  const [reseniaPara, setReseniaPara] = useState(null)

  useEffect(() => {
    cargarReservas().catch(() => {})
  }, [cargarReservas])

  const handleBuscar = (event) => {
    event.preventDefault()
    buscarReserva(busqueda)
  }

  const handleVerTodas = () => {
    setBusqueda('')
    cargarReservas().catch(() => {})
  }

  return (
    <section className="page-section">
      <h1 style={{ marginBottom: '1.5rem' }}>Mis reservas</h1>

      <form className="inline-form" onSubmit={handleBuscar} style={{ marginBottom: '1rem' }}>
        <input
          type="text"
          placeholder="Buscar por código (ej. RES-001)"
          value={busqueda}
          onChange={(e) => setBusqueda(e.target.value)}
          style={{ flex: 1 }}
        />
        <button className="btn btn-outline btn-sm" type="submit">Buscar</button>
        <button className="btn btn-outline btn-sm" type="button" onClick={handleVerTodas}>Ver todas</button>
      </form>

      {cargando && <Spinner message="Consultando reservas..." />}
      <ErrorMessage mensaje={error} />

      {!cargando && (
        <div className="table-wrap">
          <table className="admin-table">
            <thead>
              <tr>
                <th>Código</th>
                <th>Atracción</th>
                <th>Fecha reserva</th>
                <th>Estado</th>
                <th>Total</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {reservas.length === 0 && (
                <tr>
                  <td colSpan={6} style={{ textAlign: 'center', color: 'var(--text-muted)', padding: '2rem' }}>
                    {busqueda ? 'No se encontraron reservas con ese código.' : 'No tienes reservas aún.'}
                  </td>
                </tr>
              )}
              {reservas.map((reserva) => {
                // El backend usa 'A' (activa) como estado cancelable.
                const estado = String(reserva.rev_estado ?? '').toUpperCase()
                const activa = estado === 'A'
                const mostrandoResenia = reseniaPara?.rev_guid === reserva.rev_guid
                const key = reserva.rev_guid ?? reserva.rev_codigo ?? Math.random()

                return (
                  <Fragment key={key}>
                    <tr>
                      <td>
                        <span style={{ fontFamily: 'monospace', fontSize: '0.85rem' }}>
                          {reserva.rev_codigo || '—'}
                        </span>
                      </td>
                      <td>{reserva.atraccion_nombre || '—'}</td>
                      <td>
                        {reserva.rev_fecha_reserva_utc
                          ? String(reserva.rev_fecha_reserva_utc).slice(0, 10)
                          : '—'}
                      </td>
                      <td>
                        <span className={estadoBadgeClass(reserva.rev_estado)}>
                          {estadoLabel(reserva.rev_estado)}
                        </span>
                      </td>
                      <td>${Number(reserva.rev_total ?? 0).toFixed(2)}</td>
                      <td>
                        <div style={{ display: 'flex', gap: '0.4rem', flexWrap: 'wrap' }}>
                          {activa && (
                            <button
                              className="btn btn-danger btn-sm"
                              onClick={() => setModalCancelar(reserva)}
                            >
                              Cancelar
                            </button>
                          )}
                          {activa && (
                            <button
                              className="btn btn-outline btn-sm"
                              onClick={() => setReseniaPara(mostrandoResenia ? null : reserva)}
                            >
                              {mostrandoResenia ? 'Ocultar' : '★ Reseña'}
                            </button>
                          )}
                        </div>
                      </td>
                    </tr>
                    {mostrandoResenia && (
                      <tr>
                        <td colSpan={6} style={{ padding: '0.5rem 0.75rem' }}>
                          <FormResenia reserva={reserva} onCerrar={() => setReseniaPara(null)} />
                        </td>
                      </tr>
                    )}
                  </Fragment>
                )
              })}
            </tbody>
          </table>
        </div>
      )}

      {modalCancelar && (
        <ModalCancelar
          reserva={modalCancelar}
          onCerrar={() => setModalCancelar(null)}
          onExito={() => {
            setModalCancelar(null)
            cargarReservas().catch(() => {})
          }}
        />
      )}
    </section>
  )
}

export default MisReservasPage
