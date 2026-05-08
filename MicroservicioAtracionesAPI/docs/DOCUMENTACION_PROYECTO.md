# Documentación del Proyecto: Microservicio Atracciones

Este documento detalla la arquitectura, estructura de carpetas y flujos principales del microservicio de Atracciones. El proyecto sigue una arquitectura de N-Capas (4 capas) para garantizar la separación de responsabilidades, mantenibilidad y escalabilidad.

---

## 1. Resumen General de las Capas

El microservicio está dividido en las siguientes 4 capas lógicas:

1.  **DataAccess (Capa de Acceso a Datos):** Es la capa más interna que interactúa directamente con la base de datos SQL Server mediante Entity Framework Core. Contiene el contexto de base de datos, las entidades que mapean a las tablas y los repositorios básicos para operaciones CRUD.
2.  **DataManagement (Capa de Gestión de Datos):** Actúa como un puente entre la persistencia y la lógica de negocio. Orquesta los repositorios mediante el patrón *Unit of Work* y proporciona servicios de datos que transforman entidades en modelos internos, manejando la lógica de persistencia sin reglas de negocio complejas.
3.  **Business (Capa de Lógica de Negocio):** Contiene el núcleo funcional del sistema. Aquí se implementan las validaciones, reglas de dominio, orquestación de servicios y el mapeo a objetos de transferencia de datos (DTOs). Está dividida en servicios para Administradores, Usuarios Públicos y Autenticación.
4.  **Api (Capa de Presentación):** Es el punto de entrada al microservicio. Expone endpoints RESTful, gestiona la seguridad (JWT), maneja excepciones globales, implementa caché y proporciona la documentación interactiva a través de Swagger.

---

## 2. Capa: DataAccess

**Propósito:** Encapsular toda la lógica relacionada con la base de datos y la persistencia física de los objetos.

**Explicación Detallada:**
Esta es la capa de más bajo nivel en la arquitectura. Utiliza **Entity Framework Core** como ORM para mapear las clases de C# a tablas de PostgreSQL. Su responsabilidad principal es gestionar el ciclo de vida de la conexión a la base de datos, definir el esquema mediante el modelo de objetos y proporcionar una abstracción de acceso a datos mediante el patrón Repositorio. Aquí se definen las restricciones de integridad, relaciones complejas (como N:N para categorías o idiomas) y se ejecutan las consultas optimizadas hacia el motor de base de datos.

