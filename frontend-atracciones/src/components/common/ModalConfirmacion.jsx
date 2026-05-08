/**
 * Modal de confirmación reutilizable.
 *
 * Props:
 *  - abierto: boolean
 *  - titulo: string
 *  - descripcion: string
 *  - textoConfirmar: string (default "Confirmar")
 *  - textoCancelar: string (default "Cancelar")
 *  - variante: 'danger' | 'primary' (default 'danger')
 *  - cargando: boolean
 *  - onConfirmar: () => void
 *  - onCancelar: () => void
 */
function ModalConfirmacion({
  abierto,
  titulo = '¿Estás seguro?',
  descripcion = 'Esta acción no se puede deshacer.',
  textoConfirmar = 'Confirmar',
  textoCancelar = 'Cancelar',
  variante = 'danger',
  cargando = false,
  onConfirmar,
  onCancelar,
}) {
  if (!abierto) return null

  const btnClass = variante === 'danger' ? 'btn btn-danger' : 'btn'

  return (
    <div className="modal-overlay" role="dialog" aria-modal="true" aria-labelledby="modal-title">
      <div className="modal-card">
        <h2 id="modal-title">{titulo}</h2>
        {descripcion && <p>{descripcion}</p>}
        <div className="modal-actions">
          <button
            type="button"
            className="btn btn-outline"
            onClick={onCancelar}
            disabled={cargando}
          >
            {textoCancelar}
          </button>
          <button
            type="button"
            className={btnClass}
            onClick={onConfirmar}
            disabled={cargando}
          >
            {cargando ? (
              <><span className="spinner spinner-sm" /> Procesando...</>
            ) : textoConfirmar}
          </button>
        </div>
      </div>
    </div>
  )
}

export default ModalConfirmacion
