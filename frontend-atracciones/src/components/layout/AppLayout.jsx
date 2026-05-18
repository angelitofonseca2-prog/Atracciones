import { Outlet } from 'react-router-dom'
import Toast from '../common/Toast'
import Footer from './Footer'
import Header from './Header'
import PageBackNav from './PageBackNav'

function AppLayout() {
  return (
    <div className="app-shell">
      <Header />
      <main className="app-main">
        <PageBackNav />
        <Outlet />
      </main>
      <Footer />
      <Toast />
    </div>
  )
}

export default AppLayout