### Estructura de la Capa
```text
Microservicio.Atracciones.DataAccess
│
├── Context
│   └── AtraccionesDbContext.cs      # Configuración del DbContext y DbSets.
│
├── Entities                         # Clases POCO que representan las tablas de la BD.
│   ├── Atracciones
│   │   ├── AtraccionEntity.cs
│   │   ├── AtraccionIncluyeEntity.cs
│   │   ├── CategoriaAtraccionEntity.cs
│   │   ├── IdiomaAtraccionEntity.cs
│   │   └── ImagenAtraccionEntity.cs
│   ├── Auditoria
│   │   └── AuditoriaLogEntity.cs
│   ├── Catalogos
│   │   ├── CategoriaEntity.cs
│   │   ├── DestinoEntity.cs
│   │   ├── IdiomaEntity.cs
│   │   ├── ImagenEntity.cs
│   │   └── IncluyeEntity.cs
│   ├── Clientes
│   │   └── ClienteEntity.cs
│   ├── Facturacion
│   │   ├── DatosFacturacionEntity.cs
│   │   └── FacturaEntity.cs
│   ├── Reservas
│   │   ├── HorarioEntity.cs
│   │   ├── ReseniaEntity.cs
│   │   ├── ReservaDetalleEntity.cs
│   │   ├── ReservaEntity.cs
│   │   └── TicketEntity.cs
│   └── Seguridad
│       ├── RolEntity.cs
│       ├── UsuarioEntity.cs
│       └── UsuarioRolEntity.cs
│
├── Repositories                     # Implementación de patrones de acceso a datos.
│   ├── Interfaces
│   │   ├── IAtraccionRepository.cs
│   │   ├── IClienteRepository.cs
│   │   ├── IDestinoRepository.cs
│   │   ├── IFacturaRepository.cs
│   │   ├── IReseniaRepository.cs
│   │   ├── IReservaRepository.cs
│   │   ├── ITicketRepository.cs
│   │   └── IUsuarioRepository.cs
│   ├── AtraccionRepository.cs
│   ├── ClienteRepository.cs
│   ├── DestinoRepository.cs
│   ├── FacturaRepository.cs
│   ├── ReseniaRepository.cs
│   ├── ReservaRepository.cs
│   ├── TicketRepository.cs
│   └── UsuarioRepository.cs
│
├── Configurations                   # Mapeo Fluent API para configurar la BD.
│   ├── AtraccionConfiguration.cs
│   ├── AtraccionIncluyeConfiguration.cs
│   ├── AuditoriaLogConfiguration.cs
│   ├── CategoriaAtraccionConfiguration.cs
│   ├── CategoriaConfiguration.cs
│   ├── ClienteConfiguration.cs
│   ├── DatosFacturacionConfiguration.cs
│   ├── DestinoConfiguration.cs
│   ├── FacturaConfiguration.cs
│   ├── HorarioConfiguration.cs
│   ├── IdiomaAtraccionConfiguration.cs
│   ├── IdiomaConfiguration.cs
│   ├── ImagenAtraccionConfiguration.cs
│   ├── ImagenConfiguration.cs
│   ├── IncluyeConfiguration.cs
│   ├── ReseniaConfiguration.cs
│   ├── ReservaConfiguration.cs
│   ├── ReservaDetalleConfiguration.cs
│   ├── RolConfiguration.cs
│   ├── TicketConfiguration.cs
│   ├── UsuarioConfiguration.cs
│   └── UsuarioRolConfiguration.cs
│
├── Common
│   ├── PagedResult.cs
│   └── RepositoryBase.cs
│
└── Queries
    ├── AtraccionQueryRepository.cs
    ├── FacturaQueryRepository.cs
    ├── ReservaQueryRepository.cs
    └── TicketQueryRepository.cs
```

### Funcionalidad de las Clases
| Clase / Grupo | Funcionalidad |
| :--- | :--- |
| **AtraccionesDbContext** | Clase central que hereda de `DbContext`. Configura la conexión, registra los mapeos y gestiona el seguimiento de cambios (Change Tracking). |
| **Entities (\*Entity)** | Representan las tablas de la base de datos como objetos C#. Contienen las propiedades de datos y llaves foráneas. |
| **Configurations (\*Configuration)** | Implementan `IEntityTypeConfiguration`. Definen mediante Fluent API los nombres de tablas, tipos de datos, llaves primarias y relaciones (1:N, N:N). |
| **Repositories (\*Repository)** | Contienen la lógica de consulta (LINQ) para acceder a los datos. Heredan de `RepositoryBase` para tener operaciones CRUD genéricas. |
| **Interfaces (I\*Repository)** | Definen el contrato de los repositorios, permitiendo la Inversión de Control (IoC) y facilitando las pruebas unitarias. |
| **RepositoryBase** | Clase genérica que implementa los métodos estándar (Add, Update, Delete, GetById) para evitar duplicidad de código. |
| **Queries (\*QueryRepository)** | Repositorios especializados en consultas de lectura complejas o reportes que no requieren seguimiento de cambios. |

---

## 3. Capa: DataManagement

**Propósito:** Proporcionar una abstracción limpia y servicios de datos para la capa de negocio, gestionando transacciones y modelos internos.

