import { useEffect, useState } from 'react'
import { adminApi } from '../../api/adminApi'
import ErrorMessage from '../../components/common/ErrorMessage'
import Spinner from '../../components/common/Spinner'

const LIMIT = 10

function GestionUsuariosPage() {
  const [usuarios, setUsuarios] = useState([])
  const [cargando, setCargando] = useState(false)
  const [error, setError] = useState('')
  const [page, setPage] = useState(1)
  const [totalPages, setTotalPages] = useState(1)

  const cargar = async (p = 1) => {
    setCargando(true)
    setError('')
    try {
      const { data, pagination } = await adminApi.listarUsuariosAdmin({ page: p, limit: LIMIT })
      setUsuarios(Array.isArray(data) ? data : [])
      setPage(p)
      const total = pagination?.total ?? data?.length ?? 0
      const limit = pagination?.limit ?? LIMIT
      setTotalPages(Math.max(1, Math.ceil(total / limit)))
    } catch (err) {
      setError(err?.response?.data?.message || 'No se pudieron cargar los usuarios.')
    } finally {
      setCargando(false)
    }
  }

  useEffect(() => { cargar(1) }, [])

  return (
    <section className="page-section">
      <div className="admin-page-header">
        <h1>Gestión de Usuarios</h1>
      </div>
      {cargando && <Spinner message="Cargando usuarios..." />}
      <ErrorMessage mensaje={error} />

      <div className="table-wrap">
        <table className="admin-table">
          <thead>
            <tr>
              <th>Login</th>
              <th>Roles</th>
              <th>Estado</th>
            </tr>
          </thead>
          <tbody>
            {usuarios.length === 0 && !cargando && (
              <tr>
                <td colSpan={3} style={{ textAlign: 'center', color: 'var(--text-muted)', padding: '2rem' }}>
                  No hay usuarios registrados.
                </td>
              </tr>
            )}
            {usuarios.map((item) => (
              <tr key={item.usr_guid ?? item.guid ?? item.login}>
                <td>{item.login}</td>
                <td>{(item.roles || [item.rol]).filter(Boolean).join(', ') || '—'}</td>
                <td>{item.estado || '—'}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

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

export default GestionUsuariosPage
