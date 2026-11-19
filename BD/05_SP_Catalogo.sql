/*//////////////////////////////////////////////////////// Procedimiento del catálogo publico.
/////////////////////////////////////////////////////////
  */
USE [SuperTico_BD]
GO

/* Lista los productos activos e incluyendo los agotados */
CREATE OR ALTER PROCEDURE dbo.sp_Producto_CatalogoListar
    @Busqueda varchar(150) = NULL,
    @CategoriaId int = NULL,
    @ProductoId int = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SET @Busqueda = NULLIF(LTRIM(RTRIM(@Busqueda)), '');

    SELECT p.ProductoId, p.CategoriaId, p.Nombre, p.Descripcion,
           p.Precio, p.Existencias, p.ImagenUrl,
           c.Nombre AS CategoriaNombre
    FROM dbo.Producto AS p
    INNER JOIN dbo.Categoria AS c ON c.CategoriaId = p.CategoriaId
    WHERE p.Activo = 1
      AND (
          @Busqueda IS NULL
          OR p.Nombre LIKE '%' + @Busqueda + '%'
      )
      AND (
          @CategoriaId IS NULL
          OR p.CategoriaId = @CategoriaId
      )
      AND (
          @ProductoId IS NULL
          OR p.ProductoId = @ProductoId
      )
    ORDER BY p.Nombre, p.ProductoId;
END;
GO
