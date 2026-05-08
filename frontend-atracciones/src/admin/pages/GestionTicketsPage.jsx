import { useState } from 'react'
import ErrorMessage from '../../components/common/ErrorMessage'
import Spinner from '../../components/common/Spinner'
import FormularioTicket from '../components/FormularioTicket'
import TablaTickets from '../components/TablaTickets'
import { useGestionTickets } from '../hooks/useGestionTickets'

function GestionTicketsPage() {
  const { items, cargando, error, guardar, eliminar } = useGestionTickets()
  const [editando, setEditando] = useState(null)
  const [mostrarFormulario, setMostrarFormulario] = useState(false)

  const onGuardar = async (payload) => {
    await guardar(payload, editando?.tck_guid)
    setMostrarFormulario(false)
    setEditando(null)
  }

  return (
    <section className="page-section">
      <div className="admin-page-header">
        <h1>Gestión de Tickets</h1>
        <button className="btn" type="button" onClick={() => { setEditando(null); setMostrarFormulario(true) }}>
          Crear nuevo
        </button>
      </div>

      {mostrarFormulario && (
        <FormularioTicket
          inicial={editando}
          onGuardar={onGuardar}
          onCancelar={() => { setMostrarFormulario(false); setEditando(null) }}
        />
      )}

      {cargando && <Spinner message="Cargando tickets..." />}
      <ErrorMessage mensaje={error} />

      <TablaTickets
        items={items}
        onEditar={(item) => { setEditando(item); setMostrarFormulario(true) }}
        onEliminar={eliminar}
      />
    </section>
  )
}

export default GestionTicketsPage
