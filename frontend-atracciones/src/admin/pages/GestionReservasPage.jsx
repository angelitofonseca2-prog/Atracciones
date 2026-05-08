import { useEffect, useState } from 'react'
import { adminApi } from '../../api/adminApi'
import ErrorMessage from '../../components/common/ErrorMessage'
import ModalConfirmacion from '../../components/common/ModalConfirmacion'
import Spinner from '../../components/common/Spinner'
import { emitirToast } from '../../components/common/Toast'
import { estadoBadgeClass, estadoLabel } from '../../utils/estadoReserva'
import { useGestionReservas } from '../hooks/useGestionReservas'

/**
 * Acciones disponibles sobre cada reserva:
 *   - "Cancelar" → PUT /admin/reservas/{guid}/cancelar { nuevo_estado:'C', motivo }
 *   - "Anular"   → DELETE /admin/reservas/{guid}     body { nuevo_estado:'I', motivo }
 *   - "Reactivar"→ PUT /admin/reservas/{guid}/estado { nuevo_estado:'A', motivo }
 *
 * Estado actual del backend usa caracteres: 'A' (activa), 'C' (cancelada), 'I' (inactiva).
 */

const ACCIONES = {
  cancelar: {
    titulo: '¿Cancelar reserva?',
    descripcion: 'La reserva se marcará como cancelada y los cupos volverán a estar disponibles.',
    boton: 'Cancelar reserva',
    variante: 'danger',
    metodo: (guid, motivo) => adminApi.cancelarReservaAdmin(guid, motivo),
    exito: 'Reserva cancelada correctamente.',
    error: 'No se pudo cancelar la reserva.',
  },
  anular: {
    titulo: '¿Anular reserva?',
    descripcion: 'La reserva se marcará como inactiva (anulación administrativa).',
    boton: 'Anular',
    variante: 'danger',
    metodo: (guid, motivo) => adminApi.anularReservaAdmin(guid, motivo),
    exito: 'Reserva anulada correctamente.',
    error: 'No se pudo anular la reserva.',
  },
  reactivar: {
    titulo: '¿Reactivar reserva?',
    descripcion: 'La reserva volverá a quedar activa.',
    boton: 'Reactivar',
    variante: 'primary',
    metodo: (guid, motivo) => adminApi.actualizarEstadoReserva(guid, 'A', motivo),
    exito: 'Reserva reactivada correctamente.',
    error: 'No se pudo reactivar la reserva.',
  },
}