**Explicación Detallada:**
Esta capa actúa como un mediador de persistencia. Su función es evitar que la lógica de negocio dependa directamente del ORM o de las entidades físicas de la base de datos. Implementa el patrón **Unit of Work** para asegurar que las operaciones que involucran múltiples repositorios se ejecuten de forma atómica (todo o nada). Además, maneja los **Modelos de Datos**, que son versiones "limpias" de las entidades, preparadas con la información necesaria para que la capa de negocio trabaje de forma fluida.

### Estructura de la Capa
```text
Microservicio.Atracciones.DataManagement
│
├── Interfaces                       # Contratos de los servicios de gestión de datos.
│   ├── IAtraccionDataService.cs
│   ├── IAuditoriaLogDataService.cs
│   ├── ICategoriaDataService.cs
│   ├── IClienteDataService.cs
│   ├── IDestinoDataService.cs
│   ├── IFacturaDataService.cs
│   ├── IIdiomaDataService.cs
│   ├── IImagenDataService.cs
│   ├── IIncluyeDataService.cs
│   ├── IReseniaDataService.cs
│   ├── IReservaDataService.cs
│   ├── ITicketDataService.cs
│   ├── IUnitOfWork.cs
│   └── IUsuarioDataService.cs
│
├── Models                           # Modelos de datos enriquecidos para la lógica interna.
│   ├── Atracciones
│   │   ├── AtraccionDataModel.cs
│   │   ├── AtraccionFiltroDataModel.cs
│   │   ├── AtraccionIncluyeDataModel.cs
│   │   ├── CategoriaAtraccionDataModel.cs
│   │   ├── IdiomaAtraccionDataModel.cs
│   │   └── ImagenAtraccionDataModel.cs
│   ├── Auditoria
│   │   └── AuditoriaLogDataModel.cs
│   ├── Catalogos
│   │   ├── CategoriaDataModel.cs
│   │   ├── DestinoDataModel.cs
│   │   ├── IdiomaDataModel.cs
│   │   ├── ImagenDataModel.cs
│   │   └── IncluyeDataModel.cs
│   ├── Clientes
│   │   └── ClienteDataModel.cs
│   ├── Facturacion
│   │   ├── DatosFacturacionDataModel.cs
│   │   └── FacturaDataModel.cs
│   ├── Reservas
│   │   ├── HorarioDataModel.cs
│   │   ├── ReseniaDataModel.cs
│   │   ├── ReservaDataModel.cs
│   │   ├── ReservaDetalleDataModel.cs
│   │   ├── ReservaFiltroDataModel.cs
│   │   └── TicketDataModel.cs
│   ├── Seguridad
│   │   ├── LoginDataModel.cs
│   │   ├── RolDataModel.cs
│   │   └── UsuarioDataModel.cs
│   └── Common
│       └── DataPagedResult.cs
│
├── Services                         # Orquestación de repositorios.
│   ├── AtraccionDataService.cs
│   ├── AuditoriaLogDataService.cs
│   ├── CategoriaDataService.cs
│   ├── ClienteDataService.cs
│   ├── DestinoDataService.cs
│   ├── FacturaDataService.cs
│   ├── IdiomaDataService.cs
│   ├── ImagenDataService.cs
│   ├── IncluyeDataService.cs
│   ├── ReseniaDataService.cs
│   ├── ReservaDataService.cs
│   ├── TicketDataService.cs
│   ├── UnitOfWork.cs
│   └── UsuarioDataService.cs
│
├── Mappers                          # Mapeo entre Entities y Modelos de Datos.
│   ├── Atracciones
│   │   └── AtraccionDataMapper.cs
│   ├── Auditoria
│   │   └── AuditoriaLogDataMapper.cs
│   ├── Catalogos
│   │   └── DestinoDataMapper.cs
│   ├── Clientes
│   │   └── ClienteDataMapper.cs
│   ├── Facturacion
│   │   └── FacturaDataMapper.cs
│   ├── Reservas
│   │   ├── ReseniaDataMapper.cs
│   │   ├── ReservaDataMapper.cs
│   │   └── TicketDataMapper.cs
│   └── Seguridad
│       ├── RolDataMapper.cs
│       └── UsuarioDataMapper.cs
│
└── Common
    └── Constants
        └── DataManagementConstants.cs
```

