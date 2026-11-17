USE [SuperTico_BD];
GO

CREATE OR ALTER PROCEDURE dbo.sp_Categoria_AdminListar
    @Id int = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CategoriaId, Nombre, Descripcion
    FROM dbo.Categoria
    WHERE @Id IS NULL OR CategoriaId = @Id
    ORDER BY Nombre, CategoriaId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Categoria_AdminGuardar
    @Id int = NULL,
    @Nombre varchar(100),
    @Descripcion varchar(250) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @Id IS NULL
    BEGIN
        INSERT INTO dbo.Categoria (Nombre, Descripcion)
        VALUES (@Nombre, @Descripcion);
    END
    ELSE
    BEGIN
        UPDATE dbo.Categoria
        SET Nombre = @Nombre, Descripcion = @Descripcion
        WHERE CategoriaId = @Id;
    END;

    SELECT @@ROWCOUNT;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Categoria_AdminEliminar
    @Id int
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.Categoria
    WHERE CategoriaId = @Id;

    SELECT @@ROWCOUNT;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Producto_AdminListar
    @Id int = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT p.ProductoId, p.CategoriaId, p.Nombre, p.Descripcion,
           p.Precio, p.Existencias, p.ImagenUrl, p.Activo,
           c.Nombre AS CategoriaNombre
    FROM dbo.Producto AS p
    INNER JOIN dbo.Categoria AS c ON c.CategoriaId = p.CategoriaId
    WHERE @Id IS NULL OR p.ProductoId = @Id
    ORDER BY p.Nombre, p.ProductoId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Producto_AdminGuardar
    @Id int = NULL,
    @CategoriaId int,
    @Nombre varchar(150),
    @Descripcion varchar(500) = NULL,
    @Precio decimal(10,2),
    @Existencias int,
    @ImagenUrl varchar(300) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @Id IS NULL
    BEGIN
        INSERT INTO dbo.Producto
            (CategoriaId, Nombre, Descripcion, Precio, Existencias, ImagenUrl, Activo)
        VALUES
            (@CategoriaId, @Nombre, @Descripcion, @Precio, @Existencias, @ImagenUrl, 1);
    END
    ELSE
    BEGIN
        UPDATE dbo.Producto
        SET CategoriaId = @CategoriaId,
            Nombre = @Nombre,
            Descripcion = @Descripcion,
            Precio = @Precio,
            Existencias = @Existencias,
            ImagenUrl = @ImagenUrl
        WHERE ProductoId = @Id;
    END;

    SELECT @@ROWCOUNT;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Producto_AdminCambiarEstado
    @Id int,
    @Activo bit
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Producto
    SET Activo = @Activo
    WHERE ProductoId = @Id;

    SELECT @@ROWCOUNT;
END;
GO