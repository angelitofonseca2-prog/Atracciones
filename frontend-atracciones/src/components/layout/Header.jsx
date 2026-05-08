import { useState } from 'react'
import { Link, NavLink, useNavigate } from 'react-router-dom'
import { useAuthContext } from '../../context/AuthContext'

function Header() {
  const { estaAutenticado, usuario, logout } = useAuthContext()
  const navigate = useNavigate()
  const [menuAbierto, setMenuAbierto] = useState(false)

  const esAdministrador = usuario?.roles?.includes('ADMIN')
  const esClienteAutenticado = estaAutenticado && !esAdministrador

  const handleLogout = () => {
    logout()
    navigate('/login')
    setMenuAbierto(false)
  }

  const cerrarMenu = () => setMenuAbierto(false)

  const navLinks = (
    <>
      <NavLink to="/" onClick={cerrarMenu}>Inicio</NavLink>
      <NavLink to="/atracciones" onClick={cerrarMenu}>Catálogo</NavLink>
      {esClienteAutenticado && <NavLink to="/mis-reservas" onClick={cerrarMenu}>Mis Reservas</NavLink>}
      {esClienteAutenticado && <NavLink to="/mis-facturas" onClick={cerrarMenu}>Mis Facturas</NavLink>}
      {esClienteAutenticado && <NavLink to="/perfil" onClick={cerrarMenu}>Mi Perfil</NavLink>}
      {esAdministrador && <NavLink to="/admin" onClick={cerrarMenu}>Administración</NavLink>}
    </>
  )

  return (
    <>
      <header className="site-header">
        <Link to="/" className="brand" onClick={cerrarMenu}>
          Aventuras <span>Reservadas</span>
        </Link>

        <nav className="top-nav">
          {navLinks}
        </nav>

        <div className="header-auth">
          {estaAutenticado ? (
            <>
              <span className="username">{usuario?.login || 'Usuario'}</span>
              <button type="button" className="btn btn-outline btn-sm" onClick={handleLogout}>
                Cerrar sesión
              </button>
            </>
          ) : (
            <>
              <Link to="/login" className="btn btn-outline btn-sm">
                Iniciar sesión
              </Link>
              <Link to="/registro" className="btn btn-sm">
                Registrarse
              </Link>
            </>
          )}
        </div>

        <button
          type="button"
          className={`hamburger${menuAbierto ? ' open' : ''}`}
          onClick={() => setMenuAbierto((v) => !v)}
          aria-label="Menú"
        >
          <span />
          <span />
          <span />
        </button>
      </header>

      <nav className={`mobile-nav${menuAbierto ? '' : ' hidden'}`}>
        {navLinks}
        <hr className="divider" />
        {estaAutenticado ? (
          <button type="button" className="btn btn-outline btn-sm" onClick={handleLogout}>
            Cerrar sesión
          </button>
        ) : (
          <div style={{ display: 'flex', gap: '0.5rem' }}>
            <Link to="/login" className="btn btn-outline btn-sm" onClick={cerrarMenu}>Iniciar sesión</Link>
            <Link to="/registro" className="btn btn-sm" onClick={cerrarMenu}>Registrarse</Link>
          </div>
        )}
      </nav>
    </>
  )
}

export default Header