### Funcionalidad de las Clases
| Clase / Grupo | Funcionalidad |
| :--- | :--- |
| **UnitOfWork** | Gestiona la transacción de la base de datos. Asegura que todos los cambios realizados en distintos servicios se confirmen (`Commit`) o se cancelen (`Rollback`) juntos. |
| **DataServices (\*DataService)** | Implementan la lógica de orquestación de datos. Realizan llamadas a repositorios, aplican conversiones mediante mappers y devuelven Modelos de Datos. |
| **Models (\*DataModel)** | Estructuras de datos internas que transportan información entre DataManagement y Business. Están desacopladas del motor de persistencia. |
| **Mappers (\*DataMapper)** | Contienen la lógica para transformar una `Entity` (DB) en un `DataModel` (Interno) y viceversa. |
| **Interfaces (I\*DataService)** | Definen los contratos de los servicios de datos para que la capa Business pueda consumirlos mediante inyección de dependencias. |
| **DataPagedResult** | Clase genérica para manejar resultados paginados, incluyendo metadatos de total de registros y página actual. |

---

## 4. Capa: Business

**Propósito:** Implementar la lógica de negocio pura, reglas de dominio y orquestación de servicios funcionales.

**Explicación Detallada:**
Esta es la capa más importante del microservicio. Contiene el "quien hace que" del sistema. Está diseñada para ser independiente de cómo se guardan los datos (DataAccess) o cómo se exponen (Api). Se divide en servicios orientados a diferentes actores (**Admin** para administración, **Public** para clientes). Utiliza **Validators** para asegurar que los datos de entrada sean correctos, **Rules** para validar estados complejos (como si hay cupos para una reserva) y **Mappers** para entregar DTOs limpios a la capa de API.

