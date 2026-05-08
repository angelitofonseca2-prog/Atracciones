import { useEffect, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import FiltrosBusqueda from '../components/atracciones/FiltrosBusqueda'
import TarjetaAtraccion from '../components/atracciones/TarjetaAtraccion'
import ErrorMessage from '../../components/common/ErrorMessage'
import Spinner from '../../components/common/Spinner'
import { useAtracciones } from '../hooks/useAtracciones'

function CatalogoPage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const [filtrosActivos, setFiltrosActivos] = useState({
    ciudad: searchParams.get('ciudad') || '',
    page: 1,
    limit: 8,
  })

  const {
    atracciones,
    filtrosDisponibles,
    paginacion,
    cargando,
    error,
    cambiarPagina,
  } = useAtracciones(filtrosActivos)

  useEffect(() => {
    const ciudad = filtrosActivos.ciudad ? filtrosActivos.ciudad : ''
    if (ciudad) {
      setSearchParams({ ciudad })
    } else {
      setSearchParams({})
    }
  }, [filtrosActivos.ciudad, setSearchParams])

  const handlePage = (page) => {
    cambiarPagina(page)
    setFiltrosActivos((prev) => ({ ...prev, page }))
  }

  return (
    <section className="page-section">
      <h1>Catalogo de atracciones</h1>
      <FiltrosBusqueda
        filtrosDisponibles={filtrosDisponibles}
        filtrosActivos={filtrosActivos}
        onFiltroChange={setFiltrosActivos}
      />

      {cargando && <Spinner message="Cargando catalogo..." />}
      <ErrorMessage mensaje={error} />

      <div className="atracciones-grid">
        {atracciones.map((atraccion) => (
          <TarjetaAtraccion
            key={atraccion.at_guid ?? atraccion.guid ?? atraccion.id}
            atraccion={atraccion}
          />
        ))}
      </div>
      {!cargando && !error && atracciones.length === 0 && (
        <div style={{ textAlign: 'center', marginTop: '2rem' }}>
          <p>
            {Object.values(filtrosActivos).some(Boolean)
              ? 'No encontramos atracciones con esos filtros. Prueba ajustando la búsqueda.'
              : 'No se encontraron atracciones para la ciudad buscada.'}
          </p>
          <button
            type="button"
            className="btn btn-outline"
            style={{ marginTop: '0.75rem' }}
            onClick={() => setFiltrosActivos({ page: 1, limit: 8 })}
          >
            Limpiar filtros
          </button>
        </div>
      )}

      <div className="pagination">
        <button
          type="button"
          className="btn btn-outline"
          onClick={() => handlePage(Math.max(1, filtrosActivos.page - 1))}
          disabled={filtrosActivos.page <= 1}
        >
          Anterior
        </button>
        <span>
          Pagina {paginacion.page} de {paginacion.totalPages}
        </span>
        <button
          type="button"
          className="btn btn-outline"
          onClick={() =>
            handlePage(Math.min(paginacion.totalPages, filtrosActivos.page + 1))
          }
          disabled={filtrosActivos.page >= paginacion.totalPages}
        >
          Siguiente
        </button>
      </div>
    </section>
  )
}

export default CatalogoPage
