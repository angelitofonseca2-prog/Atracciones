import { useEffect, useState } from 'react'
import { adminApi } from '../../api/adminApi'
import { imagenesApi } from '../../api/imagenesApi'
import Spinner from '../../components/common/Spinner'

/**
 * Formulario de creación/edición de atracciones administrativas.
 *
 * Contrato:
 *  - POST /admin/atracciones (CrearAtraccionRequest):
 *      { destino_guid, num_establecimiento?, nombre, descripcion?, direccion?,
 *        duracion_minutos?, punto_encuentro?, precio_referencia?,
 *        categoria_guids[], idioma_guids[], imagen_guids[], incluye_guids[] }
 *      categoria_guids/idioma_guids/imagen_guids/incluye_guids son requeridos (MinLength 1).
 *
 *  - PUT/PATCH /admin/atracciones/{guid} (ActualizarAtraccionRequest):
 *      todos los campos son opcionales; además acepta `disponible: boolean`.
 *
 * Imágenes (CASO A — backend expone /admin/imagenes):
 *  El admin puede:
 *    1) Seleccionar imágenes ya existentes (chips con `img_guid`).
 *    2) Agregar nuevas URLs: en el submit se hace POST /admin/imagenes por
 *       cada URL pendiente y los `img_guid` resultantes se concatenan al
 *       array `imagen_guids` enviado en el body de la atracción.
 */

// ── Helpers de catálogos ──────────────────────────────────────────────────
const getDesGuid = (item) => item.des_guid ?? item.guid ?? ''
const getDesNombre = (item) => {
  const partes = [item.nombre]
  if (item.pais) partes.push(item.pais)
  return partes.filter(Boolean).join(' — ')
}
const getCatGuid = (item) => item.cat_guid ?? item.guid ?? ''
const getCatNombre = (item) => item.nombre ?? item.descripcion ?? ''
const getIdiGuid = (item) => item.id_guid ?? item.guid ?? ''
const getIdiNombre = (item) => item.descripcion ?? item.nombre ?? ''
const getIncGuid = (item) => item.incluye_guid ?? item.guid ?? ''
const getIncNombre = (item) => item.descripcion ?? item.nombre ?? ''
const getImgGuid = (item) => item.img_guid ?? item.guid ?? ''

const toggleGuid = (array, guid) =>
  array.includes(guid) ? array.filter((g) => g !== guid) : [...array, guid]

// ── Subcomponente: grupo de chips ─────────────────────────────────────────
function GrupoChips({ titulo, subtitulo, items, seleccionados, onChange, getId, getLabel, vacio }) {
  return (
    <div style={{ gridColumn: '1 / -1' }}>
      <p style={{ margin: '0 0 0.2rem', fontWeight: 700, fontSize: '0.9rem' }}>{titulo}</p>
      {subtitulo && (
        <p style={{ margin: '0 0 0.5rem', fontSize: '0.8rem', color: 'var(--text-muted)' }}>
          {subtitulo}
        </p>
      )}
      {items.length === 0 ? (
        <p style={{ fontSize: '0.85rem', color: 'var(--text-muted)' }}>{vacio}</p>
      ) : (
        <div className="checkbox-grid">
          {items.map((item) => {
            const id = getId(item)
            if (!id) return null
            const activo = seleccionados.includes(id)
            return (
              <label key={id} className={`checkbox-chip${activo ? ' selected' : ''}`}>
                <input
                  type="checkbox"
                  checked={activo}
                  onChange={() => onChange(toggleGuid(seleccionados, id))}
                />
                {activo ? '✓ ' : ''}{getLabel(item)}
              </label>
            )
          })}
        </div>
      )}
    </div>
  )
}

