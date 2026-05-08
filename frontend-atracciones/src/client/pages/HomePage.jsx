import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import TarjetaAtraccion from '../components/atracciones/TarjetaAtraccion'
import ErrorMessage from '../../components/common/ErrorMessage'
import Spinner from '../../components/common/Spinner'
import { useHomeDestacadas } from '../hooks/useHomeDestacadas'

const HERO_IMAGE =
  'https://images.unsplash.com/photo-1476514525535-07fb3b4ae5f1?w=1600'

function HomePage() {
  const navigate = useNavigate()
  const [ciudad, setCiudad] = useState('')
  const { destacadas, cargando, error, cargarDestacadas } = useHomeDestacadas()

  useEffect(() => {
    cargarDestacadas()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  const buscarPorCiudad = (event) => {
    event.preventDefault()
    const query = ciudad.trim() ? `?ciudad=${encodeURIComponent(ciudad.trim())}` : ''
    navigate(`/atracciones${query}`)
  }

  return (
    <>
      <section
        className="hero-section"
        style={{
          backgroundImage: `linear-gradient(to top, rgba(0, 0, 0, 0.74), rgba(0, 0, 0, 0.35)), url(${HERO_IMAGE})`,
        }}
      >
        <div className="hero-overlay">
          <p className="eyebrow">Explora Ecuador</p>
          <h1>Visita y disfruta tu proxima aventura</h1>
          <p>Explora destinos, horarios y experiencias unicas en un solo lugar.</p>
          <form className="search-form" onSubmit={buscarPorCiudad}>
            <input
              type="text"
              placeholder="Buscar por ciudad"
              value={ciudad}
              onChange={(e) => setCiudad(e.target.value)}
            />
            <button type="submit" className="btn">
              Buscar
            </button>
          </form>
        </div>
      </section>

      <section className="page-section">
        <h2>Atracciones destacadas</h2>
        {cargando && <Spinner message="Cargando destacadas..." />}
        <ErrorMessage mensaje={error} />
        {!cargando && (
          <div className="atracciones-grid">
            {destacadas.map((atraccion) => (
              <TarjetaAtraccion
                key={atraccion.at_guid ?? atraccion.guid ?? atraccion.id}
                atraccion={atraccion}
              />
            ))}
          </div>
        )}
      </section>
    </>
  )
}

export default HomePage