### Estructura de la Capa
```text
Microservicio.Atracciones.Business
│
├── DTOs                             # Objetos para enviar/recibir datos por la API.
│   ├── Admin
│   │   ├── Atracciones
│   │   │   ├── ActualizarAtraccionRequest.cs
│   │   │   ├── AtraccionAdminFiltroRequest.cs
│   │   │   ├── AtraccionAdminResponse.cs
│   │   │   └── CrearAtraccionRequest.cs
│   │   ├── Catalogos
│   │   │   ├── CatalogoRequests.cs
│   │   │   ├── CategoriaResponse.cs
│   │   │   ├── IdiomaResponse.cs
│   │   │   └── IncluyeResponse.cs
│   │   ├── Clientes
│   │   │   ├── ActualizarClienteRequest.cs
│   │   │   ├── ClienteFiltroRequest.cs
│   │   │   ├── ClienteResponse.cs
│   │   │   └── CrearClienteRequest.cs
│   │   ├── Destinos
│   │   │   ├── ActualizarDestinoRequest.cs
│   │   │   ├── CrearDestinoRequest.cs
│   │   │   └── DestinoResponse.cs
│   │   ├── Facturas
│   │   │   ├── FacturaResponse.cs
│   │   │   └── GenerarFacturaRequest.cs
│   │   ├── Imagenes
│   │   │   └── ImagenRequests.cs
│   │   ├── Resenias
│   │   │   ├── ActualizarReseniaRequest.cs
│   │   │   └── ReseniaAdminResponse.cs
│   │   ├── Reservas
│   │   │   ├── ActualizarEstadoReservaRequest.cs
│   │   │   ├── ReservaAdminFiltroRequest.cs
│   │   │   ├── ReservaAdminResponse.cs
│   │   │   └── ReservaDetalleAdminResponse.cs
│   │   ├── Tickets
│   │   │   ├── ActualizarHorarioRequest.cs
│   │   │   ├── ActualizarTicketRequest.cs
│   │   │   ├── CrearHorarioRequest.cs
│   │   │   ├── CrearTicketRequest.cs
│   │   │   ├── HorarioResponse.cs
│   │   │   └── TicketResponse.cs
│   │   └── Usuarios
│   │       ├── ActualizarUsuarioRequest.cs
│   │       ├── CrearUsuarioRequest.cs
│   │       └── UsuarioResponse.cs
│   ├── Auth
│   │   ├── LoginRequest.cs
│   │   ├── LoginResponse.cs
│   │   ├── RegistroClienteRequest.cs
│   │   └── UsuarioAutenticadoDto.cs
│   └── Public
│       ├── Atracciones
│       │   ├── AtraccionDetalleResponse.cs
│       │   ├── AtraccionFiltroRequest.cs
│       │   ├── AtraccionListadoResponse.cs
│       │   ├── DisponibilidadResponse.cs
│       │   ├── FiltrosAtraccionResponse.cs
│       │   ├── HorarioProximoResponse.cs
│       │   └── TicketDisponibleResponse.cs
│       ├── Clientes
│       │   ├── ActualizarPerfilClienteRequest.cs
│       │   └── PerfilClienteResponse.cs
│       ├── Resenias
│       │   ├── CrearReseniaRequest.cs
│       │   └── ReseniaResponse.cs
│       └── Reservas
│           ├── CancelarReservaRequest.cs
│           ├── ConfirmarPagoReservaRequest.cs
│           ├── CrearReservaRequest.cs
│           ├── ReservaDetalleRequest.cs
│           ├── ReservaDetalleResponse.cs
│           └── ReservaResponse.cs
│
├── Services                         # Implementación de lógica de negocio.
│   ├── Admin
│   │   ├── AtraccionAdminService.cs
│   │   ├── CatalogoAdminService.cs
│   │   ├── ClienteAdminService.cs
│   │   ├── DestinoAdminService.cs
│   │   ├── FacturaAdminService.cs
│   │   ├── ImagenAdminService.cs
│   │   ├── ReseniaAdminService.cs
│   │   ├── ReservaAdminService.cs
│   │   ├── TicketAdminService.cs
│   │   └── UsuarioAdminService.cs
│   ├── Auth
│   │   └── AuthService.cs
│   └── Public
│       ├── AtraccionPublicService.cs
│       ├── ClientePerfilService.cs
│       ├── FacturaPublicService.cs
│       ├── ReseniaPublicService.cs
│       └── ReservaPublicService.cs
│
├── Interfaces
│   ├── Admin
│   │   ├── IAtraccionAdminService.cs
│   │   ├── ICatalogoAdminService.cs
│   │   ├── IClienteAdminService.cs
│   │   ├── IDestinoAdminService.cs
│   │   ├── IFacturaAdminService.cs
│   │   ├── IImagenAdminService.cs
│   │   ├── IReseniaAdminService.cs
│   │   ├── IReservaAdminService.cs
│   │   ├── ITicketAdminService.cs
│   │   └── IUsuarioAdminService.cs
│   ├── Auth
│   │   └── IAuthService.cs
│   └── Public
│       ├── IAtraccionPublicService.cs
│       ├── IClientePerfilService.cs
│       ├── IFacturaPublicService.cs
│       ├── IReseniaPublicService.cs
│       └── IReservaPublicService.cs
│
├── Rules
│   ├── Admin
│   │   ├── AtraccionRules.cs
│   │   ├── FacturaRules.cs
│   │   ├── ReservaAdminRules.cs
│   │   └── TicketRules.cs
│   └── Public
│       ├── ReseniaRules.cs
│       └── ReservaRules.cs
│
├── Validators
│   ├── Admin
│   │   ├── AtraccionAdminValidator.cs
│   │   ├── ClienteAdminValidator.cs
│   │   ├── DestinoAdminValidator.cs
│   │   ├── FacturaAdminValidator.cs
│   │   ├── ReseniaAdminValidator.cs
│   │   ├── ReservaAdminValidator.cs
│   │   ├── TicketAdminValidator.cs
│   │   └── UsuarioAdminValidator.cs
│   ├── Public
│   │   ├── AtraccionPublicValidator.cs
│   │   ├── ReseniaPublicValidator.cs
│   │   └── ReservaPublicValidator.cs
│   └── AuthValidator.cs
│
├── Mappers
│   ├── Admin
│   │   ├── AtraccionAdminMapper.cs
│   │   ├── ClienteAdminMapper.cs
│   │   ├── DestinoAdminMapper.cs
│   │   ├── FacturaAdminMapper.cs
│   │   ├── ReseniaAdminMapper.cs
│   │   ├── ReservaAdminMapper.cs
│   │   ├── TicketAdminMapper.cs
│   │   └── UsuarioAdminMapper.cs
│   ├── Public
│   │   ├── AtraccionPublicMapper.cs
│   │   ├── ReseniaPublicMapper.cs
│   │   └── ReservaPublicMapper.cs
│   └── AuthBusinessMapper.cs
│
└── Exceptions
    ├── BusinessException.cs
    ├── ConflictException.cs
    ├── ForbiddenBusinessException.cs
    ├── NotFoundException.cs
    ├── UnauthorizedBusinessException.cs
    └── ValidationException.cs
```

