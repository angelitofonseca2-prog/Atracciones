import { useState } from 'react'
import ModalConfirmacion from '../../components/common/ModalConfirmacion'

/**
 * Tabla administrativa de tickets.
 *
 * Lee campos del contrato `TicketResponse`:
 *   tck_guid, at_guid, atraccion_nombre, titulo, tipo_participante,
 *   precio, capacidad_maxima, cupos_disponibles, estado.
 */
function TablaTickets({ items, onEditar, onEliminar }) {
  const [confirmando, setConfirmando] = useState(null)
  const [procesando, setProcesando] = useState(false)

  const ejecutarEliminar = async () => {
    if (!confirmando) return
    setProcesando(true)
    try {
      await onEliminar(confirmando.tck_guid)
      setConfirmando(null)
    } finally {
      setProcesando(false)
    }
  }

  return (
    <>
      <table className="admin-table">
        <thead>
          <tr>
            <th>Atracción</th>
            <th>Título</th>
            <th>Tipo</th>
            <th>Precio</th>
            <th>Capacidad</th>
            <th>Cupos</th>
            <th>Estado</th>
            <th>Acciones</th>
          </tr>
        </thead>
        <tbody>
          {items.length === 0 && (
            <tr>
              <td colSpan={8} style={{ textAlign: 'center', color: 'var(--text-muted)', padding: '2rem' }}>
                No hay tickets registrados.
              </td>
            </tr>
          )}
          {items.map((item) => (
            <tr key={item.tck_guid}>
              <td>{item.atraccion_nombre || '—'}</td>
              <td>{item.titulo || '—'}</td>
              <td>{item.tipo_participante || '—'}</td>
              <td>${Number(item.precio ?? 0).toFixed(2)}</td>
              <td>{item.capacidad_maxima ?? '—'}</td>
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
        titulo="¿Eliminar ticket?"
        descripcion={
          confirmando
            ? `Se eliminará el ticket "${confirmando.titulo}". Esta acción no se puede deshacer.`
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

export default TablaTickets
