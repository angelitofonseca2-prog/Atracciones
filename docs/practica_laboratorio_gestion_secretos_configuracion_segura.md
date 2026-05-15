# Práctica de Laboratorio: Gestión de Secretos y Configuración Segura

**Nivel:** 6to nivel Ingeniería en Sistemas

## Objetivo
Aplicar buenas prácticas de ciberseguridad mediante el manejo seguro de secretos, variables de entorno y configuración basada en mínimo privilegio, usando como caso de estudio el proyecto `Atracciones`.

El trabajo se desarrollará sobre dos copias del mismo sistema:

- **Clon inseguro controlado:** conserva problemas de seguridad no resueltos para demostrar el riesgo.
- **Clon seguro/endurecido:** incorpora las correcciones de configuración, protección de secretos y controles de seguridad.

Además, se tomará en cuenta el plan de blindaje de pagos propuesto para reducir riesgos de fraude, chargebacks y abuso del flujo de confirmación de pago.

## Herramientas
- Cursor o Visual Studio Code
- Git y GitHub
- .NET 10
- React + Vite
- PostgreSQL
- Docker y Docker Compose
- `dotnet user-secrets`
- Archivo `.env.local`
- Windows con terminal PowerShell
- Postman o Insomnia
- pgAdmin
- Jaeger / OpenTelemetry
- OWASP ZAP o Burp Suite Community para pruebas controladas

## Fase 1. Investigación inicial
Crear una aplicación básica con conexión a base de datos. Se trabajará con un proyecto con errores de seguridad intencionales.

Los estudiantes deberán investigar y seleccionar:

### Aplicación web o API sencilla
Se utilizará el proyecto `Atracciones`, que actualmente está compuesto por:

- `frontend-atracciones`: aplicación web SPA en React/Vite
- `platform/gateway`: API Gateway con YARP
- `services/ms-identidad`: autenticación y emisión de JWT
- `services/ms-atracciones`: catálogo, inventario y reseñas
- `services/ms-reservas`: reservas y CRM
- `services/ms-orquestador`: sagas de reservas y confirmación de pago
- `services/ms-facturacion`: emisión de facturas
- `services/ms-auditoria`: auditoría de eventos

### Base de datos (MySQL, PostgreSQL o MongoDB)
Se trabajará con **PostgreSQL**, con bases separadas por servicio:

- `auth_db`
- `atracciones_db`
- `reservas_db`
- `orquestador_db`
- `facturacion_db`
- `audit_db`

### Herramientas de desarrollo y control de versiones
- Git
- GitHub
- Cursor / Visual Studio Code
- Docker Compose
- pgAdmin
- Postman o Insomnia

### Métodos seguros para gestión de secretos
Para la versión segura del laboratorio se utilizarán estos mecanismos:

- `dotnet user-secrets` para secretos locales en microservicios .NET
- variables de entorno en contenedores y despliegues
- `.env.local` para configuración del frontend no sensible
- exclusión de archivos sensibles con `.gitignore`
- separación entre configuración base (`appsettings.json`) y secretos reales fuera del repositorio

## Fase 2. Identificación de vulnerabilidades (Implementación insegura controlada)
Configurar una aplicación con:

### Contraseñas visibles en código
En el estado actual del proyecto existen credenciales de desarrollo visibles en archivos versionados, por ejemplo:

- `platform/docker-compose.yml`
- `services/ms-identidad/src/Atracciones.MsIdentidad.Api/appsettings.Development.json`
- `services/ms-orquestador/src/Atracciones.MsOrquestador.Api/appsettings.Development.json`

Ejemplos de información expuesta:

- usuarios y contraseñas de PostgreSQL
- cadenas de conexión completas
- claves internas de sincronización como `dev-monolith-sync-key`

### Tokens expuestos
En el laboratorio se considerará como debilidad la exposición de claves internas, API keys de sincronización y configuraciones JWT de desarrollo dentro de archivos que pueden ser consultados por cualquier desarrollador con acceso al repositorio.

### Variables sensibles sin protección
Aunque los microservicios ya tienen `UserSecretsId` configurado, todavía existen secretos y configuraciones sensibles visibles en archivos de desarrollo y en variables definidas directamente en `docker-compose.yml`.

### Usuarios con privilegios administrativos (Permisos administrativos innecesarios)
En la versión insegura controlada se asumirá que los usuarios técnicos poseen permisos más amplios de los necesarios para evidenciar la importancia del mínimo privilegio.

