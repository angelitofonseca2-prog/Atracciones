import { useEffect, useState } from 'react'
import { adminApi } from '../../api/adminApi'
import ErrorMessage from '../../components/common/ErrorMessage'
import ModalConfirmacion from '../../components/common/ModalConfirmacion'
import Spinner from '../../components/common/Spinner'
import { emitirToast } from '../../components/common/Toast'

/**
 * Catálogo `incluye` (`/admin/incluye`).
 *
 * Contrato (snake_case):
 *  - IncluyeResponse: { incluye_guid, descripcion, estado }
 *  - CrearIncluyeRequest: { descripcion }
 *  - ActualizarIncluyeRequest: { descripcion?, estado? }
 */
function GestionIncluyePage() {
  const [items, setItems] = useState([])
  const [cargando, setCargando] = useState(false)
  const [error, setError] = useState('')
  const [descripcion, setDescripcion] = useState('')
  const [editando, setEditando] = useState(null)
  const [guardando, setGuardando] = useState(false)
  const [errorCampo, setErrorCampo] = useState('')
  const [mostrarForm, setMostrarForm] = useState(false)
  const [confirmando, setConfirmando] = useState(null)
  const [eliminando, setEliminando] = useState(false)

  const cargar = async () => {
    setCargando(true)
    setError('')
    try {
      const data = await adminApi.listarIncluye()
      setItems(Array.isArray(data) ? data : [])
    } catch (err) {
      setError(err?.response?.data?.message || 'No se pudieron cargar los elementos.')
    } finally {
      setCargando(false)
    }
  }

  useEffect(() => { cargar() }, [])

  const handleNuevo = () => {
    setEditando(null)
    setDescripcion('')
    setErrorCampo('')
    setMostrarForm(true)
  }

  const handleEditar = (item) => {
    setEditando(item)
    setDescripcion(item.descripcion ?? '')
    setErrorCampo('')
    setMostrarForm(true)
  }

  const handleGuardar = async (e) => {
    e.preventDefault()
    if (!descripcion.trim()) { setErrorCampo('La descripción es obligatoria'); return }
    setGuardando(true)
    try {
      if (editando) {
        await adminApi.updateIncluye(editando.incluye_guid, { descripcion: descripcion.trim() })
        emitirToast('Cambios guardados correctamente.', 'success')
      } else {
        await adminApi.createIncluye({ descripcion: descripcion.trim() })
        emitirToast('Registro creado correctamente.', 'success')
      }
      setMostrarForm(false)
      await cargar()
    } catch (err) {
      emitirToast(
        err?.response?.data?.message || 'No se pudo guardar el elemento.',
        'error',
      )
    } finally {
      setGuardando(false)
    }
  }

  const ejecutarEliminar = async () => {
    if (!confirmando) return
    setEliminando(true)
    try {
      await adminApi.deleteIncluye(confirmando.incluye_guid)
      emitirToast('Registro eliminado correctamente.', 'success')
      setConfirmando(null)
      await cargar()
    } catch (err) {
      emitirToast(
        err?.response?.data?.message || 'No se pudo eliminar el elemento.',
        'error',
      )
    } finally {
      setEliminando(false)
    }
  }

  return (
    <section className="page-section">
      <div className="admin-page-header">
        <h1>Gestión de Elementos Incluidos</h1>
        <button className="btn" type="button" onClick={handleNuevo}>Nuevo elemento</button>
      </div>

      {mostrarForm && (
        <form className="admin-form" onSubmit={handleGuardar} noValidate style={{ marginBottom: '1.5rem' }}>
          <h3 style={{ marginBottom: '1rem' }}>{editando ? 'Editar elemento' : 'Nuevo elemento'}</h3>
          <div className="form-group">
            <label htmlFor="inc-desc">Descripción *</label>
            <input
              id="inc-desc"
              type="text"
              value={descripcion}
              onChange={(e) => { setDescripcion(e.target.value); setErrorCampo('') }}
              placeholder="ej. Guía bilingüe"
              maxLength={150}
              className={errorCampo ? 'input-error' : ''}
            />
            {errorCampo && <span className="field-error">⚠ {errorCampo}</span>}
          </div>
          <div className="inline-form" style={{ marginTop: '0.75rem' }}>
            <button className="btn" type="submit" disabled={guardando}>
              {guardando ? <><span className="spinner spinner-sm" /> Guardando...</> : 'Guardar'}
            </button>
            <button className="btn btn-outline" type="button" onClick={() => setMostrarForm(false)} disabled={guardando}>
              Cancelar
            </button>
          </div>
        </form>
      )}

      {cargando && <Spinner message="Cargando elementos..." />}
      <ErrorMessage mensaje={error} />

      <div className="table-wrap">
        <table className="admin-table">
          <thead>
            <tr><th>Descripción</th><th>Estado</th><th>Acciones</th></tr>
          </thead>
          <tbody>
            {items.length === 0 && !cargando && (
              <tr>
                <td colSpan={3} style={{ textAlign: 'center', color: 'var(--text-muted)', padding: '2rem' }}>
                  No hay elementos registrados.
                </td>
              </tr>
            )}
            {items.map((item) => (
              <tr key={item.incluye_guid}>
                <td>{item.descripcion}</td>
                <td>{item.estado || '—'}</td>
                <td>
                  <button className="btn btn-outline btn-sm" style={{ marginRight: '0.5rem' }} onClick={() => handleEditar(item)}>Editar</button>
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
      </div>

      <ModalConfirmacion
        abierto={Boolean(confirmando)}
        titulo="¿Eliminar elemento?"
        descripcion={
          confirmando
            ? `Se eliminará "${confirmando.descripcion}". Esta acción no se puede deshacer.`
            : ''
        }
        textoConfirmar="Eliminar"
        cargando={eliminando}
        onConfirmar={ejecutarEliminar}
        onCancelar={() => !eliminando && setConfirmando(null)}
      />
    </section>
  )
}

export default GestionIncluyePage
