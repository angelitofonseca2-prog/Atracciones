import { Navigate, Route, Routes } from 'react-router-dom'
import DashboardPage from './admin/pages/DashboardPage'
import GestionAtraccionesPage from './admin/pages/GestionAtraccionesPage'
import GestionCategoriasPage from './admin/pages/GestionCategoriasPage'
import GestionDestinosPage from './admin/pages/GestionDestinosPage'
import GestionHorariosPage from './admin/pages/GestionHorariosPage'
import GestionIdiomasPage from './admin/pages/GestionIdiomasPage'
import GestionIncluyePage from './admin/pages/GestionIncluyePage'
import GestionReservasPage from './admin/pages/GestionReservasPage'
import GestionTicketsPage from './admin/pages/GestionTicketsPage'
import GestionUsuariosPage from './admin/pages/GestionUsuariosPage'
import CatalogoPage from './client/pages/CatalogoPage'
import DetallePage from './client/pages/DetallePage'
import HomePage from './client/pages/HomePage'
import LoginPage from './client/pages/LoginPage'
import MisFacturasPage from './client/pages/MisFacturasPage'
import MisReservasPage from './client/pages/MisReservasPage'
import PerfilPage from './client/pages/PerfilPage'
import RegistroPage from './client/pages/RegistroPage'
import ReservaPage from './client/pages/ReservaPage'
import AppLayout from './components/layout/AppLayout'
import RutaAdmin from './components/routing/RutaAdmin'
import RutaProtegida from './components/routing/RutaProtegida'

function App() {
  return (
    <Routes>
      <Route element={<AppLayout />}>
        <Route path="/" element={<HomePage />} />
        <Route path="/atracciones" element={<CatalogoPage />} />
        <Route path="/atracciones/:guid" element={<DetallePage />} />
        <Route path="/reservar/:guid" element={<ReservaPage />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/registro" element={<RegistroPage />} />
        <Route
          path="/mis-reservas"
          element={
            <RutaProtegida>
              <MisReservasPage />
            </RutaProtegida>
          }
        />
        <Route
          path="/mis-facturas"
          element={
            <RutaProtegida>
              <MisFacturasPage />
            </RutaProtegida>
          }
        />
        <Route
          path="/perfil"
          element={
            <RutaProtegida>
              <PerfilPage />
            </RutaProtegida>
          }
        />
        <Route
          path="/admin"
          element={
            <RutaAdmin>
              <DashboardPage />
            </RutaAdmin>
          }
        />
        <Route
          path="/admin/atracciones"
          element={
            <RutaAdmin>
              <GestionAtraccionesPage />
            </RutaAdmin>
          }
        />
        <Route
          path="/admin/tickets"
          element={
            <RutaAdmin>
              <GestionTicketsPage />
            </RutaAdmin>
          }
        />
        <Route
          path="/admin/horarios"
          element={
            <RutaAdmin>
              <GestionHorariosPage />
            </RutaAdmin>
          }
        />
        <Route
          path="/admin/reservas"
          element={
            <RutaAdmin>
              <GestionReservasPage />
            </RutaAdmin>
          }
        />
        <Route
          path="/admin/usuarios"
          element={
            <RutaAdmin>
              <GestionUsuariosPage />
            </RutaAdmin>
          }
        />
        <Route
          path="/admin/destinos"
          element={
            <RutaAdmin>
              <GestionDestinosPage />
            </RutaAdmin>
          }
        />
        <Route
          path="/admin/categorias"
          element={
            <RutaAdmin>
              <GestionCategoriasPage />
            </RutaAdmin>
          }
        />
        <Route
          path="/admin/idiomas"
          element={
            <RutaAdmin>
              <GestionIdiomasPage />
            </RutaAdmin>
          }
        />
        <Route
          path="/admin/incluye"
          element={
            <RutaAdmin>
              <GestionIncluyePage />
            </RutaAdmin>
          }
        />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Route>
    </Routes>
  )
}

export default App
