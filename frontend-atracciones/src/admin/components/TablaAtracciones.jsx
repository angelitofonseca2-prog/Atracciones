import { useState } from 'react'
import ModalConfirmacion from '../../components/common/ModalConfirmacion'

/**
 * Tabla administrativa de atracciones.
 *
 * Lee campos según el contrato `AtraccionAdminResponse`:
 *   at_guid, nombre, ciudad, pais, disponible, estado, imagen_principal.
 */
function TablaAtracciones({ items, onEditar, onDesactivar }) {
  const [confirmando, setConfirmando] = useState(null)
  const [procesando, setProcesando] = useState(false)

  const ejecutarDesactivar = async () => {
    if (!confirmando) return
    setProcesando(true)
    try {
      await onDesactivar(confirmando.at_guid)
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
            <th style={{ width: 60 }}>Imagen</th>
            <th>Nombre</th>
            <th>Ciudad</th>
            <th>País</th>
            <th>Disponible</th>
            <th>Estado</th>
            <th>Acciones</th>
          </tr>
        </thead>
        <tbody>
          {items.length === 0 && (
            <tr>
              <td colSpan={7} style={{ textAlign: 'center', color: 'var(--text-muted)', padding: '2rem' }}>
                No hay atracciones registradas.
              </td>
            </tr>
          )}
          {items.map((item) => (
            <tr key={item.at_guid}>
              <td>
                {item.imagen_principal ? (
                  <img
                    src={item.imagen_principal}
                    alt={item.nombre}
                    style={{
                      width: 44, height: 44, objectFit: 'cover',
                      borderRadius: 6, border: '1px solid rgba(255,255,255,0.15)',
                    }}
                    onError={(e) => { e.currentTarget.style.display = 'none' }}
                  />
                ) : (
                  <span className="text-muted text-sm">—</span>
                )}
              </td>
              <td>{item.nombre}</td>
              <td>{item.ciudad || '—'}</td>
              <td>{item.pais || '—'}</td>
              <td>{item.disponible ? 'Sí' : 'No'}</td>
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
                  Desactivar
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      <ModalConfirmacion
        abierto={Boolean(confirmando)}
        titulo="¿Desactivar atracción?"
        descripcion={
          confirmando
            ? `La atracción "${confirmando.nombre}" se marcará como inactiva. Esta acción es lógica y no elimina datos.`
            : ''
        }
        textoConfirmar="Desactivar"
        cargando={procesando}
        onConfirmar={ejecutarDesactivar}
        onCancelar={() => !procesando && setConfirmando(null)}
      />
    </>
  )
}

export default TablaAtracciones
