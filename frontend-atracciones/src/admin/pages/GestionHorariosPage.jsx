import { useState } from 'react'
import ErrorMessage from '../../components/common/ErrorMessage'
import Spinner from '../../components/common/Spinner'
import { emitirToast } from '../../components/common/Toast'
import FormularioHorario from '../components/FormularioHorario'
import TablaHorarios from '../components/TablaHorarios'
import { useGestionHorarios } from '../hooks/useGestionHorarios'

function GestionHorariosPage() {
  const { items, cargando, error, crear, actualizar, eliminar } = useGestionHorarios()
  const [editando, setEditando] = useState(null)
  const [mostrarFormulario, setMostrarFormulario] = useState(false)

  const handleCrear = async (payload) => {
    try {
      await crear(payload)
      setMostrarFormulario(false)
      setEditando(null)
    } catch (err) {
      emitirToast(
        err?.response?.data?.message || 'No se pudo crear el horario.',
        'error',
      )
    }
  }

  const handleActualizar = async (guid, payload) => {
    try {
      await actualizar(guid, payload)
      setMostrarFormulario(false)
      setEditando(null)
    } catch (err) {
      emitirToast(
        err?.response?.data?.message || 'No se pudo actualizar el horario.',
        'error',
      )
    }
  }

  return (
    <section className="page-section">
      <div className="admin-page-header">
        <h1>Gestión de Horarios</h1>
        <button className="btn" type="button" onClick={() => { setEditando(null); setMostrarFormulario(true) }}>
          Crear nuevo horario
        </button>
      </div>

      {mostrarFormulario && (
        <FormularioHorario
          inicial={editando}
          onCrear={handleCrear}
          onActualizar={handleActualizar}
          onCancelar={() => { setMostrarFormulario(false); setEditando(null) }}
        />
      )}

      {cargando && <Spinner message="Cargando horarios..." />}
      <ErrorMessage mensaje={error} />

      <TablaHorarios
        items={items}
        onEditar={(item) => { setEditando(item); setMostrarFormulario(true) }}
        onEliminar={eliminar}
      />
    </section>
  )
}

export default GestionHorariosPage