// ── Subcomponente: gestor de imágenes ─────────────────────────────────────
function GestorImagenes({ existentes, seleccionadas, onChangeSeleccionadas, nuevas, onChangeNuevas, errorImagenes }) {
  const [urlNueva, setUrlNueva] = useState('')
  const [descNueva, setDescNueva] = useState('')
  const [errorUrl, setErrorUrl] = useState('')

  const validarUrl = (u) => {
    try {
      // eslint-disable-next-line no-new
      new URL(u)
      return true
    } catch {
      return false
    }
  }

  const agregar = () => {
    setErrorUrl('')
    const url = urlNueva.trim()
    if (!url) { setErrorUrl('Ingresa una URL'); return }
    if (!validarUrl(url)) { setErrorUrl('URL no válida (incluye https://)'); return }
    if (nuevas.some((x) => x.url === url)) { setErrorUrl('Esa URL ya está agregada'); return }
    onChangeNuevas([...nuevas, { url, descripcion: descNueva.trim() || undefined }])
    setUrlNueva('')
    setDescNueva('')
  }

  const quitarNueva = (url) => {
    onChangeNuevas(nuevas.filter((x) => x.url !== url))
  }

  return (
    <div style={{ gridColumn: '1 / -1' }}>
      <p style={{ margin: '0 0 0.2rem', fontWeight: 700, fontSize: '0.9rem' }}>Imágenes *</p>
      <p style={{ margin: '0 0 0.5rem', fontSize: '0.8rem', color: 'var(--text-muted)' }}>
        Selecciona imágenes ya registradas o agrega nuevas URLs. Se requiere al menos una.
      </p>

      {/* Existentes */}
      {existentes.length === 0 ? (
        <p style={{ fontSize: '0.85rem', color: 'var(--text-muted)' }}>
          Aún no hay imágenes registradas en el catálogo.
        </p>
      ) : (
        <div className="checkbox-grid" style={{ marginBottom: '0.75rem' }}>
          {existentes.map((img) => {
            const id = getImgGuid(img)
            const activo = seleccionadas.includes(id)
            return (
              <label key={id} className={`checkbox-chip${activo ? ' selected' : ''}`} style={{ alignItems: 'center', gap: '0.5rem' }}>
                <input
                  type="checkbox"
                  checked={activo}
                  onChange={() => onChangeSeleccionadas(toggleGuid(seleccionadas, id))}
                />
                {img.url ? (
                  <img
                    src={img.url}
                    alt={img.descripcion || 'imagen'}
                    style={{
                      width: 36, height: 36, objectFit: 'cover',
                      borderRadius: 4, border: '1px solid rgba(255,255,255,0.15)',
                    }}
                    onError={(e) => { e.currentTarget.style.display = 'none' }}
                  />
                ) : null}
                <span style={{ fontSize: '0.8rem' }}>
                  {img.descripcion || (img.url ? new URL(img.url).hostname : id.slice(0, 8))}
                </span>
              </label>
            )
          })}
        </div>
      )}

      {/* Nuevas pendientes */}
      {nuevas.length > 0 && (
        <div style={{ marginBottom: '0.75rem' }}>
          <p style={{ fontSize: '0.8rem', color: 'var(--text-muted)', margin: '0 0 0.4rem' }}>
            Nuevas imágenes a registrar al guardar:
          </p>
          <div className="checkbox-grid">
            {nuevas.map((img) => (
              <span key={img.url} className="checkbox-chip selected" style={{ gap: '0.4rem' }}>
                <span style={{ fontSize: '0.75rem' }}>
                  {img.descripcion ? `${img.descripcion} — ` : ''}{img.url}
                </span>
                <button
                  type="button"
                  onClick={() => quitarNueva(img.url)}
                  style={{
                    background: 'none', border: 'none', cursor: 'pointer',
                    color: 'inherit', padding: '0 0.25rem', fontSize: '0.95rem',
                  }}
                  aria-label="Quitar imagen"
                >
                  ×
                </button>
              </span>
            ))}
          </div>
        </div>
      )}

      {/* Agregar nueva */}
      <div className="form-grid form-grid-2" style={{ alignItems: 'end' }}>
        <div className="form-group" style={{ marginBottom: 0 }}>
          <label htmlFor="fa-img-url">Nueva URL de imagen</label>
          <input
            id="fa-img-url"
            type="url"
            value={urlNueva}
            onChange={(e) => { setUrlNueva(e.target.value); if (errorUrl) setErrorUrl('') }}
            placeholder="https://res.cloudinary.com/..."
            className={errorUrl ? 'input-error' : ''}
          />
          {errorUrl && <span className="field-error">⚠ {errorUrl}</span>}
        </div>
        <div className="form-group" style={{ marginBottom: 0 }}>
          <label htmlFor="fa-img-desc">Descripción (opcional)</label>
          <div className="inline-form">
            <input
              id="fa-img-desc"
              type="text"
              value={descNueva}
              onChange={(e) => setDescNueva(e.target.value)}
              placeholder="Galería principal"
              style={{ flex: 1 }}
            />
            <button type="button" className="btn btn-outline btn-sm" onClick={agregar}>
              Agregar
            </button>
          </div>
        </div>
      </div>

      {errorImagenes && <span className="field-error">⚠ {errorImagenes}</span>}
    </div>
  )
}

