import { useEffect, useState } from 'react'
import { adminApi } from '../../api/adminApi'
import ErrorMessage from '../../components/common/ErrorMessage'
import ModalConfirmacion from '../../components/common/ModalConfirmacion'
import Spinner from '../../components/common/Spinner'
import { emitirToast } from '../../components/common/Toast'

/**
 * Catálogo de categorías (`/admin/categorias`).
 *
 * Contrato (snake_case):
 *  - CategoriaResponse: { cat_guid, nombre, parent_guid?, parent_nombre?, estado }
 *  - CrearCategoriaRequest: { nombre, parent_guid? }
 *  - ActualizarCategoriaRequest: { nombre?, parent_guid?, estado? }
 */
function GestionCategoriasPage() {
  const [items, setItems] = useState([])
  const [cargando, setCargando] = useState(false)
  const [error, setError] = useState('')
  const [form, setForm] = useState({ nombre: '', parent_guid: '' })
  const [editando, setEditando] = useState(null)
  const [guardando, setGuardando] = useState(false)
  const [errores, setErrores] = useState({})
  const [mostrarForm, setMostrarForm] = useState(false)
  const [confirmando, setConfirmando] = useState(null)
  const [eliminando, setEliminando] = useState(false)

  const cargar = async () => {
    setCargando(true)
    setError('')
    try {
      const data = await adminApi.listarCategorias()
      setItems(Array.isArray(data) ? data : [])
    } catch (err) {
      setError(err?.response?.data?.message || 'No se pudieron cargar las categorías.')
    } finally {
      setCargando(false)
    }
  }

  useEffect(() => { cargar() }, [])

  const set = (campo) => (e) => {
    setForm((p) => ({ ...p, [campo]: e.target.value }))
    if (errores[campo]) setErrores((p) => ({ ...p, [campo]: '' }))
  }

  const handleNuevo = () => {
    setEditando(null)
    setForm({ nombre: '', parent_guid: '' })
    setErrores({})
    setMostrarForm(true)
  }

  const handleEditar = (item) => {
    setEditando(item)
    setForm({
      nombre: item.nombre ?? '',
      parent_guid: item.parent_guid ?? '',
    })
    setErrores({})
    setMostrarForm(true)
  }

  const handleGuardar = async (e) => {
    e.preventDefault()
    if (!form.nombre.trim()) { setErrores({ nombre: 'El nombre es obligatorio' }); return }
    setGuardando(true)
    try {
      const payload = { nombre: form.nombre.trim() }
      if (form.parent_guid) payload.parent_guid = form.parent_guid
      if (editando) {
        await adminApi.updateCategoria(editando.cat_guid, payload)
        emitirToast('Cambios guardados correctamente.', 'success')
      } else {
        await adminApi.createCategoria(payload)
        emitirToast('Registro creado correctamente.', 'success')
      }
      setMostrarForm(false)
      await cargar()
    } catch (err) {
      emitirToast(
        err?.response?.data?.message || 'No se pudo guardar la categoría.',
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
      await adminApi.deleteCategoria(confirmando.cat_guid)
      emitirToast('Registro eliminado correctamente.', 'success')
      setConfirmando(null)
      await cargar()
    } catch (err) {
      emitirToast(
        err?.response?.data?.message || 'No se pudo eliminar la categoría.',
        'error',
      )
    } finally {
      setEliminando(false)
    }
  }

  // Para el selector "Padre" no listamos a la propia categoría editada.
  const opcionesPadre = items.filter((i) => !editando || i.cat_guid !== editando.cat_guid)

  return (
    <section className="page-section">
      <div className="admin-page-header">
        <h1>Gestión de Categorías</h1>
        <button className="btn" type="button" onClick={handleNuevo}>Nueva categoría</button>
      </div>

      {mostrarForm && (
        <form className="admin-form" onSubmit={handleGuardar} noValidate style={{ marginBottom: '1.5rem' }}>
          <h3 style={{ marginBottom: '1rem' }}>{editando ? 'Editar categoría' : 'Nueva categoría'}</h3>

          <div className="form-group">
            <label htmlFor="cat-nombre">Nombre *</label>
            <input
              id="cat-nombre"
              type="text"
              value={form.nombre}
              onChange={set('nombre')}
              placeholder="ej. Aventura"
              maxLength={120}
              className={errores.nombre ? 'input-error' : ''}
            />
            {errores.nombre && <span className="field-error">⚠ {errores.nombre}</span>}
          </div>

          <div className="form-group">
            <label htmlFor="cat-parent">Categoría padre <span className="text-muted">(opcional)</span></label>
            <select id="cat-parent" value={form.parent_guid} onChange={set('parent_guid')}>
              <option value="">— Sin categoría padre —</option>
              {opcionesPadre.map((c) => (
                <option key={c.cat_guid} value={c.cat_guid}>{c.nombre}</option>
              ))}
            </select>
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

      {cargando && <Spinner message="Cargando categorías..." />}
      <ErrorMessage mensaje={error} />

      <div className="table-wrap">
        <table className="admin-table">
          <thead>
            <tr><th>Nombre</th><th>Padre</th><th>Estado</th><th>Acciones</th></tr>
          </thead>
          <tbody>
            {items.length === 0 && !cargando && (
              <tr>
                <td colSpan={4} style={{ textAlign: 'center', color: 'var(--text-muted)', padding: '2rem' }}>
                  No hay categorías registradas.
                </td>
              </tr>
            )}
            {items.map((item) => (
              <tr key={item.cat_guid}>
                <td>{item.nombre}</td>
                <td>{item.parent_nombre || '—'}</td>
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
        titulo="¿Eliminar categoría?"
        descripcion={
          confirmando
            ? `Se eliminará "${confirmando.nombre}". Esta acción no se puede deshacer.`
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

export default GestionCategoriasPage
