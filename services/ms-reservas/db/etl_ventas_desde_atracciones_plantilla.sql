-- Plantilla ETL (Fase 5): copiar reservas existentes desde el monolito hacia ventas.*
-- Ajustar nombres de servidor/esquema/origen según entorno. Idempotente vía ON CONFLICT (rev_codigo).
--
-- Origen típico: atracciones.reserva / atracciones.reserva_detalle (IDs internos + FK).
-- Destino: ventas.reservas / ventas.reserva_detalle (solo GUIDs; cli_guid = usuario.cli_guid).

BEGIN;

-- CREATE SCHEMA IF NOT EXISTS ventas;
-- Ejemplo de INSERT transformado (pseudocódigo): requiere JOIN a cliente.usuario por usu_guid.

COMMIT;