### Funcionalidad de las Clases
| Clase / Grupo | Funcionalidad |
| :--- | :--- |
| **AdminServices (\*AdminService)** | Servicios dedicados a las operaciones de backoffice. Gestión de atracciones, tickets, usuarios y reportes administrativos. |
| **PublicServices (\*PublicService)** | Servicios para el cliente final. Procesamiento de reservas públicas, consulta de catálogo y gestión de perfil de usuario. |
| **AuthService** | Gestiona el inicio de sesión, registro de nuevos clientes y la validación de credenciales. |
| **Rules (\*Rules)** | Contienen lógica de decisión compleja. Por ejemplo, `ReservaRules` verifica si una fecha es válida y si hay cupos disponibles. |
| **Validators (\*Validator)** | Implementan reglas de validación de formato (ej: correos válidos, campos obligatorios) usando `FluentValidation`. |
| **DTOs (\*Request / \*Response)** | Objetos de Transferencia de Datos. Definen la estructura exacta de lo que viaja por la red hacia/desde el frontend. |
| **Mappers (\*Mapper)** | Transforman los `DataModels` internos en `DTOs` de respuesta, ocultando IDs internos y exponiendo GUIDs. |
| **Exceptions (\*Exception)** | Definición de errores de negocio personalizados (404, 403, 409) para ser capturados por el middleware de la API. |

---

## 5. Capa: Api

**Propósito:** Interfaz de comunicación externa, manejo de protocolos HTTP y preocupaciones transversales.

**Explicación Detallada:**
Esta capa es el punto de entrada al microservicio. Su responsabilidad es recibir las peticiones HTTP, autenticar a los usuarios mediante **JWT (JSON Web Tokens)** con soporte para `usu_guid` y `cli_id`, validar que los datos cumplan con el esquema básico y delegar la ejecución a la capa de negocio. Implementa políticas de autorización basadas en roles (**SoloAdmin**, **ClienteAutenticado**, **AdminOCliente**) y maneja el **Manejo Global de Excepciones** para asegurar que el cliente siempre reciba una respuesta estructurada (JSON). Además, configura la caché de respuestas y la documentación automática con **Swagger**.

