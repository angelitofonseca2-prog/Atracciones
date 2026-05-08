import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import * as reseniasApi from '../../api/reseniasApi'
import ErrorMessage from '../../components/common/ErrorMessage'
import Spinner from '../../components/common/Spinner'
import { useAuthContext } from '../../context/AuthContext'
import { useAtracciones } from '../hooks/useAtracciones'

const FALLBACK_IMAGE = 'https://placehold.co/400x300?text=Sin+imagen'

function Estrellas({ rating }) {
  return (
    <span>
      {[1, 2, 3, 4, 5].map((n) => (
        <span key={n} style={{ color: n <= rating ? '#f5c518' : 'rgba(255,255,255,0.3)' }}>
          ★
        </span>
      ))}
    </span>
  )
}

function FormReseniaDetalle({ atraccionGuid, onEnviada }) {
  const [rating, setRating] = useState(5)
  const [comentario, setComentario] = useState('')
  const [cargando, setCargando] = useState(false)
  const [error, setError] = useState('')

  const handleEnviar = async () => {
    setCargando(true)
    setError('')
    try {
      await reseniasApi.crearResenia({ at_guid: atraccionGuid, rating, comentario })
      onEnviada()
    } catch (err) {
      if (err?.response?.status === 409) {
        setError('Ya registraste una reseña para esta atracción.')
      } else {
        setError(err?.response?.data?.message || err?.response?.data?.details?.[0] || 'No se pudo enviar la reseña.')
      }
    } finally {
      setCargando(false)
    }
  }

  return (
    <div className="reserva-card" style={{ marginTop: '1rem' }}>
      <h4>Escribir reseña</h4>
      <div className="inline-form">
        {[1, 2, 3, 4, 5].map((n) => (
          <button
            key={n}
            type="button"
            onClick={() => setRating(n)}
            style={{
              background: 'none',
              border: 'none',
              cursor: 'pointer',
              fontSize: '1.5rem',
              color: n <= rating ? '#f5c518' : 'rgba(255,255,255,0.3)',
              padding: '0',
            }}
          >
            ★
          </button>
        ))}
        <span>{rating}/5</span>
      </div>
      <textarea
        value={comentario}
        onChange={(e) => setComentario(e.target.value)}
        rows={3}
        style={{ width: '100%', marginTop: '0.5rem' }}
        placeholder="Comparte tu experiencia..."
      />
      <ErrorMessage mensaje={error} />
      <button
        className="btn"
        onClick={handleEnviar}
        disabled={cargando}
        style={{ marginTop: '0.75rem' }}
      >
        {cargando ? 'Enviando...' : 'Publicar reseña'}
      </button>
    </div>
  )
}

function DetallePage() {
  const { guid } = useParams()
  const navigate = useNavigate()
  const { estaAutenticado, usuario } = useAuthContext()
  const { detalle, cargarDetalle, cargando, error } = useAtracciones({})
  const [mostrarFormResenia, setMostrarFormResenia] = useState(false)

  const esCliente = estaAutenticado && !usuario?.roles?.includes('ADMIN')

  useEffect(() => {
    cargarDetalle(guid).catch(() => {})
  }, [guid, cargarDetalle])

  if (cargando && !detalle && !error) return <Spinner message="Cargando detalle..." />

  return (
    <section className="page-section">
      <ErrorMessage mensaje={error} />
      {detalle && (
        <>
          <h1>{detalle.nombre}</h1>
          <div className="detalle-grid">
            <div className="gallery">
              {(detalle.imagenes?.length ? detalle.imagenes : [FALLBACK_IMAGE]).map(
                (img, idx) => (
                  <img
                    key={idx}
                    src={img || FALLBACK_IMAGE}
                    alt={`${detalle.nombre} ${idx + 1}`}
                  />
                ),
              )}
            </div>

            <div className="detalle-content">
              <p>
                {detalle.ciudad}, {detalle.pais}
              </p>
              <p>{detalle.descripcion}</p>
              <p>
                <strong>Incluye:</strong> {(detalle.incluye || []).join(', ') || 'N/D'}
              </p>
              <p>
                <strong>No incluye:</strong>{' '}
                {(detalle.no_incluye || []).join(', ') || 'N/D'}
              </p>
              <p>
                <strong>Idiomas:</strong>{' '}
                {(detalle.idiomas_disponibles || []).join(', ') || 'N/D'}
              </p>

              <h3>Tickets</h3>
              <ul>
                {(detalle.tickets || []).map((ticket) => (
                  <li key={ticket.tck_guid}>
                    {ticket.titulo} — ${Number(ticket.precio).toFixed(2)}
                  </li>
                ))}
              </ul>

              <h3>Horarios próximos</h3>
              {(detalle.horarios_proximos || []).length === 0 ? (
                <p className="text-muted">
                  No hay horarios disponibles en los próximos 7 días.
                </p>
              ) : (
                <ul>
                  {detalle.horarios_proximos.map((horario, index) => (
                    <li key={`${horario.fecha}-${horario.hora_inicio}-${index}`}>
                      {horario.fecha} {horario.hora_inicio}
                      {horario.hora_fin ? `–${horario.hora_fin}` : ''}{' '}
                      {horario.cupos != null ? `(${horario.cupos} cupos)` : ''}
                    </li>
                  ))}
                </ul>
              )}

              <h3>Reseñas</h3>
              {(detalle.resenias || detalle.resenas || []).length === 0 ? (
                <p className="text-muted">Aún no hay reseñas para esta atracción.</p>
              ) : (
                <ul>
                  {(detalle.resenias || detalle.resenas || []).map((resena) => (
                    <li
                      key={resena.rsn_guid || resena.fecha_creacion}
                      style={{ marginBottom: '0.75rem' }}
                    >
                      <Estrellas rating={resena.rating} />
                      <span style={{ marginLeft: '0.5rem', color: '#9ddcff' }}>
                        {resena.fecha_creacion?.slice(0, 10)}
                      </span>
                      <p style={{ margin: '0.25rem 0 0' }}>{resena.comentario}</p>
                    </li>
                  ))}
                </ul>
              )}

              {esCliente && (
                <>
                  {mostrarFormResenia ? (
                    <FormReseniaDetalle
                      atraccionGuid={guid}
                      onEnviada={() => {
                        setMostrarFormResenia(false)
                        cargarDetalle(guid).catch(() => {})
                      }}
                    />
                  ) : (
                    <button
                      type="button"
                      className="btn btn-outline"
                      style={{ marginTop: '0.5rem' }}
                      onClick={() => setMostrarFormResenia(true)}
                    >
                      Escribir reseña
                    </button>
                  )}
                </>
              )}

              <button
                type="button"
                className="btn"
                style={{ marginTop: '1rem' }}
                onClick={() => navigate(`/reservar/${guid}`)}
              >
                Reservar
              </button>
            </div>
          </div>
        </>
      )}
    </section>
  )
}

export default DetallePage
