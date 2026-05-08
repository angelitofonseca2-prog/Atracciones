import { useEffect, useState } from 'react'
import { adminApi } from '../../api/adminApi'
import ErrorMessage from '../../components/common/ErrorMessage'
import ModalConfirmacion from '../../components/common/ModalConfirmacion'
import Spinner from '../../components/common/Spinner'
import { emitirToast } from '../../components/common/Toast'

/**
 * Catálogo de destinos (`/admin/destinos`).
 *
 * Contrato (snake_case):
 *  - DestinoResponse: { des_guid, nombre, pais, imagen_url?, estado }
 *  - CrearDestinoRequest:    { nombre, pais, imagen_url? }
 *  - ActualizarDestinoRequest: { nombre?, pais?, imagen_url?, estado? }
 */
const VACIO = { nombre: '', pais: '', imagen_url: '' }

function GestionDestinosPage() {
  const [items, setItems] = useState([])
  const [cargando, setCargando] = useState(false)
  const [error, setError] = useState('')
  const [form, setForm] = useState(VACIO)
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
      const data = await adminApi.listDestinos()
      setItems(Array.isArray(data) ? data : [])
    } catch (err) {
      setError(err?.response?.data?.message || 'No se pudieron cargar los destinos.')
    } finally {
      setCargando(false)
    }
  }

  useEffect(() => { cargar() }, [])

  const set = (campo) => (e) => {
    setForm((p) => ({ ...p, [campo]: e.target.value }))
    if (errores[campo]) setErrores((p) => ({ ...p, [campo]: '' }))
  }

  const validarUrl = (u) => {
    if (!u) return true
    try { new URL(u); return true } catch { return false }
  }

  const validar = () => {
    const e = {}
    if (!form.nombre.trim()) e.nombre = 'El nombre es obligatorio'
    if (!form.pais.trim()) e.pais = 'El país es obligatorio'
    if (form.imagen_url && !validarUrl(form.imagen_url.trim())) e.imagen_url = 'URL no válida (incluye https://)'
    return e
  }

  const handleNuevo = () => {
    setEditando(null)
    setForm(VACIO)
    setErrores({})
    setMostrarForm(true)
  }

  const handleEditar = (item) => {
    setEditando(item)
    setForm({
      nombre: item.nombre ?? '',
      pais: item.pais ?? '',
      imagen_url: item.imagen_url ?? '',
    })
    setErrores({})
    setMostrarForm(true)
  }

  const handleGuardar = async (e) => {
    e.preventDefault()
    const errs = validar()
    if (Object.keys(errs).length) { setErrores(errs); return }
    setGuardando(true)
    try {
      const payload = {
        nombre: form.nombre.trim(),
        pais: form.pais.trim(),
      }
      if (form.imagen_url.trim()) payload.imagen_url = form.imagen_url.trim()
      if (editando) {
        await adminApi.updateDestino(editando.des_guid, payload)
        emitirToast('Cambios guardados correctamente.', 'success')
      } else {
        await adminApi.createDestino(payload)
        emitirToast('Registro creado correctamente.', 'success')
      }
      setMostrarForm(false)
      await cargar()
    } catch (err) {
      emitirToast(
        err?.response?.data?.message || 'No se pudo guardar el destino.',
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
      await adminApi.deleteDestino(confirmando.des_guid)
      emitirToast('Registro eliminado correctamente.', 'success')
      setConfirmando(null)
      await cargar()
    } catch (err) {
      emitirToast(
        err?.response?.data?.message || 'No se pudo eliminar el destino.',
        'error',
      )
    } finally {
      setEliminando(false)
    }
  }

  return (
    <section className="page-section">
      <div className="admin-page-header">
        <h1>Gestión de Destinos</h1>
        <button className="btn" type="button" onClick={handleNuevo}>Nuevo destino</button>
      </div>

      {mostrarForm && (
        <form className="admin-form" onSubmit={handleGuardar} noValidate style={{ marginBottom: '1.5rem' }}>
          <h3 style={{ marginBottom: '1rem' }}>{editando ? 'Editar destino' : 'Nuevo destino'}</h3>

          <div className="form-group">
            <label htmlFor="dest-nombre">Nombre *</label>
            <input
              id="dest-nombre"
              type="text"
              value={form.nombre}
              onChange={set('nombre')}
              placeholder="ej. Quito"
              maxLength={150}
              className={errores.nombre ? 'input-error' : ''}
            />
            {errores.nombre && <span className="field-error">⚠ {errores.nombre}</span>}
          </div>

          <div className="form-group">
            <label htmlFor="dest-pais">País *</label>
            <input
              id="dest-pais"
              type="text"
              value={form.pais}
              onChange={set('pais')}
              placeholder="ej. Ecuador"
              maxLength={100}
              className={errores.pais ? 'input-error' : ''}
            />
            {errores.pais && <span className="field-error">⚠ {errores.pais}</span>}
          </div>

          <div className="form-group">
            <label htmlFor="dest-imagen">URL de imagen <span className="text-muted">(opcional)</span></label>
            <input
              id="dest-imagen"
              type="url"
              value={form.imagen_url}
              onChange={set('imagen_url')}
              placeholder="https://..."
              maxLength={500}
              className={errores.imagen_url ? 'input-error' : ''}
            />
            {errores.imagen_url && <span className="field-error">⚠ {errores.imagen_url}</span>}
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

      {cargando && <Spinner message="Cargando destinos..." />}
      <ErrorMessage mensaje={error} />

      <div className="table-wrap">
        <table className="admin-table">
          <thead>
            <tr>
              <th style={{ width: 60 }}>Imagen</th>
              <th>Nombre</th>
              <th>País</th>
              <th>Estado</th>
              <th>Acciones</th>
            </tr>
          </thead>
          <tbody>
            {items.length === 0 && !cargando && (
              <tr>
                <td colSpan={5} style={{ textAlign: 'center', color: 'var(--text-muted)', padding: '2rem' }}>
                  No hay destinos registrados.
                </td>
              </tr>
            )}
            {items.map((item) => (
              <tr key={item.des_guid}>
                <td>
                  {item.imagen_url ? (
                    <img
                      src={item.imagen_url}
                      alt={item.nombre}
                      style={{
                        width: 44, height: 44, objectFit: 'cover',
                        borderRadius: 6, border: '1px solid rgba(255,255,255,0.15)',
                      }}
                      onError={(e) => { e.currentTarget.style.display = 'none' }}
                    />
                  ) : <span className="text-muted text-sm">—</span>}
                </td>
                <td>{item.nombre}</td>
                <td>{item.pais || '—'}</td>
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
        titulo="¿Eliminar destino?"
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

export default GestionDestinosPage