### Estructura de la Capa
```text
Microservicio.Atracciones.Api
│
├── Controllers                      # Endpoints organizados por versiones y módulos.
│   └── V1
│       ├── Internal
│       │   ├── AtraccionesAdminController.cs
│       │   ├── CatalogosAdminController.cs
│       │   ├── DestinosController.cs
│       │   ├── ImagenesController.cs
│       │   ├── ReseniasAdminController.cs
│       │   ├── ReseniasController.cs
│       │   └── UsuariosController.cs
│       ├── Booking
│       │   ├── AtraccionesController.cs
│       │   ├── ClientesController.cs
│       │   ├── ClientesPerfilController.cs
│       │   ├── FacturasController.cs
│       │   ├── FacturasPublicController.cs
│       │   ├── ReservasAdminController.cs
│       │   ├── ReservasController.cs
│       │   ├── TicketsController.cs
│       │   └── TicketsPublicController.cs
│       └── Auth
│           └── AuthController.cs
│
├── Middleware
│   └── ExceptionHandlingMiddleware.cs
│
├── Filters
│   └── ValidateModelFilter.cs
│
├── Helpers
│   ├── CacheProfileNames.cs
│   ├── EndpointNames.cs
│   ├── LinkBuilder.cs
│   └── SorterFactory.cs
│
├── Extensions
│   ├── ApiVersioningExtensions.cs
│   ├── AuthenticationExtensions.cs
│   ├── AuthorizationExtensions.cs
│   ├── AuthorizeOperationFilter.cs
│   ├── CorsExtensions.cs
│   ├── ResponseCachingExtensions.cs
│   ├── ServiceCollectionExtensions.cs
│   └── SwaggerExtensions.cs
│
├── Mappers
│   ├── Admin
│   │   ├── AtraccionesAdminApiMapper.cs
│   │   ├── AuthApiMapper.cs
│   │   ├── ClientesApiMapper.cs
│   │   ├── DestinosApiMapper.cs
│   │   ├── FacturasApiMapper.cs
│   │   ├── ReseniasAdminApiMapper.cs
│   │   ├── ReservasAdminApiMapper.cs
│   │   ├── TicketsApiMapper.cs
│   │   └── UsuariosApiMapper.cs
│   └── Public
│       ├── AtraccionesApiMapper.cs
│       ├── ReseniasApiMapper.cs
│       └── ReservasApiMapper.cs
│
├── Models
│   ├── Common
│   │   ├── ApiErrorResponse.cs
│   │   ├── ApiItemResponse.cs
│   │   ├── ApiListResponse.cs
│   │   ├── FilterStatsResponse.cs
│   │   ├── LinksResponse.cs
│   │   ├── PaginationResponse.cs
│   │   └── SorterResponse.cs
│   └── Settings
│       ├── ApiSettings.cs
│       ├── CacheSettings.cs
│       ├── CorsSettings.cs
│       └── JwtSettings.cs
│
├── Services
│   ├── BcryptPasswordHasher.cs
│   ├── JwtTokenService.cs
│   └── TokenService.cs
│
└── Program.cs                       # Configuración de servicios y pipeline de ASP.NET Core.
```

### Funcionalidad de las Clases
| Clase / Grupo | Funcionalidad |
| :--- | :--- |
| **Controllers** | Clases que heredan de `ControllerBase`. Definen las rutas de la API, los verbos HTTP (GET, POST, etc.) y los códigos de respuesta. |
| **ExceptionHandlingMiddleware** | Intercepta cualquier error no controlado para retornar una respuesta `ApiErrorResponse` con formato estándar. |
| **Extensions (\*Extensions)** | Separan la configuración de `Program.cs` en métodos manejables (ej: configuración de Swagger, JWT, CORS). |
| **ApiMappers (\*ApiMapper)** | Realizan mapeos finales específicos para la API, como añadir enlaces HATEOAS o ajustar formatos de fecha. |
| **Models (Api\*)** | Estructuras de respuesta genéricas para asegurar que todas las respuestas de la API tengan la misma forma (`Data`, `Message`, `Code`). |
| **JwtTokenService** | Genera y valida los tokens de seguridad para los usuarios autenticados. |
| **Program.cs** | Punto de entrada que configura el servidor Kestrel, los servicios de inyección de dependencias y el middleware. |
| **Helpers (\*Names / Sorter)** | Centralizan nombres de perfiles de caché, nombres de endpoints y lógica de ordenamiento dinámico. |

