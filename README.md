# SúperTico – SC-701 Programación Avanzada en Web (Grupo 4)

Supermercado ficticio con pedidos para retiro. ASP.NET Core 10 (MVC + Web API), SQL Server, Dapper con procedimientos almacenados y JWT.

## Estructura

| Carpeta | Contenido |
|---|---|
| `BD/` | `01_SuperTico_BD.sql` (tablas y datos de prueba) + un archivo `SP_*.sql` por integrante |
| `Proyecto-API/` | Web API (`https://localhost:7118`, documentación en `/scalar/v1`) |
| `Proyecto-WEB/` | Aplicación MVC con Razor (`https://localhost:7018`) |

## Cómo levantarlo

1. **Base de datos:** en SSMS ejecute `BD/01_SuperTico_BD.sql` y luego **todos** los `BD/SP_*.sql`.
   (El 01 borra y recrea la base; los SP se pueden ejecutar las veces que quiera.)
2. **Cadena de conexión:** la API usa `Server=localhost`. Si su SQL Server tiene otro nombre (por ejemplo `localhost\SQL2025` o `localhost\SQLEXPRESS`), cree el archivo
   `Proyecto-API/appsettings.Local.json` (**no se sube a GitHub**):
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost\\SQLEXPRESS; Database=SuperTico_BD; Trusted_Connection=True; TrustServerCertificate=True;"
     }
   }
   ```
   Las credenciales del correo (`Correo:Cuenta` y `Correo:Clave`) también van en ese archivo.
3. Ejecute **primero la API** y después la WEB.

Usuarios de prueba: `admin@supertico.com` / `Admin123` (Administrador) y `cliente@supertico.com` / `Cliente123` (Cliente).

## Lo que ya da la base (úsenlo, no lo dupliquen)

- **Contraseñas:** `ISeguridadService.GenerarHash(...)` y `VerificarHash(...)` en la API (PBKDF2). Nunca guarden contraseñas en texto plano.
- **Endpoints protegidos (API):** pongan `[Authorize]` o `[Authorize(Roles = "Administrador")]`. El id del usuario se obtiene con `User.FindFirstValue(ClaimTypes.NameIdentifier)`.
- **Llamar a la API con token (WEB):** `client.AgregarToken(HttpContext);` antes del `PostAsJsonAsync`/`GetAsync`.
- **Proteger pantallas (WEB):** `[SesionActiva]` (cualquier usuario) o `[SesionActiva(Rol = SesionHelper.RolAdministrador)]`.
- **Datos del usuario en la WEB:** `HttpContext.Session.GetInt32(SesionHelper.UsuarioId)`, `EstaAutenticado()`, `EsAdministrador()`.
- **Errores:** no hace falta `try/catch`. El middleware de la API y el `ErrorController` de la WEB guardan todo en la tabla `BitacoraError`.

## Estándar de programación

- Nombres en español y PascalCase; las interfaces empiezan con `I` (`ICategoriaService`).
- Procedimientos almacenados con el formato `sp_Entidad_Accion` (`sp_Producto_Listar`, `sp_Categoria_Registrar`), con `CREATE OR ALTER`.
- API: `[Route("api/[controller]")]` + `[Route("Accion")]`, Dapper con `DynamicParameters`. WEB: `#region` por funcionalidad, `[ValidateAntiForgeryToken]` en los POST, y validaciones con `[Required(ErrorMessage = "...")]`.
- Comentarios `///` en las clases y en los métodos que no sean obvios.
- Trabajo en ramas `feature/...` y Pull Request a `main`; nadie hace commit directo a `main`.
