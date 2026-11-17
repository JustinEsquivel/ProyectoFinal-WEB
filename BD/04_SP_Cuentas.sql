/* =====================================================================
   SúperTico - Procedimientos de cuentas
   Registro de usuarios, recuperación y cambio de contraseña.
   Convención: sp_Entidad_Accion. Se puede ejecutar varias veces.
   ===================================================================== */

USE [SuperTico_BD]
GO

/* CU-01: registra un usuario nuevo con el rol Cliente.
   Si el correo ya existe no inserta nada y la API recibe 0 filas afectadas. */
CREATE OR ALTER PROCEDURE [dbo].[sp_Usuario_Registrar]
	@NombreUsuario	varchar(50),
	@Correo			varchar(150),
	@Contrasena		varchar(256)
AS
BEGIN

	DECLARE @RolCliente INT = (SELECT RolId FROM dbo.Rol WHERE Nombre = 'Cliente')

	IF NOT EXISTS(
		SELECT	1 FROM dbo.Usuario
		WHERE	Correo = @Correo)
	BEGIN

		INSERT INTO dbo.Usuario(NombreUsuario,Correo,Contrasena,RolId)
		VALUES(@NombreUsuario,@Correo,@Contrasena,@RolCliente)

	END
END
GO

/* CU-04 y CU-05: guarda el hash de la contraseña nueva de un usuario activo. */
CREATE OR ALTER PROCEDURE [dbo].[sp_Usuario_ActualizarContrasena]
	@UsuarioId		int,
	@Contrasena		varchar(256)
AS
BEGIN

	UPDATE	dbo.Usuario
	SET		Contrasena = @Contrasena
	WHERE	UsuarioId = @UsuarioId
		AND Activo = 1

END
GO