---

## 6. Listado de Endpoints

### Endpoints Administrativos (Requieren Rol: Admin)
| Método | Endpoint | Funcionalidad Breve |
| :--- | :--- | :--- |
| `GET` | `/api/v1/admin/atracciones` | Lista atracciones con filtros detallados para gestión. |
| `POST` | `/api/v1/admin/atracciones` | Crea una nueva atracción con sus imágenes, idiomas y categorías. |
| `PUT` | `/api/v1/admin/atracciones/{guid}` | Actualiza todos los datos de una atracción existente. |
| `DELETE` | `/api/v1/admin/atracciones/{guid}` | Elimina de forma lógica una atracción. |
| `GET` | `/api/v1/admin/categorias` | Lista todas las categorías maestras. |
| `POST` | `/api/v1/admin/categorias` | Crea una nueva categoría (soporta jerarquías). |
| `PUT` | `/api/v1/admin/categorias/{guid}` | Actualiza el nombre o padre de una categoría. |
| `DELETE` | `/api/v1/admin/categorias/{guid}` | Elimina de forma lógica una categoría. |
| `GET` | `/api/v1/admin/idiomas` | Lista todos los idiomas disponibles. |
| `POST` | `/api/v1/admin/idiomas` | Registra un nuevo idioma. |
| `PUT` | `/api/v1/admin/idiomas/{guid}` | Actualiza la descripción de un idioma. |
| `DELETE` | `/api/v1/admin/idiomas/{guid}` | Elimina un idioma del catálogo. |
| `GET` | `/api/v1/admin/incluye` | Lista los ítems de inclusión (lo que incluye la entrada). |
| `POST` | `/api/v1/admin/incluye` | Crea un nuevo ítem de inclusión. |
| `GET` | `/api/v1/admin/reservas` | Consulta el historial global de reservas realizadas. |

### Endpoints Públicos (Acceso Abierto / Cliente)
| Método | Endpoint | Funcionalidad Breve |
| :--- | :--- | :--- |
| `GET` | `/api/v1/atracciones` | Listado paginado de atracciones con filtros avanzados (ciudad, calificacion, etc). |
| `GET` | `/api/v1/atracciones/filtros` | Obtiene opciones de filtrado dinámico y estadísticas según la ciudad. |
| `GET` | `/api/v1/atracciones/{guid}` | Detalle completo de una atracción específica. |
| `GET` | `/api/v1/atracciones/{guid}/tickets` | Lista los tipos de tickets disponibles para una atracción. |
| `GET` | `/api/v1/atracciones/{guid}/horarios-disponibles` | Consulta los próximos horarios con cupos para la atracción. |
| `GET` | `/api/v1/reservas` | Historial de reservas del cliente autenticado. |
| `POST` | `/api/v1/reservas` | Crea una reserva (cabecera + detalle) con descuento de cupos. |
| `GET` | `/api/v1/reservas/{guid}` | Obtiene el detalle de una reserva específica. |
| `POST` | `/api/v1/reservas/{guid}/confirmar-pago` | Procesa el pago y genera la factura legal (IVA 15%). |
| `PUT` | `/api/v1/reservas/{guid}/cancelar` | Cancela una reserva pendiente y libera cupos. |
| `GET` | `/api/v1/resenias?atraccionGuid={guid}` | Lista reseñas públicas de una atracción. |
| `POST` | `/api/v1/resenias` | Permite a un cliente calificar una atracción tras su visita. |
| `POST` | `/api/v1/auth/login` | Autenticación de usuarios para obtener token JWT. |


