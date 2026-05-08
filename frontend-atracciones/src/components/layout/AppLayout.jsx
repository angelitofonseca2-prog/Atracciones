import { Outlet } from 'react-router-dom'
import Toast from '../common/Toast'
import Footer from './Footer'
import Header from './Header'

function AppLayout() {
  return (
    <div className="app-shell">
      <Header />
      <main className="app-main">
        <Outlet />
      </main>
      <Footer />
      <Toast />
    </div>
  )
}

export default AppLayout
