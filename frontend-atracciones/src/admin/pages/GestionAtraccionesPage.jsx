import { useEffect, useState } from 'react'
import ErrorMessage from '../../components/common/ErrorMessage'
import Spinner from '../../components/common/Spinner'
import FormularioAtraccion from '../components/FormularioAtraccion'
import TablaAtracciones from '../components/TablaAtracciones'
import { useGestionAtracciones } from '../hooks/useGestionAtracciones'

function GestionAtraccionesPage() {
  const { items, cargando, error, page, totalPages, cargar, guardar, desactivar } =
    useGestionAtracciones()
  const [editando, setEditando] = useState(null)
  const [mostrarFormulario, setMostrarFormulario] = useState(false)

  useEffect(() => {
    cargar(1)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  const abrirNuevo = () => {
    setEditando(null)
    setMostrarFormulario(true)
  }

  const abrirEditar = (item) => {
    // AtraccionAdminResponse del listado ya trae todos los GUIDs necesarios
    // para precargar el formulario. No existe GET /admin/atracciones/{guid}.
    setEditando({
      ...item,
      categoria_guids: Array.isArray(item.categoria_guids) ? item.categoria_guids : [],
      idioma_guids:    Array.isArray(item.idioma_guids)    ? item.idioma_guids    : [],
      imagen_guids:    Array.isArray(item.imagen_guids)    ? item.imagen_guids    : [],
      incluye_guids:   Array.isArray(item.incluye_guids)   ? item.incluye_guids   : [],
    })
    setMostrarFormulario(true)
  }

  const onGuardar = async (payload) => {
    await guardar(payload, editando?.at_guid)
    setMostrarFormulario(false)
    setEditando(null)
  }

  return (
    <section className="page-section">
      <div className="admin-page-header">
        <h1>Gestión de Atracciones</h1>
        <button className="btn" type="button" onClick={abrirNuevo}>Crear nueva</button>
      </div>

      {mostrarFormulario && (
        <FormularioAtraccion
          inicial={editando}
          onGuardar={onGuardar}
          onCancelar={() => { setMostrarFormulario(false); setEditando(null) }}
        />
      )}

      {cargando && <Spinner message="Cargando..." />}
      <ErrorMessage mensaje={error} />

      <TablaAtracciones
        items={items}
        onEditar={abrirEditar}
        onDesactivar={desactivar}
      />

      <div className="pagination">
        <button
          className="btn btn-outline btn-sm"
          disabled={page <= 1}
          onClick={() => cargar(page - 1)}
        >
          ← Anterior
        </button>
        <span className="pagination-info">Página {page} de {totalPages}</span>
        <button
          className="btn btn-outline btn-sm"
          disabled={page >= totalPages}
          onClick={() => cargar(page + 1)}
        >
          Siguiente →
        </button>
      </div>
    </section>
  )
}

export default GestionAtraccionesPage
