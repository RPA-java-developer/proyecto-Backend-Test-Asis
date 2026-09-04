# Backend .NET 8

Backend desarrollado con **.NET 8 y ASP.NET Core Web API**, organizado bajo principios de separación de responsabilidades, bajo acoplamiento y mantenibilidad.

## Arquitectura

El proyecto utiliza una estructura organizada por responsabilidades. El objetivo es mantener el código **escalable, comprobable, mantenible y adaptable** a las necesidades cambiantes del negocio.

La arquitectura busca:

* Desacoplar las reglas de negocio de la infraestructura.
* Facilitar las pruebas unitarias, de integración y de extremo a extremo.
* Reducir la deuda técnica mediante límites claros.
* Facilitar la incorporación de nuevas funcionalidades.
* Mantener una estructura sencilla y fácil de comprender.

### Beneficios

| Característica          | Descripción                                                                                  |
| ----------------------- | -------------------------------------------------------------------------------------------- |
| **Escalabilidad**       | Facilita incorporar nuevas funcionalidades sin afectar innecesariamente el código existente. |
| **Capacidad de prueba** | Permite probar la lógica de aplicación de forma aislada.                                     |
| **Mantenibilidad**      | Una estructura clara facilita la evolución del código.                                       |
| **Desacoplamiento**     | Reduce dependencias innecesarias entre componentes.                                          |
| **Adaptabilidad**       | Permite evolucionar componentes tecnológicos de forma controlada.                            |

---

## Estructura del proyecto

```text
Backend/
│
├── Controllers/
│
├── Data/
│
├── DTOs/
│
├── Models/
│
├── Services/
│
├── Middleware/
│
├── Extensions/
│
├── Validators/
│
├── Program.cs
│
├── appsettings.json
├── appsettings.Development.json
│
└── Backend.csproj
```

La estructura está diseñada para mantener cada responsabilidad en su lugar y evitar que una única clase concentre demasiada lógica.

---

## Controllers

Los `Controllers` representan la **puerta de entrada HTTP** de la aplicación.

Responsabilidades principales:

* Recibir solicitudes HTTP.
* Recibir parámetros y DTOs.
* Delegar operaciones a los Services.
* Devolver respuestas HTTP.

```text
HTTP Request
     │
     ▼
Controller
     │
     ▼
Service
```

Los Controllers deben mantenerse delgados y evitar reglas de negocio complejas.

---

## Services

Los `Services` contienen la **lógica de aplicación**.

Aquí se concentran:

* Reglas de negocio.
* Procesamiento de información.
* Validaciones.
* Orquestación de operaciones.
* Coordinación con acceso a datos.
* Integraciones con otros servicios.

Una organización recomendada:

```text
Services/
│
├── Interfaces/
│   ├── IUserService.cs
│   └── IAuthService.cs
│
├── UserService.cs
└── AuthService.cs
```

El uso de interfaces facilita **Dependency Injection** y las pruebas unitarias.

---

## Data

`Data` concentra las responsabilidades relacionadas con la **persistencia de información**.

```text
Data/
│
├── AppDbContext.cs
│
├── Configurations/
│
├── Repositories/
│
└── Migrations/
```

### DbContext

`AppDbContext` representa la interacción con la base de datos mediante **Entity Framework Core**.

### Configurations

Contiene la configuración de entidades, relaciones, restricciones y claves.

### Repositories

Encapsulan las operaciones de acceso a datos.

```text
IUserRepository
       ▲
       │
UserRepository
       │
       ▼
Entity Framework Core
       │
       ▼
Database
```

Esto permite que los Services no tengan que conocer detalles innecesarios de persistencia.

---

## Models

`Models` contiene los modelos y entidades principales utilizados por la aplicación.

```text
Models/
│
├── User.cs
├── Role.cs
└── ...
```

Los Models representan conceptos relevantes del sistema.

Se recomienda evitar utilizar directamente estas entidades como contrato público de la API.

---

## DTOs

Los `DTOs (Data Transfer Objects)` representan los objetos utilizados para la comunicación entre la API y sus consumidores.

Una organización por funcionalidad facilita su mantenimiento:

```text
DTOs/
│
├── Auth/
│   ├── LoginRequest.cs
│   └── LoginResponse.cs
│
├── Users/
│   ├── CreateUserRequest.cs
│   ├── UpdateUserRequest.cs
│   └── UserResponse.cs
│
└── Common/
    ├── ApiResponse.cs
    └── PagedResponse.cs
```

Los DTOs permiten controlar qué información entra y sale de la API.

### Flujo de entrada

```text
Request DTO
     │
     ▼
Controller
     │
     ▼
Service
     │
     ▼
Model
     │
     ▼
Database
```

### Flujo de salida

```text
Database
     │
     ▼
Model
     │
     ▼
Service
     │
     ▼
Response DTO
     │
     ▼
Controller
     │
     ▼
HTTP Response
```

---

## Middleware

`Middleware` contiene componentes que procesan las solicitudes HTTP de forma transversal.

Una responsabilidad especialmente útil es el manejo global de excepciones:

```text
Client
  │
  ▼
Middleware
  │
  ├── Exception Handling
  ├── Logging
  └── Request Processing
  │
  ▼
Controller
```

Esto evita repetir lógica de manejo de errores en cada Controller.

---

## Validators

Los `Validators` concentran las validaciones de entrada.

Ejemplo:

```text
CreateUserRequest
       │
       ▼
CreateUserValidator
       │
       ├── Campos obligatorios
       ├── Formato de email
       ├── Longitud
       └── Reglas de validación
```

Esto mantiene los Controllers limpios y facilita las pruebas.

---

## Extensions

`Extensions` permite mantener organizado `Program.cs`.

Por ejemplo:

