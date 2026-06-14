import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { listarResenias } from '../../api/reseniasApi'
import ErrorMessage from '../../components/common/ErrorMessage'
import Spinner from '../../components/common/Spinner'
import { useAuthContext } from '../../context/AuthContext'
import { formatearRangoFechas } from '../../utils/formatFechas'
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

function DetallePage() {
  const { guid } = useParams()
  const navigate = useNavigate()
  const { estaAutenticado, usuario } = useAuthContext()
  const { detalle, cargarDetalle, cargando, error } = useAtracciones({})
  const [resenias, setResenias] = useState([])
  const [cargandoResenias, setCargandoResenias] = useState(false)
  const esCliente = estaAutenticado && !usuario?.roles?.includes('ADMIN')

  useEffect(() => {
    cargarDetalle(guid).catch(() => {})
  }, [guid, cargarDetalle])

  useEffect(() => {
    if (!guid) return
    setCargandoResenias(true)
    listarResenias(guid, { page: 1, page_size: 20 })
      .then((payload) => {
        const items = payload?.data ?? (Array.isArray(payload) ? payload : [])
        setResenias(Array.isArray(items) ? items : [])
      })
      .catch(() => setResenias([]))
      .finally(() => setCargandoResenias(false))
  }, [guid])

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
                    <li key={`${horario.hor_guid || horario.fecha}-${horario.hora_inicio}-${index}`}>
                      {formatearRangoFechas(horario.fecha, horario.fecha_fin)} · {horario.hora_inicio}
                      {horario.hora_fin ? `–${horario.hora_fin}` : ''}{' '}
                      {horario.cupos != null ? `(${horario.cupos} cupos)` : ''}
                    </li>
                  ))}
                </ul>
              )}

              <h3>Reseñas</h3>
              {cargandoResenias && <p className="text-muted">Cargando reseñas...</p>}
              {!cargandoResenias && resenias.length === 0 ? (
                <p className="text-muted">Aún no hay reseñas para esta atracción.</p>
              ) : (
                <ul>
                  {resenias.map((resena) => (
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
                <p className="text-muted text-sm" style={{ marginTop: '0.5rem' }}>
                  Puedes dejar una reseña desde <strong>Mis reservas</strong> después de completar una compra confirmada.
                </p>
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
