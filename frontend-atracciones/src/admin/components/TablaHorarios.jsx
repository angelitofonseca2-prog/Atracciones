import { useState } from 'react'
import ModalConfirmacion from '../../components/common/ModalConfirmacion'

/**
 * Tabla de horarios.
 *
 * Lee campos del contrato `HorarioResponse`:
 *   hor_guid, tck_guid, ticket_titulo, atraccion_nombre,
 *   fecha (yyyy-MM-dd), hora_inicio (HH:mm:ss), hora_fin?, cupos_disponibles, estado.
 */
function TablaHorarios({ items, onEditar, onEliminar }) {
  const [confirmando, setConfirmando] = useState(null)
  const [procesando, setProcesando] = useState(false)

  const ejecutarEliminar = async () => {
    if (!confirmando) return
    setProcesando(true)
    try {
      await onEliminar(confirmando.hor_guid)
      setConfirmando(null)
    } finally {
      setProcesando(false)
    }
  }

  const formatearHora = (h) => (h ? String(h).slice(0, 5) : '—')

  return (
    <>
      <table className="admin-table">
        <thead>
          <tr>
            <th>Atracción</th>
            <th>Ticket</th>
            <th>Fecha</th>
            <th>Inicio</th>
            <th>Fin</th>
            <th>Cupos</th>
            <th>Estado</th>
            <th>Acciones</th>
          </tr>
        </thead>
        <tbody>
          {items.length === 0 && (
            <tr>
              <td colSpan={8} style={{ textAlign: 'center', color: 'var(--text-muted)', padding: '2rem' }}>
                No hay horarios registrados.
              </td>
            </tr>
          )}
          {items.map((item) => (
            <tr key={item.hor_guid}>
              <td>{item.atraccion_nombre || '—'}</td>
              <td>{item.ticket_titulo || '—'}</td>
              <td>{item.fecha ? String(item.fecha).slice(0, 10) : '—'}</td>
              <td>{formatearHora(item.hora_inicio)}</td>
              <td>{formatearHora(item.hora_fin)}</td>
              <td>{item.cupos_disponibles ?? '—'}</td>
              <td>{item.estado || '—'}</td>
              <td>
                <button
                  className="btn btn-outline btn-sm"
                  style={{ marginRight: '0.5rem' }}
                  onClick={() => onEditar(item)}
                >
                  Editar
                </button>
                <button
                  className="btn btn-outline btn-sm"
                  style={{ color: 'var(--danger, #e55)' }}
                  onClick={() => setConfirmando(item)}
                >
                  Eliminar
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      <ModalConfirmacion
        abierto={Boolean(confirmando)}
        titulo="¿Eliminar horario?"
        descripcion={
          confirmando
            ? `Se eliminará el horario del ${confirmando.fecha} ${String(confirmando.hora_inicio || '').slice(0, 5)}. Esta acción no se puede deshacer.`
            : ''
        }
        textoConfirmar="Eliminar"
        cargando={procesando}
        onConfirmar={ejecutarEliminar}
        onCancelar={() => !procesando && setConfirmando(null)}
      />
    </>
  )
}

export default TablaHorarios
