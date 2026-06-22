import { apolloClient, MUTATIONS, QUERIES, parseGraphqlJson } from './client'

function sleep(ms) {
  return new Promise((resolve) => setTimeout(resolve, ms))
}

/** Genera un UUID v4 compatible con todos los navegadores. */
export function newCorrelationId() {
  if (typeof crypto !== 'undefined' && crypto.randomUUID) return crypto.randomUUID()
  return `${Date.now()}-${Math.random().toString(36).slice(2)}`
}

export async function graphqlListarAtracciones(params = {}) {
  const { data } = await apolloClient.query({
    query: QUERIES.ATRACCIONES,
    variables: {
      ciudad: params.Ciudad || params.ciudad || null,
      tipo: params.Tipo || params.tipo || null,
      subtipo: params.Subtipo || params.subtipo || null,
      idioma: params.Idioma || params.idioma || null,
      calificacionMin: params.CalificacionMin ?? params.calificacion_min ?? null,
      disponible: params.Disponible ?? params.disponible ?? null,
      ordenarPor: params.OrdenarPor || params.ordenar_por || null,
      page: params.Page || params.page || 1,
      limit: params.Limit || params.limit || 10,
    },
    fetchPolicy: 'network-only',
  })
  return parseGraphqlJson(data.atracciones)
}

export async function graphqlObtenerFiltros(ciudad) {
  const { data } = await apolloClient.query({
    query: QUERIES.FILTROS,
    variables: { ciudad: ciudad || null },
    fetchPolicy: 'network-only',
  })
  const raw = parseGraphqlJson(data.filtros)
  return raw?.data ?? raw
}

export async function graphqlObtenerAtraccion(guid) {
  const { data } = await apolloClient.query({
    query: QUERIES.ATRACCION,
    variables: { guid },
    fetchPolicy: 'network-only',
  })
  return parseGraphqlJson(data.atraccion)
}

export async function graphqlObtenerHorarios(atGuid, disponibles = true) {
  const { data } = await apolloClient.query({
    query: QUERIES.HORARIOS,
    variables: { atGuid: String(atGuid), disponibles },
    fetchPolicy: 'network-only',
  })
  const raw = parseGraphqlJson(data.horarios)
  return raw?.data ?? raw ?? []
}

export async function graphqlObtenerTickets(atGuid) {
  const { data } = await apolloClient.query({
    query: QUERIES.TICKETS,
    variables: { atGuid: String(atGuid) },
    fetchPolicy: 'network-only',
  })
  const raw = parseGraphqlJson(data.tickets)
  return raw?.data ?? raw ?? []
}

export async function graphqlSolicitarReserva(input) {
  const { data } = await apolloClient.mutate({
    mutation: MUTATIONS.SOLICITAR_RESERVA,
    variables: {
      input: {
        cliGuid: input.cli_guid || null,
        atGuid: String(input.at_guid),
        horGuid: String(input.hor_guid),
        fechaVisita: input.fecha_visita || null,
        origenCanal: input.origen_canal || 'MARKETPLACE',
        lineas: (input.lineas || []).map((l) => ({
          tckGuid: String(l.tck_guid),
          cantidad: l.cantidad,
        })),
        clienteInvitado: input.cliente_invitado
          ? {
              tipoIdentificacion: input.cliente_invitado.tipo_identificacion,
              numeroIdentificacion: input.cliente_invitado.numero_identificacion,
              nombres: input.cliente_invitado.nombres || null,
              apellidos: input.cliente_invitado.apellidos || null,
              correo: input.cliente_invitado.correo,
              telefono: input.cliente_invitado.telefono || null,
              direccion: input.cliente_invitado.direccion || null,
            }
          : null,
      },
    },
  })
  return data.solicitarReserva
}

export async function graphqlEsperarConfirmacionReserva(seguimientoId, { maxIntentos = 30, intervaloMs = 2000 } = {}) {
  for (let i = 0; i < maxIntentos; i += 1) {
    const { data } = await apolloClient.query({
      query: QUERIES.ESTADO_RESERVA,
      variables: { seguimientoId: String(seguimientoId) },
      fetchPolicy: 'network-only',
    })
    const estado = data?.estadoReserva
    if (!estado) {
      await sleep(intervaloMs)
      continue
    }
    if (estado.estado === 'CONFIRMADA') return estado
    if (estado.estado === 'RECHAZADA') {
      throw new Error(estado.motivoRechazo || 'La reserva fue rechazada.')
    }
    await sleep(intervaloMs)
  }
  throw new Error('Tiempo de espera agotado. La reserva sigue en proceso.')
}