En este proyecto el riesgo se refleja en:

- credenciales completas por servicio
- acceso directo a bases por usuario técnico
- ausencia de una política explícita de privilegios mínimos documentada para laboratorio

### Problemática específica detectada en el proyecto
Además de la exposición de secretos, el sistema presenta una problemática lógica importante en el flujo de pago:

- hoy existe una **simulación de pago**
- el flujo de confirmación aún no depende de un proveedor real de pagos
- el endpoint de confirmación de pago es una superficie crítica que debe endurecerse antes de integrar un PSP real

Esto implica que el sistema es útil como caso de estudio para demostrar cómo una mala configuración y un mal diseño de endpoints sensibles pueden favorecer:

- fraude lógico
- confirmaciones de pago no confiables
- abuso del flujo de reservas
- mayor impacto ante futuras vulnerabilidades XSS

## Fase 3. Corrección segura y endurecimiento
Aplicar:

### Crear archivo con variables de entorno `.env`
En la versión segura se usará:

- `.env.local` para el frontend
- variables de entorno para contenedores
- `dotnet user-secrets` para valores sensibles de microservicios

El archivo `.env.local` contendrá solo configuración no sensible, por ejemplo:

- `VITE_API_URL`

No se almacenarán en frontend:

- contraseñas de base de datos
- API keys privadas
- secretos de proveedores de pago

### Exclusión de secretos con `.gitignore`
Se mantendrá y reforzará la exclusión de:

- `.env`
- archivos locales
- artefactos temporales
- configuraciones privadas

Esto evita subir por error secretos al repositorio.

### Aplicar mínimo privilegio en BD y usuarios
En la versión segura se definirá que cada servicio tenga acceso únicamente a su propia base y a los permisos estrictamente necesarios.

Objetivo:

- `ms-identidad` solo sobre `auth_db`
- `ms-atracciones` solo sobre `atracciones_db`
- `ms-reservas` solo sobre `reservas_db`
- `ms-orquestador` solo sobre `orquestador_db`
- `ms-facturacion` solo sobre `facturacion_db`
- `ms-auditoria` solo sobre `audit_db`

Esto limita el daño si una credencial es expuesta o reutilizada.

### Separación de ambientes de desarrollo y producción
La versión segura debe diferenciar claramente:

- desarrollo
- pruebas
- producción

Cada ambiente tendrá:

- credenciales distintas
- endpoints distintos
- políticas distintas
- configuración separada

Además:

- en desarrollo se usará `dotnet user-secrets`
- en producción se usarán variables de entorno o gestor seguro de secretos
- no se versionarán credenciales reales en `appsettings.*.json`

### Endurecimiento adicional basado en el proyecto actual
Como parte del endurecimiento de este proyecto se aplicará también:

- rediseño seguro del flujo de pago
- validación server-to-server con proveedor de pago
- eliminación del patrón de confirmación de pago impulsado solo por el cliente
- políticas CSP y headers de seguridad en gateway/frontend
- reducción del riesgo XSS

## Fase 4 - Validación, análisis y pruebas (20 min)
Ejecutar la aplicación y comprobar que funciona sin exponer secretos.

Realizar pruebas funcionales y documentar vulnerabilidades corregidas y riesgos mitigados.

### Pruebas a realizar en el clon inseguro
- revisar archivos de configuración para identificar secretos visibles
- verificar exposición de cadenas de conexión y claves internas
- comprobar que el flujo de pago sigue siendo una simulación
- demostrar que existe una superficie crítica en la confirmación de pago
- verificar ausencia de endurecimiento suficiente en navegador/gateway

### Pruebas a realizar en el clon seguro
- verificar que la aplicación funciona con secretos fuera del código versionado
- comprobar uso de `.env.local` y `dotnet user-secrets`
- validar que no existen credenciales reales en el repositorio
- verificar separación de ambientes
- comprobar limitación de permisos en base de datos
- validar el flujo de pago seguro propuesto
- revisar headers de seguridad y políticas del gateway
- documentar riesgos mitigados respecto al clon inseguro

### Tipo de ataque que se va a realizar
Se ejecutarán ataques controlados de laboratorio, orientados a demostrar riesgo sin afectar terceros.

#### Ataque 1. Exposición y reutilización de secretos
**Objetivo:** demostrar que un secreto visible en archivos versionados puede permitir acceso indebido a servicios internos.

