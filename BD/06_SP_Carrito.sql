USE [SuperTico_BD]
GO

CREATE OR ALTER PROCEDURE dbo.sp_Carrito_Consultar
    @UsuarioId int
AS
BEGIN
    SET NOCOUNT ON;

    SELECT p.ProductoId, p.Nombre, p.Precio, p.ImagenUrl,
           p.Existencias, p.Activo, d.Cantidad
    FROM dbo.CarritoDetalle AS d
    INNER JOIN dbo.Producto AS p ON p.ProductoId = d.ProductoId
    WHERE d.CarritoId = (
        SELECT TOP (1) CarritoId FROM dbo.Carrito
        WHERE UsuarioId = @UsuarioId ORDER BY CarritoId
    )
    ORDER BY p.Nombre, p.ProductoId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Carrito_Guardar
    @UsuarioId int,
    @ProductoId int,
    @Cantidad int,
    @Sumar bit
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    DECLARE @UsuarioActivo bit;
    SELECT @UsuarioActivo = Activo
    FROM dbo.Usuario WITH (UPDLOCK, HOLDLOCK)
    WHERE UsuarioId = @UsuarioId;

    IF ISNULL(@UsuarioActivo, 0) = 0 OR @Cantidad <= 0
    BEGIN
        ROLLBACK TRANSACTION;
        SELECT N'Revise el usuario y la cantidad ingresada.';
        RETURN;
    END;

    DECLARE @CarritoId int;
    SELECT TOP (1) @CarritoId = CarritoId
    FROM dbo.Carrito WHERE UsuarioId = @UsuarioId ORDER BY CarritoId;

    DECLARE @CantidadActual int = 0;
    SELECT @CantidadActual = Cantidad FROM dbo.CarritoDetalle
    WHERE CarritoId = @CarritoId AND ProductoId = @ProductoId;

    IF @Sumar = 0 AND @CantidadActual = 0
    BEGIN
        ROLLBACK TRANSACTION;
        SELECT N'El producto ya no está en el carrito.';
        RETURN;
    END;

    DECLARE @NuevaCantidad bigint = @Cantidad;
    IF @Sumar = 1 SET @NuevaCantidad = @NuevaCantidad + @CantidadActual;

    DECLARE @Existencias int, @Activo bit;
    SELECT @Existencias = Existencias, @Activo = Activo
    FROM dbo.Producto WITH (HOLDLOCK)
    WHERE ProductoId = @ProductoId;

    IF ISNULL(@Activo, 0) = 0
    BEGIN
        ROLLBACK TRANSACTION;
        SELECT N'El producto ya no está disponible.';
        RETURN;
    END;

    IF @NuevaCantidad > @Existencias
    BEGIN
        ROLLBACK TRANSACTION;
        SELECT N'La cantidad supera las existencias disponibles.';
        RETURN;
    END;

    IF @CarritoId IS NULL
    BEGIN
        INSERT INTO dbo.Carrito (UsuarioId) VALUES (@UsuarioId);
        SET @CarritoId = CONVERT(int, SCOPE_IDENTITY());
    END;

    IF @CantidadActual = 0
        INSERT INTO dbo.CarritoDetalle (CarritoId, ProductoId, Cantidad)
        VALUES (@CarritoId, @ProductoId, CONVERT(int, @NuevaCantidad));
    ELSE
        UPDATE dbo.CarritoDetalle SET Cantidad = CONVERT(int, @NuevaCantidad)
        WHERE CarritoId = @CarritoId AND ProductoId = @ProductoId;

    COMMIT TRANSACTION;
    SELECT CAST(NULL AS nvarchar(200));
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Carrito_Eliminar
    @UsuarioId int,
    @ProductoId int
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    DECLARE @Usuario int;
    SELECT @Usuario = UsuarioId FROM dbo.Usuario WITH (UPDLOCK, HOLDLOCK)
    WHERE UsuarioId = @UsuarioId;

    DELETE FROM dbo.CarritoDetalle
    WHERE ProductoId = @ProductoId AND CarritoId = (
        SELECT TOP (1) CarritoId FROM dbo.Carrito
        WHERE UsuarioId = @UsuarioId ORDER BY CarritoId
    );

    COMMIT TRANSACTION;
END;
GO
