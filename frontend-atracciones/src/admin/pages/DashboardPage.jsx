import { Link } from 'react-router-dom'

const SECCIONES = [
  {
    to: '/admin/atracciones',
    icon: '🏔️',
    titulo: 'Atracciones',
    descripcion: 'Gestiona el catálogo de atracciones',
  },
  {
    to: '/admin/tickets',
    icon: '🎟️',
    titulo: 'Tickets',
    descripcion: 'Tipos y precios de tickets',
  },
  {
    to: '/admin/horarios',
    icon: '🗓️',
    titulo: 'Horarios',
    descripcion: 'Programa fechas y cupos',
  },
  {
    to: '/admin/reservas',
    icon: '📋',
    titulo: 'Reservas',
    descripcion: 'Revisa y gestiona reservas',
  },
  {
    to: '/admin/usuarios',
    icon: '👥',
    titulo: 'Usuarios',
    descripcion: 'Gestiona clientes y cuentas',
  },
  {
    to: '/admin/destinos',
    icon: '📍',
    titulo: 'Destinos',
    descripcion: 'Administra los destinos turísticos',
  },
  {
    to: '/admin/categorias',
    icon: '🏷️',
    titulo: 'Categorías',
    descripcion: 'Categorías de atracciones',
  },
  {
    to: '/admin/idiomas',
    icon: '🌐',
    titulo: 'Idiomas',
    descripcion: 'Idiomas disponibles',
  },
  {
    to: '/admin/incluye',
    icon: '✅',
    titulo: 'Elementos incluidos',
    descripcion: 'Servicios y recursos incluidos',
  },
]

function DashboardPage() {
  return (
    <section className="page-section">
      <div style={{ marginBottom: '2rem', textAlign: 'center' }}>
        <p className="eyebrow">Panel de control</p>
        <h1 style={{ fontSize: '1.8rem', marginTop: '0.4rem' }}>Administración</h1>
        <p className="text-muted" style={{ marginTop: '0.5rem' }}>
          Gestiona atracciones, tickets, horarios, reservas y usuarios desde un solo lugar.
        </p>
      </div>

      <div className="admin-dashboard-grid">
        {SECCIONES.map((s) => (
          <Link key={s.to} to={s.to} className="admin-dashboard-card">
            <span className="card-icon">{s.icon}</span>
            <h3>{s.titulo}</h3>
            <p>{s.descripcion}</p>
          </Link>
        ))}
      </div>
    </section>
  )
}

export default DashboardPage
