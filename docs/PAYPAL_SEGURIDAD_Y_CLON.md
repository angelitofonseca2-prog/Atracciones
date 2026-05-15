# PayPal seguro en el repositorio principal

Este repositorio (**Atracciones**) es la base de despliegue y mantiene el flujo de pago con **PayPal Checkout (Orders API v2)**: creación de orden en servidor, aprobación en la UI alojada de PayPal, **captura en servidor** y webhooks firmados. No se acepta PAN ni CVV en backend propio.

## Separación respecto a un clon “inseguro” de laboratorio

Si el equipo conserva una **copia anterior** del proyecto solo para informes académicos (pago simulado, endpoints sin verificación PSP, ausencia de CSP, etc.), esa copia **no** debe:

- recibir secretos reales de PayPal (`client_secret`, `webhook_id` de producción);
- desplegarse como producción ni mezclarse con este monorepo por ramas no controladas.

El código de producción y el informe de riesgos deben vivir en **repositorios o ramas claramente etiquetadas** para no confundir despliegues.

## Puntos de verificación en este repo

- Reserva activa (`P` → `A`) solo tras **captura verificada** en el orquestador y validación de monto/moneda/`rev_guid`.
- Webhooks con **verificación de firma** PayPal e **idempotencia** por `transmission_id`.
- Secretos solo en variables de entorno / user-secrets; el frontend solo expone `VITE_PAYPAL_CLIENT_ID` (público).
- CSP y cabeceras HTTP alineadas con dominios de PayPal (ver `index.html` del frontend y el gateway).
