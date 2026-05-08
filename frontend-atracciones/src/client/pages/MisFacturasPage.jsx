import { useEffect, useState } from 'react'
import * as facturasApi from '../../api/facturasApi'
import ErrorMessage from '../../components/common/ErrorMessage'
import Spinner from '../../components/common/Spinner'

/**
 * Listado de facturas del cliente autenticado.
 * Endpoint: GET /api/v1/facturas/mis-facturas
 * Respuesta: ApiListResponse<FacturaResponse>
 * Campos: fac_guid, fac_numero, rev_codigo, total, moneda, fecha_emision,
 *         estado, nombre_receptor, correo_receptor
 */

function ModalFactura({ factura, onCerrar }) {
  return (
    <div className="modal-overlay" onClick={onCerrar}>
      <div className="modal-card" onClick={(e) => e.stopPropagation()}>
        <h2>Factura #{factura.fac_numero || '—'}</h2>
        {factura.rev_codigo && (
          <p><strong>Código reserva:</strong> <span style={{ fontFamily: 'monospace' }}>{factura.rev_codigo}</span></p>
        )}
        {factura.nombre_receptor && (
          <p><strong>Receptor:</strong> {factura.nombre_receptor}</p>
        )}
        {factura.correo_receptor && (
          <p><strong>Correo:</strong> {factura.correo_receptor}</p>
        )}
        <p>
          <strong>Fecha de emisión:</strong>{' '}
          {factura.fecha_emision
            ? String(factura.fecha_emision).slice(0, 10)
            : '—'}
        </p>
        <p>
          <strong>Total:</strong> ${Number(factura.total ?? 0).toFixed(2)}
          {factura.moneda ? ` ${factura.moneda}` : ''}
        </p>
        <p><strong>Estado:</strong> {factura.estado || '—'}</p>
        {factura.observacion && (
          <p><strong>Observación:</strong> {factura.observacion}</p>
        )}
        <button className="btn btn-outline" style={{ marginTop: '1rem' }} onClick={onCerrar}>
          Cerrar
        </button>
      </div>
    </div>
  )
}

function MisFacturasPage() {
  const [facturas, setFacturas] = useState([])
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState('')
  const [page, setPage] = useState(1)
  const [totalPages, setTotalPages] = useState(1)
  const [seleccionada, setSeleccionada] = useState(null)

  const cargar = async (p = 1) => {
    setCargando(true)
    setError('')
    try {
      const respuesta = await facturasApi.listarMisFacturas({ page: p, limit: 10 })
      const lista = respuesta?.data || []
      setFacturas(lista)
      setPage(p)
      const pagination = respuesta?.pagination
      const total = pagination?.total ?? lista.length
      const limit = pagination?.limit ?? 10
      setTotalPages(Math.max(1, Math.ceil(total / limit)))
    } catch (err) {
      const status = err?.response?.status
      if (status === 401 || status === 403) {
        setError('No tienes permisos para ver las facturas. Verifica que hayas iniciado sesión.')
      } else {
        setError(err?.response?.data?.message || 'No se pudieron cargar las facturas.')
      }
    } finally {
      setCargando(false)
    }
  }

  useEffect(() => { cargar(1) }, [])

  return (
    <section className="page-section">
      <h1>Mis Facturas</h1>
      <ErrorMessage mensaje={error} />
      {cargando && <Spinner message="Cargando facturas..." />}

      {!cargando && !error && (
        <div className="table-wrap">
          <table className="admin-table">
            <thead>
              <tr>
                <th>Número</th>
                <th>Código reserva</th>
                <th>Fecha emisión</th>
                <th>Total</th>
                <th>Estado</th>
                <th>Receptor</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {facturas.length === 0 && (
                <tr>
                  <td colSpan={7} style={{ textAlign: 'center', color: 'var(--text-muted)', padding: '2rem' }}>
                    No tienes facturas registradas aún.
                  </td>
                </tr>
              )}
              {facturas.map((f) => (
                <tr key={f.fac_guid}>
                  <td>
                    <span style={{ fontFamily: 'monospace', fontSize: '0.85rem' }}>
                      {f.fac_numero || '—'}
                    </span>
                  </td>
                  <td>
                    <span style={{ fontFamily: 'monospace', fontSize: '0.85rem' }}>
                      {f.rev_codigo || '—'}
                    </span>
                  </td>
                  <td>{f.fecha_emision ? String(f.fecha_emision).slice(0, 10) : '—'}</td>
                  <td>
                    <strong>${Number(f.total ?? 0).toFixed(2)}</strong>
                    {f.moneda ? <span className="text-muted text-sm"> {f.moneda}</span> : ''}
                  </td>
                  <td>{f.estado || '—'}</td>
                  <td>{f.nombre_receptor || '—'}</td>
                  <td>
                    <button
                      className="btn btn-outline btn-sm"
                      onClick={() => setSeleccionada(f)}
                    >
                      Ver detalle
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

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

      {seleccionada && (
        <ModalFactura factura={seleccionada} onCerrar={() => setSeleccionada(null)} />
      )}
    </section>
  )
}

export default MisFacturasPage