```text
Extensions/
│
├── ServiceCollectionExtensions.cs
├── AuthenticationExtensions.cs
└── SwaggerExtensions.cs
```

La configuración puede dividirse en métodos de extensión para evitar que `Program.cs` crezca innecesariamente.

---

# Flujo general

El flujo principal de una solicitud puede representarse de forma sencilla:

```text
                    CLIENT
                       │
                       │ HTTP
                       ▼
               ┌──────────────┐
               │ Controllers  │
               └──────┬───────┘
                      │
                      ▼
               ┌──────────────┐
               │   Services   │
               └──────┬───────┘
                      │
                      ▼
               ┌──────────────┐
               │     Data     │
               └──────┬───────┘
                      │
                      ▼
               ┌──────────────┐
               │   Database   │
               └──────────────┘
```

Los `DTOs` y `Models` participan en el intercambio de información entre estas capas.

---

# Principios de diseño

El proyecto busca aplicar principios fundamentales de desarrollo de software:

| Principio                  | Aplicación                                                          |
| -------------------------- | ------------------------------------------------------------------- |
| **Single Responsibility**  | Cada componente tiene una responsabilidad definida.                 |
| **Dependency Injection**   | Las dependencias se administran mediante inyección de dependencias. |
| **Separation of Concerns** | Las responsabilidades están separadas por componentes.              |
| **Low Coupling**           | Se reducen dependencias directas e innecesarias.                    |
| **High Cohesion**          | Cada componente agrupa responsabilidades relacionadas.              |
| **Testability**            | La lógica puede probarse de forma independiente.                    |
| **Maintainability**        | La estructura facilita el mantenimiento.                            |
| **Scalability**            | La solución puede crecer progresivamente.                           |

---

# Evolución de la arquitectura

La estructura actual está pensada para mantener el proyecto **simple y práctico**.

Si el sistema aumenta considerablemente su complejidad, puede evolucionar hacia una separación física por proyectos:

```text
                 ┌──────────────┐
                 │     API      │
                 └──────┬───────┘
                        │
                        ▼
               ┌─────────────────┐
               │   Application   │
               └────────┬────────┘
                        │
                        ▼
                  ┌────────────┐
                  │   Domain   │
                  └────────────┘
                        ▲
                        │
               ┌────────┴────────┐
               │  Infrastructure │
               └─────────────────┘
```

Esta evolución debe realizarse cuando la **complejidad real del negocio** lo justifique, evitando introducir sobrearquitectura innecesaria.

---

# Regla fundamental

Una regla práctica para mantener saludable el proyecto:

```text
Controllers  → reciben y responden
Services     → procesan y aplican reglas
Data         → persiste y consulta
Models       → representan información
DTOs         → definen contratos de comunicación
Middleware   → resuelve preocupaciones transversales
Validators   → validan entradas
```

Si una clase comienza a asumir responsabilidades que pertenecen a otro componente, debe revisarse su diseño.

---

# Tecnologías

* **.NET 8**
* **ASP.NET Core Web API**
* **C#**
* **Entity Framework Core**
* **REST API**
* **Dependency Injection**
* **OpenAPI / Swagger**

---

# Requisitos

Para ejecutar el proyecto se requiere:

* .NET 8 SDK.
* Base de datos configurada.
* IDE compatible con .NET 8.

Verificar la versión instalada:

```bash
dotnet --version
```

---

# Instalación

Clonar el proyecto:

```bash
git clone <repository-url>
```

Ingresar al directorio:

```bash
cd Backend
```

Restaurar dependencias:

```bash
dotnet restore
```

Compilar:

```bash
dotnet build
```

Ejecutar:

```bash
dotnet run
```

---

# Configuración

La aplicación utiliza principalmente:

```text
appsettings.json
appsettings.Development.json
Environment Variables
```

Las credenciales, contraseñas, tokens y demás información sensible **no deben almacenarse directamente en el repositorio**.

Para desarrollo pueden utilizarse mecanismos como:

```text
.NET User Secrets
```

En ambientes productivos se recomienda utilizar variables de entorno o un sistema especializado de gestión de secretos.

---

# Testing

La arquitectura permite implementar diferentes niveles de pruebas:

```text
                 TESTING
                    │
          ┌─────────┼─────────┐
          ▼         ▼         ▼
      Unitarias  Integración  E2E
          │         │         │
          ▼         ▼         ▼
       Services   Data/API    API
```

Las pruebas unitarias deben concentrarse especialmente en la lógica de aplicación y reglas de negocio.

---

# Seguridad

La seguridad debe formar parte del diseño desde las primeras etapas.

Se recomienda considerar:

* Autenticación.
* Autorización.
* Validación de entradas.
* HTTPS.
* Configuración adecuada de CORS.
* Protección de secretos.
* Manejo centralizado de excepciones.
* Logging sin información sensible.
* Principio de mínimo privilegio.
* Respuestas HTTP controladas.

---

# Escalabilidad

La solución puede evolucionar progresivamente:

```text
                 Proyecto inicial
                       │
                       ▼
          Controllers / Services / Data
                       │
                       ▼
              Organización modular
                       │
                       ▼
           Separación por proyectos
                       │
                       ▼
        API / Application / Domain /
             Infrastructure
```

La arquitectura debe evolucionar de acuerdo con las necesidades reales del sistema.

---

# Conclusión

La estructura propuesta proporciona una base **simple, clara y profesional** para un backend desarrollado con ASP.NET Core 8.

El objetivo principal es mantener:

```text
Responsabilidades claras
          +
Bajo acoplamiento
          +
Código comprobable
          +
Mantenibilidad
          +
Evolución controlada
```

Una buena arquitectura no consiste en tener muchas capas, sino en establecer **límites correctos y responsabilidades bien definidas**.