// ── Formulario principal ──────────────────────────────────────────────────
function FormularioAtraccion({ inicial, onGuardar, onCancelar }) {
  const [catalogos, setCatalogos] = useState({
    destinos: [],
    categorias: [],
    idiomas: [],
    incluye: [],
    imagenes: [],
  })
  const [cargandoCatalogos, setCargandoCatalogos] = useState(true)
  const [errorCatalogos, setErrorCatalogos] = useState('')
  const [guardando, setGuardando] = useState(false)
  const [errores, setErrores] = useState({})

  const [form, setForm] = useState({
    destino_guid: '',
    num_establecimiento: '',
    nombre: '',
    descripcion: '',
    direccion: '',
    duracion_minutos: '',
    punto_encuentro: '',
    precio_referencia: '',
    categoria_guids: [],
    idioma_guids: [],
    incluye_guids: [],
    imagen_guids_existentes: [],
    imagenes_nuevas: [], // [{ url, descripcion? }]
    disponible: true,
  })

  const set = (campo) => (e) => {
    setForm((p) => ({ ...p, [campo]: e.target.value }))
    if (errores[campo]) setErrores((p) => ({ ...p, [campo]: '' }))
  }

  const cargarCatalogos = () => {
    setCargandoCatalogos(true)
    setErrorCatalogos('')
    Promise.all([
      adminApi.listDestinos(),
      adminApi.listarCategorias(),
      adminApi.listarIdiomas(),
      adminApi.listarIncluye(),
      imagenesApi.listar().catch(() => []),
    ])
      .then(([destinos, categorias, idiomas, incluye, imagenes]) => {
        setCatalogos({ destinos, categorias, idiomas, incluye, imagenes })
      })
      .catch((err) => {
        setErrorCatalogos(
          err?.response?.data?.message || 'No se pudieron cargar los catálogos. Reintenta.',
        )
      })
      .finally(() => setCargandoCatalogos(false))
  }

  useEffect(() => {
    cargarCatalogos()
  }, [])

  // Precarga al editar — todas las claves vienen en snake_case del contrato.
  useEffect(() => {
    if (!inicial) return
    setForm((prev) => ({
      ...prev,
      destino_guid: inicial.destino_guid ?? '',
      num_establecimiento: inicial.num_establecimiento ?? '',
      nombre: inicial.nombre ?? '',
      descripcion: inicial.descripcion ?? '',
      direccion: inicial.direccion ?? '',
      duracion_minutos: inicial.duracion_minutos ?? '',
      punto_encuentro: inicial.punto_encuentro ?? '',
      precio_referencia: inicial.precio_referencia ?? '',
      categoria_guids: Array.isArray(inicial.categoria_guids) ? inicial.categoria_guids : [],
      idioma_guids: Array.isArray(inicial.idioma_guids) ? inicial.idioma_guids : [],
      incluye_guids: Array.isArray(inicial.incluye_guids) ? inicial.incluye_guids : [],
      imagen_guids_existentes: Array.isArray(inicial.imagen_guids) ? inicial.imagen_guids : [],
      imagenes_nuevas: [],
      disponible: inicial.disponible ?? true,
    }))
  }, [inicial])

  const validar = () => {
    const e = {}
    if (!form.destino_guid) e.destino_guid = 'Selecciona un destino'
    if (!form.nombre.trim()) e.nombre = 'El nombre es obligatorio'
    if (form.duracion_minutos && Number(form.duracion_minutos) <= 0) e.duracion_minutos = 'Debe ser mayor a 0'
    if (form.precio_referencia && Number(form.precio_referencia) < 0) e.precio_referencia = 'Debe ser positivo'
    if (form.categoria_guids.length === 0) e.categoria_guids = 'Selecciona al menos una categoría'
    if (form.idioma_guids.length === 0) e.idioma_guids = 'Selecciona al menos un idioma'
    if (form.incluye_guids.length === 0) e.incluye_guids = 'Selecciona al menos un elemento incluido'
    const totalImagenes = form.imagen_guids_existentes.length + form.imagenes_nuevas.length
    if (totalImagenes === 0) e.imagenes = 'Agrega al menos una imagen (existente o nueva)'
    return e
  }

  const submit = async (event) => {
    event.preventDefault()
    const e = validar()
    if (Object.keys(e).length) { setErrores(e); return }

    setGuardando(true)
    try {
      // 1) Crear las imágenes nuevas, si hay, antes de la atracción.
      const guidsCreados = []
      for (const img of form.imagenes_nuevas) {
        const creada = await imagenesApi.crear({
          url: img.url,
          descripcion: img.descripcion,
        })
        const guid = getImgGuid(creada)
        if (!guid) throw new Error('La API no devolvió un img_guid válido.')
        guidsCreados.push(guid)
      }

      // 2) Construir el body final con snake_case del contrato.
      const payload = {
        destino_guid: form.destino_guid,
        nombre: form.nombre.trim(),
        categoria_guids: form.categoria_guids,
        idioma_guids: form.idioma_guids,
        incluye_guids: form.incluye_guids,
        imagen_guids: [...form.imagen_guids_existentes, ...guidsCreados],
      }
      if (form.num_establecimiento.trim()) payload.num_establecimiento = form.num_establecimiento.trim()
      if (form.descripcion.trim()) payload.descripcion = form.descripcion.trim()
      if (form.direccion.trim()) payload.direccion = form.direccion.trim()
      if (form.duracion_minutos !== '' && form.duracion_minutos != null) {
        payload.duracion_minutos = Number(form.duracion_minutos)
      }
      if (form.punto_encuentro.trim()) payload.punto_encuentro = form.punto_encuentro.trim()
      if (form.precio_referencia !== '' && form.precio_referencia != null) {
        payload.precio_referencia = Number(form.precio_referencia)
      }
      if (inicial) payload.disponible = Boolean(form.disponible)

      await onGuardar(payload)
    } catch (err) {
      const mensaje =
        err?.response?.data?.message ||
        err?.response?.data?.details?.[0] ||
        err?.message ||
        'No se pudo guardar la atracción.'
      setErrores((p) => ({ ...p, _global: mensaje }))
    } finally {
      setGuardando(false)
    }
  }

  if (cargandoCatalogos) {
    return <Spinner message="Cargando catálogos..." />
  }

  if (errorCatalogos) {
    return (
      <div>
        <div className="error-message" style={{ marginBottom: '1rem' }}>
          <span>⚠</span><span>{errorCatalogos}</span>
        </div>
        <div className="inline-form">
          <button className="btn btn-outline" type="button" onClick={cargarCatalogos}>Reintentar</button>
          <button className="btn btn-outline" type="button" onClick={onCancelar}>Cancelar</button>
        </div>
      </div>
    )
  }

  return (
    <form className="admin-form two-columns" onSubmit={submit} noValidate>

      {/* Destino */}
      <div className="form-group">
        <label htmlFor="fa-destino">Destino *</label>
        <select
          id="fa-destino"
          value={form.destino_guid}
          onChange={set('destino_guid')}
          className={errores.destino_guid ? 'input-error' : ''}
        >
          <option value="">Selecciona un destino</option>
          {catalogos.destinos.map((d) => {
            const guid = getDesGuid(d)
            return <option key={guid} value={guid}>{getDesNombre(d)}</option>
          })}
        </select>
        {errores.destino_guid && <span className="field-error">⚠ {errores.destino_guid}</span>}
        {catalogos.destinos.length === 0 && (
          <span className="field-error" style={{ color: 'var(--text-muted)' }}>
            No hay destinos. Crea uno desde Gestión → Destinos.
          </span>
        )}
      </div>

      {/* Nombre */}
      <div className="form-group">
        <label htmlFor="fa-nombre">Nombre *</label>
        <input
          id="fa-nombre"
          type="text"
          value={form.nombre}
          onChange={set('nombre')}
          placeholder="Nombre de la atracción"
          maxLength={200}
          className={errores.nombre ? 'input-error' : ''}
        />
        {errores.nombre && <span className="field-error">⚠ {errores.nombre}</span>}
      </div>

      {/* Núm. establecimiento */}
      <div className="form-group">
        <label htmlFor="fa-num">Núm. de establecimiento <span className="text-muted">(opcional)</span></label>
        <input
          id="fa-num"
          type="text"
          value={form.num_establecimiento}
          onChange={set('num_establecimiento')}
          placeholder="ej. EST-001"
          maxLength={30}
        />
      </div>

      {/* Disponible (solo edición) */}
      {inicial && (
        <div className="form-group">
          <label htmlFor="fa-disp">Disponible</label>
          <select
            id="fa-disp"
            value={String(form.disponible)}
            onChange={(e) => setForm((p) => ({ ...p, disponible: e.target.value === 'true' }))}
          >
            <option value="true">Sí</option>
            <option value="false">No</option>
          </select>
        </div>
      )}

      {/* Descripción */}
      <div className="form-group" style={{ gridColumn: '1 / -1' }}>
        <label htmlFor="fa-desc">Descripción</label>
        <textarea
          id="fa-desc"
          value={form.descripcion}
          onChange={set('descripcion')}
          placeholder="Describe la experiencia..."
          rows={3}
          maxLength={2000}
        />
      </div>

      {/* Dirección */}
      <div className="form-group">
        <label htmlFor="fa-dir">Dirección</label>
        <input id="fa-dir" type="text" value={form.direccion} onChange={set('direccion')} placeholder="Calle y número" maxLength={300} />
      </div>

      {/* Duración */}
      <div className="form-group">
        <label htmlFor="fa-dur">Duración (minutos)</label>
        <input
          id="fa-dur"
          type="number"
          min="1"
          value={form.duracion_minutos}
          onChange={set('duracion_minutos')}
          placeholder="ej. 120"
          className={errores.duracion_minutos ? 'input-error' : ''}
        />
        {errores.duracion_minutos && <span className="field-error">⚠ {errores.duracion_minutos}</span>}
      </div>

      {/* Punto de encuentro */}
      <div className="form-group">
        <label htmlFor="fa-pe">Punto de encuentro</label>
        <input id="fa-pe" type="text" value={form.punto_encuentro} onChange={set('punto_encuentro')} placeholder="Dónde se reúnen los participantes" maxLength={300} />
      </div>

      {/* Precio de referencia */}
      <div className="form-group">
        <label htmlFor="fa-precio">Precio de referencia ($)</label>
        <input
          id="fa-precio"
          type="number"
          min="0"
          step="0.01"
          value={form.precio_referencia}
          onChange={set('precio_referencia')}
          placeholder="0.00"
          className={errores.precio_referencia ? 'input-error' : ''}
        />
        {errores.precio_referencia && <span className="field-error">⚠ {errores.precio_referencia}</span>}
      </div>

      {/* Imágenes (gestor real CASO A) */}
      <GestorImagenes
        existentes={catalogos.imagenes}
        seleccionadas={form.imagen_guids_existentes}
        onChangeSeleccionadas={(guids) => {
          setForm((p) => ({ ...p, imagen_guids_existentes: guids }))
          if (errores.imagenes) setErrores((p) => ({ ...p, imagenes: '' }))
        }}
        nuevas={form.imagenes_nuevas}
        onChangeNuevas={(items) => {
          setForm((p) => ({ ...p, imagenes_nuevas: items }))
          if (errores.imagenes) setErrores((p) => ({ ...p, imagenes: '' }))
        }}
        errorImagenes={errores.imagenes}
      />

      {/* Categorías */}
      <GrupoChips
        titulo="Categorías *"
        subtitulo="Selecciona al menos una categoría aplicable."
        items={catalogos.categorias}
        seleccionados={form.categoria_guids}
        onChange={(guids) => {
          setForm((p) => ({ ...p, categoria_guids: guids }))
          if (errores.categoria_guids) setErrores((p) => ({ ...p, categoria_guids: '' }))
        }}
        getId={getCatGuid}
        getLabel={getCatNombre}
        vacio="No hay categorías. Crea una desde Gestión → Categorías."
      />
      {errores.categoria_guids && (
        <span className="field-error" style={{ gridColumn: '1 / -1' }}>⚠ {errores.categoria_guids}</span>
      )}

      {/* Idiomas */}
      <GrupoChips
        titulo="Idiomas disponibles *"
        subtitulo="Selecciona al menos un idioma."
        items={catalogos.idiomas}
        seleccionados={form.idioma_guids}
        onChange={(guids) => {
          setForm((p) => ({ ...p, idioma_guids: guids }))
          if (errores.idioma_guids) setErrores((p) => ({ ...p, idioma_guids: '' }))
        }}
        getId={getIdiGuid}
        getLabel={getIdiNombre}
        vacio="No hay idiomas. Crea uno desde Gestión → Idiomas."
      />
      {errores.idioma_guids && (
        <span className="field-error" style={{ gridColumn: '1 / -1' }}>⚠ {errores.idioma_guids}</span>
      )}

      {/* Elementos incluidos */}
      <GrupoChips
        titulo="Elementos incluidos *"
        subtitulo="Selecciona al menos un elemento."
        items={catalogos.incluye}
        seleccionados={form.incluye_guids}
        onChange={(guids) => {
          setForm((p) => ({ ...p, incluye_guids: guids }))
          if (errores.incluye_guids) setErrores((p) => ({ ...p, incluye_guids: '' }))
        }}
        getId={getIncGuid}
        getLabel={getIncNombre}
        vacio="No hay elementos. Crea uno desde Gestión → Incluye."
      />
      {errores.incluye_guids && (
        <span className="field-error" style={{ gridColumn: '1 / -1' }}>⚠ {errores.incluye_guids}</span>
      )}

      {/* Error global */}
      {errores._global && (
        <div className="error-message" style={{ gridColumn: '1 / -1' }}>
          <span>⚠</span><span>{errores._global}</span>
        </div>
      )}

      {/* Acciones */}
      <div className="inline-form" style={{ gridColumn: '1 / -1', marginTop: '0.5rem' }}>
        <button className="btn" type="submit" disabled={guardando}>
          {guardando ? <><span className="spinner spinner-sm" /> Guardando...</> : 'Guardar'}
        </button>
        <button className="btn btn-outline" type="button" onClick={onCancelar} disabled={guardando}>
          Cancelar
        </button>
      </div>
    </form>
  )
}

export default FormularioAtraccion
