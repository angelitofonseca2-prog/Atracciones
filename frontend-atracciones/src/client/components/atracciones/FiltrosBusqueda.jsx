/**
 * Extrae array de items de filtros del response del backend.
 * El backend (snake_case) devuelve:
 *   destination_filters | type_filters | supported_language_filters
 */
function extraerCiudades(f) {
  return (
    f.destination_filters ??
    f.destinationFilters ??
    f.ciudades ??
    f.destinos ??
    f.cities ??
    []
  )
}

function extraerTipos(f) {
  return (
    f.type_filters ??
    f.typeFilters ??
    f.tipos ??
    f.categorias ??
    f.categories ??
    []
  )
}

function extraerIdiomas(f) {
  return (
    f.supported_language_filters ??
    f.supportedLanguageFilters ??
    f.idiomas ??
    f.languages ??
    []
  )
}

/** Normaliza un item de filtro usando tagname como value (tipos/categorías). */
function normalItem(item) {
  return {
    value: item.tagname ?? item.value ?? item.guid ?? item.id ?? item.nombre ?? item.name ?? '',
    label: item.name ?? item.nombre ?? item.label ?? item.tagname ?? '',
    children: item.childFilterOptions ?? item.subtipos ?? item.children ?? [],
  }
}

/**
 * Normaliza un item de filtro usando name como value.
 * Se usa para Ciudad e Idioma porque el backend espera el nombre legible
 * en los query params Ciudad= e Idioma= (no el tagname en minúscula).
 */
function normalItemByName(item) {
  return {
    value: item.name ?? item.nombre ?? item.tagname ?? item.value ?? '',
    label: item.name ?? item.nombre ?? item.label ?? item.tagname ?? '',
    children: item.childFilterOptions ?? item.subtipos ?? item.children ?? [],
  }
}

function FiltrosBusqueda({ filtrosDisponibles, filtrosActivos, onFiltroChange }) {
  const ciudades = extraerCiudades(filtrosDisponibles).map(normalItemByName)
  const tipos = extraerTipos(filtrosDisponibles).map(normalItem)
  const idiomas = extraerIdiomas(filtrosDisponibles).map(normalItemByName)

  const tipoSeleccionado = tipos.find((t) => t.value === filtrosActivos.tipo)
  const subtipos = (tipoSeleccionado?.children ?? []).map(normalItem)

  const set = (campo) => (e) =>
    onFiltroChange({ ...filtrosActivos, [campo]: e.target.value, page: 1 })

  const limpiar = () =>
    onFiltroChange({ page: 1, limit: filtrosActivos.limit || 8 })

  const hayFiltros =
    filtrosActivos.ciudad ||
    filtrosActivos.tipo ||
    filtrosActivos.subtipo ||
    filtrosActivos.idioma ||
    filtrosActivos.calificacion_min ||
    filtrosActivos.disponible

  return (
    <div className="filtros-bar">
      {/* Ciudad */}
      <div className="filtro-item">
        <label className="filtro-label" htmlFor="f-ciudad">Ciudad</label>
        <select
          id="f-ciudad"
          name="ciudad"
          value={filtrosActivos.ciudad || ''}
          onChange={set('ciudad')}
        >
          <option value="">Todas las ciudades</option>
          {ciudades.map((c) => (
            <option key={c.value} value={c.value}>{c.label || c.value}</option>
          ))}
        </select>
      </div>

      {/* Tipo */}
      <div className="filtro-item">
        <label className="filtro-label" htmlFor="f-tipo">Tipo</label>
        <select
          id="f-tipo"
          name="tipo"
          value={filtrosActivos.tipo || ''}
          onChange={(e) =>
            onFiltroChange({ ...filtrosActivos, tipo: e.target.value, subtipo: '', page: 1 })
          }
        >
          <option value="">Todos los tipos</option>
          {tipos.map((t) => (
            <option key={t.value} value={t.value}>{t.label || t.value}</option>
          ))}
        </select>
      </div>

      {/* Subtipo — solo si el tipo tiene hijos */}
      {subtipos.length > 0 && (
        <div className="filtro-item">
          <label className="filtro-label" htmlFor="f-subtipo">Subtipo</label>
          <select
            id="f-subtipo"
            name="subtipo"
            value={filtrosActivos.subtipo || ''}
            onChange={set('subtipo')}
          >
            <option value="">Todos</option>
            {subtipos.map((s) => (
              <option key={s.value} value={s.value}>{s.label || s.value}</option>
            ))}
          </select>
        </div>
      )}

      {/* Idioma */}
      <div className="filtro-item">
        <label className="filtro-label" htmlFor="f-idioma">Idioma</label>
        <select
          id="f-idioma"
          name="idioma"
          value={filtrosActivos.idioma || ''}
          onChange={set('idioma')}
        >
          <option value="">Todos</option>
          {idiomas.map((i) => (
            <option key={i.value} value={i.value}>{i.label || i.value}</option>
          ))}
        </select>
      </div>

      {/* Calificación */}
      <div className="filtro-item">
        <label className="filtro-label" htmlFor="f-cal">Calificación</label>
        <select
          id="f-cal"
          name="calificacion_min"
          value={filtrosActivos.calificacion_min ?? ''}
          onChange={set('calificacion_min')}
        >
          <option value="">Cualquiera</option>
          <option value="3">★★★ 3+</option>
          <option value="4">★★★★ 4+</option>
          <option value="5">★★★★★ 5</option>
        </select>
      </div>

      {/* Ordenar */}
      <div className="filtro-item">
        <label className="filtro-label" htmlFor="f-orden">Ordenar</label>
        <select
          id="f-orden"
          name="ordenar_por"
          value={filtrosActivos.ordenar_por || 'trending'}
          onChange={set('ordenar_por')}
        >
          <option value="trending">Relevancia</option>
          <option value="lowest_price">Menor precio</option>
          <option value="highest_weighted_rating">Mejor calificación</option>
        </select>
      </div>

      {/* Switch disponible */}
      <div className="filtro-item filtro-switch">
        <label className="filtro-label">Disponibles</label>
        <label className="toggle-row" htmlFor="f-disponible">
          <input
            id="f-disponible"
            type="checkbox"
            className="toggle-input"
            checked={Boolean(filtrosActivos.disponible)}
            onChange={(e) =>
              onFiltroChange({ ...filtrosActivos, disponible: e.target.checked, page: 1 })
            }
          />
          <span className="toggle-slider" />
          <span className="text-sm text-muted">Solo con cupos</span>
        </label>
      </div>

      {/* Limpiar */}
      {hayFiltros && (
        <div className="filtro-item filtro-limpiar">
          <label className="filtro-label">&nbsp;</label>
          <button type="button" className="btn btn-outline btn-sm" onClick={limpiar}>
            Limpiar filtros
          </button>
        </div>
      )}
    </div>
  )
}

export default FiltrosBusqueda
