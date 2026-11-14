/* =====================================================================
   SúperTico - Procedimientos base (Justin)
   Inicio de sesión y bitácora de errores.
   Convención: sp_Entidad_Accion. Se puede ejecutar varias veces.
   ===================================================================== */

USE [SuperTico_BD]
GO

/* CU-02: devuelve el usuario por correo (activo o no) con su rol.
   La contraseña se compara en la API contra el hash, por eso se devuelve. */
CREATE OR ALTER PROCEDURE [dbo].[sp_Usuario_ConsultarPorCorreo]
	@Correo		varchar(150)
AS
BEGIN

	SELECT	U.UsuarioId,
			U.NombreUsuario,
			U.Correo,
			U.Contrasena,
			U.RolId,
			R.Nombre AS NombreRol,
			U.Activo
	FROM	dbo.Usuario U
	INNER JOIN dbo.Rol R ON R.RolId = U.RolId
	WHERE	U.Correo = @Correo

END
GO

/* Registro de errores: lo usa el middleware de la API y el ErrorController de la WEB */
CREATE OR ALTER PROCEDURE [dbo].[sp_BitacoraError_Registrar]
	@Origen		varchar(20),
	@Ruta		varchar(300),
	@Mensaje	varchar(max),
	@Detalle	varchar(max),
	@UsuarioId	int
AS
BEGIN

	INSERT INTO dbo.BitacoraError(Origen,Ruta,Mensaje,Detalle,UsuarioId)
	VALUES(@Origen,@Ruta,@Mensaje,@Detalle,@UsuarioId)

END
GO
