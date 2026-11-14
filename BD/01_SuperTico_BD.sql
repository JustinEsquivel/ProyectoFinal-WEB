/* =====================================================================
   SúperTico - SC-701 Programación Avanzada en Web - Grupo 4
   Script 01: creación de la base de datos, tablas y datos iniciales.

   ⚠ Este script BORRA y vuelve a crear la base SuperTico_BD.
     Ejecútelo solo la primera vez o cuando quiera reiniciar los datos.
     Después ejecute los archivos SP_*.sql (cada uno se puede volver a
     ejecutar sin problema porque usan CREATE OR ALTER).

   Usuarios de prueba:
     admin@supertico.com    / Admin123    (Administrador)
     cliente@supertico.com  / Cliente123  (Cliente)
   ===================================================================== */

USE [master]
GO

IF DB_ID('SuperTico_BD') IS NOT NULL
BEGIN
	ALTER DATABASE [SuperTico_BD] SET SINGLE_USER WITH ROLLBACK IMMEDIATE
	DROP DATABASE [SuperTico_BD]
END
GO

CREATE DATABASE [SuperTico_BD]
GO

USE [SuperTico_BD]
GO

/* ---------------------------------------------------------------------
   Tablas
   --------------------------------------------------------------------- */

CREATE TABLE [dbo].[Rol](
	[RolId]		[int] IDENTITY(1,1) NOT NULL,
	[Nombre]	[varchar](50) NOT NULL,
 CONSTRAINT [PK_Rol] PRIMARY KEY CLUSTERED ([RolId] ASC),
 CONSTRAINT [UK_Rol_Nombre] UNIQUE ([Nombre])
)
GO

CREATE TABLE [dbo].[Usuario](
	[UsuarioId]		[int] IDENTITY(1,1) NOT NULL,
	[NombreUsuario]	[varchar](50) NOT NULL,
	[Correo]		[varchar](150) NOT NULL,
	[Contrasena]	[varchar](256) NOT NULL,
	[RolId]			[int] NOT NULL,
	[FechaRegistro]	[datetime] NOT NULL CONSTRAINT [DF_Usuario_FechaRegistro] DEFAULT (GETDATE()),
	[Activo]		[bit] NOT NULL CONSTRAINT [DF_Usuario_Activo] DEFAULT (1),
 CONSTRAINT [PK_Usuario] PRIMARY KEY CLUSTERED ([UsuarioId] ASC),
 CONSTRAINT [UK_Usuario_Correo] UNIQUE ([Correo]),
 CONSTRAINT [FK_Usuario_Rol] FOREIGN KEY ([RolId]) REFERENCES [dbo].[Rol]([RolId])
)
GO

CREATE TABLE [dbo].[Categoria](
	[CategoriaId]	[int] IDENTITY(1,1) NOT NULL,
	[Nombre]		[varchar](100) NOT NULL,
	[Descripcion]	[varchar](250) NULL,
 CONSTRAINT [PK_Categoria] PRIMARY KEY CLUSTERED ([CategoriaId] ASC),
 CONSTRAINT [UK_Categoria_Nombre] UNIQUE ([Nombre])
)
GO

CREATE TABLE [dbo].[Producto](
	[ProductoId]	[int] IDENTITY(1,1) NOT NULL,
	[CategoriaId]	[int] NOT NULL,
	[Nombre]		[varchar](150) NOT NULL,
	[Descripcion]	[varchar](500) NULL,
	[Precio]		[decimal](10, 2) NOT NULL,
	[Existencias]	[int] NOT NULL,
	[ImagenUrl]		[varchar](300) NULL,
	[Activo]		[bit] NOT NULL CONSTRAINT [DF_Producto_Activo] DEFAULT (1),
 CONSTRAINT [PK_Producto] PRIMARY KEY CLUSTERED ([ProductoId] ASC),
 CONSTRAINT [FK_Producto_Categoria] FOREIGN KEY ([CategoriaId]) REFERENCES [dbo].[Categoria]([CategoriaId]),
 CONSTRAINT [CK_Producto_Precio] CHECK ([Precio] > 0),
 CONSTRAINT [CK_Producto_Existencias] CHECK ([Existencias] >= 0)
)
GO

CREATE TABLE [dbo].[Carrito](
	[CarritoId]		[int] IDENTITY(1,1) NOT NULL,
	[UsuarioId]		[int] NOT NULL,
	[FechaCreacion]	[datetime] NOT NULL CONSTRAINT [DF_Carrito_FechaCreacion] DEFAULT (GETDATE()),
 CONSTRAINT [PK_Carrito] PRIMARY KEY CLUSTERED ([CarritoId] ASC),
 CONSTRAINT [FK_Carrito_Usuario] FOREIGN KEY ([UsuarioId]) REFERENCES [dbo].[Usuario]([UsuarioId])
)
GO