function GestionReservasPage() {
  const { items, cargando, error, page, totalPages, cargar } = useGestionReservas()
  const [accion, setAccion] = useState(null) // { tipo, reserva, motivo }
  const [procesando, setProcesando] = useState(false)
  const [motivo, setMotivo] = useState('')

  useEffect(() => {
    cargar(1)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  const abrirAccion = (tipo, reserva) => {
    setAccion({ tipo, reserva })
    setMotivo('')
  }

  const cerrarAccion = () => {
    if (procesando) return
    setAccion(null)
    setMotivo('')
  }

  const ejecutar = async () => {
    if (!accion) return
    const cfg = ACCIONES[accion.tipo]
    if (!cfg) return
    setProcesando(true)
    try {
      await cfg.metodo(accion.reserva.rev_guid, motivo.trim() || cfg.titulo)
      emitirToast(cfg.exito, 'success')
      setAccion(null)
      setMotivo('')
      await cargar(page)
    } catch (err) {
      emitirToast(err?.response?.data?.message || cfg.error, 'error')
    } finally {
      setProcesando(false)
    }
  }

  const cfg = accion ? ACCIONES[accion.tipo] : null

  return (
    <section className="page-section">
      <div className="admin-page-header">
        <h1>Gestión de Reservas</h1>
      </div>

      {cargando && <Spinner message="Cargando reservas..." />}
      <ErrorMessage mensaje={error} />

      <div className="table-wrap">
        <table className="admin-table">
          <thead>
            <tr>
              <th>Código</th>
              <th>Atracción</th>
              <th>Cliente</th>
              <th>Reservada</th>
              <th>Horario</th>
              <th>Estado</th>
              <th>Total</th>
              <th>Acciones</th>
            </tr>
          </thead>
          <tbody>
            {items.length === 0 && !cargando && (
              <tr>
                <td colSpan={8} style={{ textAlign: 'center', color: 'var(--text-muted)', padding: '2rem' }}>
                  No hay reservas registradas.
                </td>
              </tr>
            )}
            {items.map((item) => {
              const estado = String(item.rev_estado ?? '').toUpperCase()
              const estaActiva = estado === 'A'
              const estaCancelada = estado === 'C' || estado === 'I'
              const fechaReserva = item.rev_fecha_reserva_utc
                ? String(item.rev_fecha_reserva_utc).slice(0, 10) : '—'
              const horarioFecha = item.hor_fecha
                ? String(item.hor_fecha).slice(0, 10) : ''
              const horaInicio = item.hor_hora_inicio
                ? String(item.hor_hora_inicio).slice(0, 5) : ''
              return (
                <tr key={item.rev_guid}>
                  <td>
                    <span style={{ fontFamily: 'monospace', fontSize: '0.85rem' }}>
                      {item.rev_codigo || '—'}
                    </span>
                  </td>
                  <td>{item.atraccion_nombre || '—'}</td>
                  <td>{item.cliente_nombre || item.cliente_login || '—'}</td>
                  <td>{fechaReserva}</td>
                  <td>{horarioFecha ? `${horarioFecha}${horaInicio ? ` ${horaInicio}` : ''}` : '—'}</td>
                  <td>
                    <span className={estadoBadgeClass(item.rev_estado)}>
                      {estadoLabel(item.rev_estado)}
                    </span>
                  </td>
                  <td><strong>${Number(item.rev_total ?? 0).toFixed(2)}</strong></td>
                  <td style={{ display: 'flex', gap: '0.4rem', flexWrap: 'wrap' }}>
                    {estaActiva && (
                      <>
                        <button
                          className="btn btn-outline btn-sm"
                          style={{ color: 'var(--danger, #e55)' }}
                          onClick={() => abrirAccion('cancelar', item)}
                        >
                          Cancelar
                        </button>
                        <button
                          className="btn btn-outline btn-sm"
                          style={{ color: 'var(--danger, #e55)' }}
                          onClick={() => abrirAccion('anular', item)}
                        >
                          Anular
                        </button>
                      </>
                    )}
                    {estaCancelada && (
                      <button
                        className="btn btn-outline btn-sm"
                        onClick={() => abrirAccion('reactivar', item)}
                      >
                        Reactivar
                      </button>
                    )}
                  </td>
                </tr>
              )
            })}
          </tbody>
        </table>
      </div>

      <div className="pagination">
        <button
          className="btn btn-outline btn-sm"
          disabled={page <= 1}
          onClick={() => cargar(page - 1)}
        >
          ← Anterior
        </button>
        <span className="pagination-info">Página {page} de {totalPages}</span>
        <button
          className="btn btn-outline btn-sm"
          disabled={page >= totalPages}
          onClick={() => cargar(page + 1)}
        >
          Siguiente →
        </button>
      </div>

      <ModalConfirmacion
        abierto={Boolean(accion)}
        titulo={cfg?.titulo}
        descripcion={
          accion ? (
            <>
              <span style={{ display: 'block', marginBottom: '0.75rem' }}>
                {cfg?.descripcion} Reserva: <strong>{accion.reserva.rev_codigo}</strong>.
              </span>
              <label htmlFor="motivo-accion" style={{ display: 'block', marginBottom: '0.25rem' }}>
                Motivo {cfg?.variante === 'danger' ? '*' : '(opcional)'}
              </label>
              <textarea
                id="motivo-accion"
                value={motivo}
                onChange={(e) => setMotivo(e.target.value)}
                placeholder="Describe el motivo..."
                rows={3}
                style={{ width: '100%' }}
              />
            </>
          ) : ''
        }
        textoConfirmar={cfg?.boton || 'Confirmar'}
        variante={cfg?.variante || 'danger'}
        cargando={procesando}
        onConfirmar={ejecutar}
        onCancelar={cerrarAccion}
      />
    </section>
  )
}

export default GestionReservasPage