**Ejemplos de prueba:**
- lectura de credenciales en `docker-compose.yml`
- revisión de `appsettings.Development.json`
- uso de conexiones visibles en entorno local

#### Ataque 2. Abuso lógico del flujo de pago
**Objetivo:** demostrar que una confirmación de pago sin validación contra un proveedor real no es confiable.

**Ejemplo de prueba:**
- analizar el flujo de confirmación actual
- evidenciar que se necesita webhook firmado y verificación server-to-server

#### Ataque 3. Prueba controlada de XSS
**Objetivo:** demostrar que, aunque hoy React escape texto, un futuro flujo de pago sería riesgoso sin CSP y endurecimiento.

**Ejemplo de prueba:**
- introducir payloads controlados en campos de texto
- verificar renderizado
- justificar la necesidad de CSP y restricciones de scripts

#### Ataque 4. Revisión de privilegios excesivos
**Objetivo:** demostrar el impacto de credenciales con privilegios amplios.

### Límites del ataque
- se realizará únicamente en entorno local o de laboratorio
- no se usarán tarjetas reales ni pasarelas reales de cobro
- no se atacarán sistemas externos
- no se usarán datos reales de clientes
- no se harán ataques destructivos
- no se ejecutarán pruebas de denegación de servicio
- el objetivo será demostrar vulnerabilidades y su mitigación

## Resultados esperados
- aplicación funcional en entorno inseguro controlado
- aplicación funcional en entorno seguro/endurecido
- variables sensibles protegidas
- secretos fuera del repositorio
- roles y permisos limitados correctamente
- flujo de pago rediseñado de forma más segura
- reducción del riesgo de fraude lógico
- reducción del impacto potencial de XSS
- evidencia clara de buenas prácticas de configuración segura

## Entregables
- código del clon inseguro controlado
- código del clon seguro/endurecido
- capturas de pantalla
- evidencia de secretos expuestos en el clon inseguro
- evidencia de secretos protegidos en el clon seguro
- evidencia de `.gitignore`, `.env.local` y `dotnet user-secrets`
- informe técnico con vulnerabilidades detectadas y soluciones aplicadas
- conclusiones sobre desarrollo seguro

## Informe técnico con vulnerabilidades detectadas y soluciones aplicadas

### Vulnerabilidades detectadas
1. Credenciales visibles en configuración de desarrollo.
2. Claves internas expuestas en archivos versionados.
3. Separación incompleta entre configuración sensible y configuración pública.
4. Falta de política explícita de mínimo privilegio para laboratorio.
5. Flujo de pago todavía simulado y no respaldado por un proveedor real.
6. Necesidad de endurecimiento adicional contra XSS y abuso de endpoints sensibles.

### Soluciones aplicadas
1. Migrar secretos a `dotnet user-secrets` y variables de entorno.
2. Mantener `.env.local` solo para configuración no sensible del frontend.
3. Excluir secretos con `.gitignore`.
4. Definir permisos mínimos por servicio y por base de datos.
5. Separar ambientes de desarrollo y producción.
6. Rediseñar el flujo de pago con:
   - checkout/tokenización del proveedor
   - webhook firmado
   - validación server-to-server
   - separación entre estado de reserva y estado de pago
7. Incorporar CSP y headers de seguridad.

## Conclusiones sobre desarrollo seguro
El proyecto `Atracciones` permite demostrar que la seguridad no depende solo del código funcional, sino también de cómo se gestionan los secretos, permisos y configuraciones. La comparación entre el clon inseguro y el clon seguro evidencia que:

- una credencial expuesta puede comprometer varios componentes
- los secretos no deben permanecer en archivos versionados
- el principio de mínimo privilegio reduce el impacto de una filtración
- la separación por ambientes evita trasladar malas prácticas a producción
- un flujo de pago mal diseñado puede facilitar fraude aunque todavía no exista un PSP real
- el endurecimiento del navegador y del gateway es clave para prevenir riesgos futuros como XSS y robo de sesión

## Anexo: comparación entre ambos clones

### Clon inseguro controlado
- credenciales visibles
- variables sensibles en archivos versionados
- flujo de pago simulado sin validación real
- configuración más débil
- mayor superficie de ataque

### Clon seguro/endurecido
- secretos fuera del repositorio
- uso de `dotnet user-secrets`
- uso de variables de entorno
- mínimo privilegio en base de datos
- separación de ambientes
- flujo de pago más seguro
- mayor control contra fraude y XSS
