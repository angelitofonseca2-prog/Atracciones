import { useNavigate } from 'react-router-dom'

const FALLBACK_IMAGE = 'https://placehold.co/400x300/0a1b30/55d2ff?text=Sin+imagen'

function Stars({ valor }) {
  const llenas = Math.round(Number(valor) || 0)
  return (
    <span className="stars" aria-label={`${llenas} de 5 estrellas`}>
      {'★'.repeat(llenas)}{'☆'.repeat(5 - llenas)}
    </span>
  )
}

function TarjetaAtraccion({ atraccion }) {
  const navigate = useNavigate()
  const guid = atraccion?.at_guid ?? atraccion?.guid ?? atraccion?.id
  const imagen = atraccion?.imagen_principal || FALLBACK_IMAGE
  const precio = atraccion?.precio_desde ?? atraccion?.precio ?? 0
  const calificacion = Number(atraccion?.calificacion ?? 0).toFixed(1)
  const resenas = atraccion?.total_resenas ?? atraccion?.total_resenias ?? 0

  return (
    <article className="atraccion-card fade-in">
      <div className="atraccion-card-img">
        <img
          src={imagen}
          alt={atraccion?.nombre || 'Atracción'}
          onError={(e) => { e.currentTarget.src = FALLBACK_IMAGE }}
        />
        {precio > 0 && (
          <div className="atraccion-badge">Desde ${Number(precio).toFixed(2)}</div>
        )}
      </div>

      <div className="atraccion-info">
        <h3>{atraccion?.nombre}</h3>
        {atraccion?.ciudad && (
          <p className="atraccion-ciudad">📍 {atraccion.ciudad}</p>
        )}

        <div className="atraccion-rating">
          <Stars valor={calificacion} />
          <span>{calificacion}</span>
          {resenas > 0 && <span>({resenas} reseñas)</span>}
        </div>

        {(atraccion?.idiomas_disponibles || []).length > 0 && (
          <p className="text-sm text-muted">
            🌐 {atraccion.idiomas_disponibles.join(', ')}
          </p>
        )}

        <button
          type="button"
          className="btn"
          onClick={() => navigate(`/atracciones/${guid}`)}
          disabled={!guid}
        >
          Ver detalle
        </button>
      </div>
    </article>
  )
}

export default TarjetaAtraccion
