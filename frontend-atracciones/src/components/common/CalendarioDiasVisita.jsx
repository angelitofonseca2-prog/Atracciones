import { useEffect, useMemo, useState } from 'react'
import { formatearFechaCorta, formatearRangoFechas } from '../../utils/formatFechas'

const DIAS_SEMANA = ['Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb', 'Dom']
const MESES = [
  'Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio',
  'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre',
]

function parseUtc(yyyyMmDd) {
  const [y, m, d] = String(yyyyMmDd).slice(0, 10).split('-').map(Number)
  return { y, m: m - 1, d }
}

function isoFromParts(y, m, d) {
  const dt = new Date(Date.UTC(y, m, d))
  return dt.toISOString().slice(0, 10)
}

function diasDelMes(y, m) {
  const primero = new Date(Date.UTC(y, m, 1))
  const ultimoDia = new Date(Date.UTC(y, m + 1, 0)).getUTCDate()
  const offset = (primero.getUTCDay() + 6) % 7
  const celdas = []
  for (let i = 0; i < offset; i += 1) celdas.push(null)
  for (let d = 1; d <= ultimoDia; d += 1) {
    celdas.push(isoFromParts(y, m, d))
  }
  return celdas
}

/**
 * Calendario mensual: solo los días en `diasHabilitados` son seleccionables.
 */
export default function CalendarioDiasVisita({
  diasHabilitados = [],
  valor = '',
  onChange,
  rangoLabel = '',
  error = false,
}) {
  const habilitadosSet = useMemo(() => new Set(diasHabilitados), [diasHabilitados])

  const mesesConDias = useMemo(() => {
    const map = new Map()
    diasHabilitados.forEach((iso) => {
      const { y, m } = parseUtc(iso)
      map.set(`${y}-${m}`, { y, m })
    })
    return [...map.values()].sort((a, b) => (a.y - b.y) || (a.m - b.m))
  }, [diasHabilitados])

  const [indiceMes, setIndiceMes] = useState(0)

  useEffect(() => {
    setIndiceMes(0)
  }, [diasHabilitados])

  useEffect(() => {
    if (!valor || !habilitadosSet.has(valor)) return
    const { y, m } = parseUtc(valor)
    const idx = mesesConDias.findIndex((x) => x.y === y && x.m === m)
    if (idx >= 0) setIndiceMes(idx)
  }, [valor, mesesConDias, habilitadosSet])

  useEffect(() => {
    if (diasHabilitados.length !== 1 || valor) return
    onChange?.(diasHabilitados[0])
  }, [diasHabilitados, valor, onChange])

  if (diasHabilitados.length === 0) {
    return (
      <p className="text-muted text-sm calendario-visita-vacio">
        No hay fechas disponibles en este horario.
      </p>
    )
  }

  const mesActual = mesesConDias[indiceMes] ?? mesesConDias[0]
  const celdas = mesActual ? diasDelMes(mesActual.y, mesActual.m) : []
  const puedeAnterior = indiceMes > 0
  const puedeSiguiente = indiceMes < mesesConDias.length - 1

  return (
    <div className={`calendario-visita${error ? ' calendario-visita--error' : ''}`}>
      {rangoLabel && (
        <p className="calendario-visita-rango text-muted text-sm">
          Periodo del horario: <strong>{rangoLabel}</strong>
        </p>
      )}

      <div className="calendario-visita-nav">
        <button
          type="button"
          className="btn btn-outline btn-sm"
          disabled={!puedeAnterior}
          onClick={() => setIndiceMes((i) => Math.max(0, i - 1))}
          aria-label="Mes anterior"
        >
          ‹
        </button>
        <span className="calendario-visita-mes">
          {mesActual ? `${MESES[mesActual.m]} ${mesActual.y}` : '—'}
        </span>
        <button
          type="button"
          className="btn btn-outline btn-sm"
          disabled={!puedeSiguiente}
          onClick={() => setIndiceMes((i) => Math.min(mesesConDias.length - 1, i + 1))}
          aria-label="Mes siguiente"
        >
          ›
        </button>
      </div>

      <div className="calendario-visita-grid" role="grid" aria-label="Calendario de visita">
        {DIAS_SEMANA.map((d) => (
          <span key={d} className="calendario-visita-dow" role="columnheader">
            {d}
          </span>
        ))}
        {celdas.map((iso, i) => {
          if (!iso) {
            return <span key={`empty-${i}`} className="calendario-visita-celda calendario-visita-celda--vacia" />
          }
          const activo = habilitadosSet.has(iso)
          const seleccionado = valor === iso
          return (
            <button
              key={iso}
              type="button"
              role="gridcell"
              disabled={!activo}
              className={[
                'calendario-visita-celda',
                activo ? 'calendario-visita-celda--activa' : 'calendario-visita-celda--inactiva',
                seleccionado ? 'calendario-visita-celda--seleccionada' : '',
              ].filter(Boolean).join(' ')}
              onClick={() => activo && onChange?.(iso)}
              aria-label={activo ? formatearFechaCorta(iso) : undefined}
              aria-pressed={seleccionado}
            >
              {parseUtc(iso).d}
            </button>
          )
        })}
      </div>

      {valor && (
        <p className="calendario-visita-seleccion text-sm">
          Día elegido: <strong>{formatearFechaCorta(valor)}</strong>
        </p>
      )}
    </div>
  )
}