CREATE TABLE [dbo].[CarritoDetalle](
	[CarritoDetalleId]	[int] IDENTITY(1,1) NOT NULL,
	[CarritoId]			[int] NOT NULL,
	[ProductoId]		[int] NOT NULL,
	[Cantidad]			[int] NOT NULL,
 CONSTRAINT [PK_CarritoDetalle] PRIMARY KEY CLUSTERED ([CarritoDetalleId] ASC),
 CONSTRAINT [UK_CarritoDetalle_Carrito_Producto] UNIQUE ([CarritoId], [ProductoId]),
 CONSTRAINT [FK_CarritoDetalle_Carrito] FOREIGN KEY ([CarritoId]) REFERENCES [dbo].[Carrito]([CarritoId]),
 CONSTRAINT [FK_CarritoDetalle_Producto] FOREIGN KEY ([ProductoId]) REFERENCES [dbo].[Producto]([ProductoId]),
 CONSTRAINT [CK_CarritoDetalle_Cantidad] CHECK ([Cantidad] > 0)
)
GO

CREATE TABLE [dbo].[EstadoPedido](
	[EstadoPedidoId]	[int] IDENTITY(1,1) NOT NULL,
	[Nombre]			[varchar](50) NOT NULL,
 CONSTRAINT [PK_EstadoPedido] PRIMARY KEY CLUSTERED ([EstadoPedidoId] ASC),
 CONSTRAINT [UK_EstadoPedido_Nombre] UNIQUE ([Nombre])
)
GO

CREATE TABLE [dbo].[Pedido](
	[PedidoId]			[int] IDENTITY(1,1) NOT NULL,
	[UsuarioId]			[int] NOT NULL,
	[EstadoPedidoId]	[int] NOT NULL,
	[FechaPedido]		[datetime] NOT NULL CONSTRAINT [DF_Pedido_FechaPedido] DEFAULT (GETDATE()),
	[FechaRetiro]		[datetime] NULL,
	[Total]				[decimal](10, 2) NOT NULL,
 CONSTRAINT [PK_Pedido] PRIMARY KEY CLUSTERED ([PedidoId] ASC),
 CONSTRAINT [FK_Pedido_Usuario] FOREIGN KEY ([UsuarioId]) REFERENCES [dbo].[Usuario]([UsuarioId]),
 CONSTRAINT [FK_Pedido_EstadoPedido] FOREIGN KEY ([EstadoPedidoId]) REFERENCES [dbo].[EstadoPedido]([EstadoPedidoId])
)
GO

CREATE TABLE [dbo].[PedidoDetalle](
	[PedidoDetalleId]	[int] IDENTITY(1,1) NOT NULL,
	[PedidoId]			[int] NOT NULL,
	[ProductoId]		[int] NOT NULL,
	[Cantidad]			[int] NOT NULL,
	[PrecioUnitario]	[decimal](10, 2) NOT NULL,
 CONSTRAINT [PK_PedidoDetalle] PRIMARY KEY CLUSTERED ([PedidoDetalleId] ASC),
 CONSTRAINT [FK_PedidoDetalle_Pedido] FOREIGN KEY ([PedidoId]) REFERENCES [dbo].[Pedido]([PedidoId]),
 CONSTRAINT [FK_PedidoDetalle_Producto] FOREIGN KEY ([ProductoId]) REFERENCES [dbo].[Producto]([ProductoId]),
 CONSTRAINT [CK_PedidoDetalle_Cantidad] CHECK ([Cantidad] > 0)
)
GO

/* Registro de errores del sistema (requisito técnico del proyecto).
   La llena el middleware de la API y el ErrorController de la WEB. */
CREATE TABLE [dbo].[BitacoraError](
	[BitacoraErrorId]	[int] IDENTITY(1,1) NOT NULL,
	[Fecha]				[datetime] NOT NULL CONSTRAINT [DF_BitacoraError_Fecha] DEFAULT (GETDATE()),
	[Origen]			[varchar](20) NOT NULL,
	[Ruta]				[varchar](300) NULL,
	[Mensaje]			[varchar](max) NOT NULL,
	[Detalle]			[varchar](max) NULL,
	[UsuarioId]			[int] NULL,
 CONSTRAINT [PK_BitacoraError] PRIMARY KEY CLUSTERED ([BitacoraErrorId] ASC)
)
GO

/* ---------------------------------------------------------------------
   Datos iniciales
   --------------------------------------------------------------------- */

SET IDENTITY_INSERT [dbo].[Rol] ON
INSERT [dbo].[Rol] ([RolId], [Nombre]) VALUES
	(1, 'Administrador'),
	(2, 'Cliente')
SET IDENTITY_INSERT [dbo].[Rol] OFF
GO

SET IDENTITY_INSERT [dbo].[EstadoPedido] ON
INSERT [dbo].[EstadoPedido] ([EstadoPedidoId], [Nombre]) VALUES
	(1, 'Pendiente'),
	(2, 'En preparación'),
	(3, 'Listo para retiro'),
	(4, 'Entregado'),
	(5, 'Cancelado')
SET IDENTITY_INSERT [dbo].[EstadoPedido] OFF
GO

/* Contraseñas guardadas con PBKDF2-SHA256 (mismo formato que SeguridadService de la API) */
INSERT [dbo].[Usuario] ([NombreUsuario], [Correo], [Contrasena], [RolId]) VALUES
	('Administrador SúperTico', 'admin@supertico.com',   '0eCiOoAgTXICGc9d3KrXMx8fGqKAVdXYy7Cdh+HPRCeWR61dK6d5ucw2jrydrY4o', 1),
	('Cliente de Prueba',       'cliente@supertico.com', 'KP1ZMGIssxCxZ+lde3Yi9dI5UXihmJZTx5kUW9QzfITqjHU8/YSFpgZiFjpICb4U', 2)
GO

SET IDENTITY_INSERT [dbo].[Categoria] ON
INSERT [dbo].[Categoria] ([CategoriaId], [Nombre], [Descripcion]) VALUES
	(1, 'Frutas y verduras',  'Productos frescos de temporada'),
	(2, 'Carnes y pescados',  'Carnes, pollo y mariscos'),
	(3, 'Lácteos',            'Leche, quesos y derivados'),
	(4, 'Panadería',          'Pan fresco y repostería'),
	(5, 'Bebidas',            'Jugos, refrescos y agua'),
	(6, 'Abarrotes',          'Granos, pastas y productos básicos'),
	(7, 'Snacks y dulces',    'Chocolates, frutos secos y golosinas'),
	(8, 'Mascotas',           'Alimento y accesorios para mascotas'),
	(9, 'Limpieza',           'Artículos de limpieza para el hogar')
SET IDENTITY_INSERT [dbo].[Categoria] OFF
GO

INSERT [dbo].[Producto] ([CategoriaId], [Nombre], [Descripcion], [Precio], [Existencias], [ImagenUrl]) VALUES
	(1, 'Brócoli',                     'Brócoli fresco, por unidad.',                    850.00,  40, '/images/product-thumb-5.png'),
	(1, 'Tomate',                      'Tomate maduro, bandeja de 4 unidades.',          1200.00, 35, '/images/product-thumb-29.png'),
	(1, 'Chile dulce rojo',            'Bolsa de chiles dulces rojos.',                  1450.00, 20, '/images/product-thumb-4.png'),
	(1, 'Lechuga americana',           'Lechuga fresca, por unidad.',                    700.00,  25, '/images/product-thumb-16.png'),
	(1, 'Limón mandarina',             'Bolsa de limones de 1 kg.',                      1100.00, 3,  '/images/product-thumb-13.png'),
	(1, 'Hongos frescos',              'Bandeja de hongos blancos de 250 g.',            1950.00, 15, '/images/product-thumb-15.png'),
	(2, 'Pescado entero',              'Corvina entera fresca, aprox. 1 kg.',            5800.00, 10, '/images/product-thumb-6.png'),
	(3, 'Queso amarillo',              'Queso amarillo en trozos, 300 g.',               2650.00, 18, '/images/product-thumb-3.png'),
	(4, 'Pan baguette',                'Baguette horneada del día.',                     950.00,  30, '/images/product-thumb-17.png'),
	(4, 'Pan artesanal',               'Pan artesanal con hierbas, 500 g.',              1800.00, 12, '/images/product-thumb-1.png'),
	(5, 'Jugo de naranja',             'Jugo de naranja natural, 1 L.',                  1650.00, 22, '/images/product-thumb-26.png'),
	(5, 'Jugo de frutas mixtas',       'Jugo de frutas en caja, 1 L.',                   1350.00, 28, '/images/product-thumb-7.png'),
	(5, 'Jugo en lata',                'Jugo de frutas en lata, 330 ml.',                650.00,  50, '/images/product-thumb-30.png'),
	(6, 'Pasta corta',                 'Pasta tipo penne, 500 g.',                       890.00,  45, '/images/product-thumb-2.png'),
	(6, 'Arroz 99% grano entero',      'Arroz blanco, 1 kg.',                            1250.00, 60, '/images/product-thumb-12.png'),
	(7, 'Bombones de chocolate',       'Caja de bombones de chocolate, 200 g.',          3200.00, 14, '/images/product-thumb-22.png'),
	(7, 'Pistachos',                   'Pistachos tostados y salados, 150 g.',           2900.00, 4,  '/images/product-thumb-25.png'),
	(8, 'Alimento para perro adulto',  'Alimento seco para perro, 2 kg.',                6500.00, 9,  '/images/product-thumb-11.png'),
	(9, 'Detergente líquido',          'Detergente para ropa, 1 L.',                     2300.00, 0,  NULL)
GO
