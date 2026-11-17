using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal_WEB.Models.Admin;

public class CategoriaForm
{
	[Required(ErrorMessage = "Escriba el nombre.")]
	[StringLength(100)]
	public string Nombre { get; set; } = string.Empty;

	[StringLength(250)]
	public string? Descripcion { get; set; }
}

public class CategoriaFila : CategoriaForm
{
	public int CategoriaId { get; set; }
}

public class ProductoForm
{
	[Range(1, int.MaxValue, ErrorMessage = "Seleccione una categoría.")]
	public int CategoriaId { get; set; }

	[Required(ErrorMessage = "Escriba el nombre.")]
	[StringLength(150)]
	public string Nombre { get; set; } = string.Empty;

	[StringLength(500)]
	public string? Descripcion { get; set; }

	public decimal Precio { get; set; }

	[Required(ErrorMessage = "Ingrese las existencias.")]
	[Range(0, int.MaxValue, ErrorMessage = "Las existencias no pueden ser negativas.")]
	public int? Existencias { get; set; }

	[StringLength(300)]
	public string? ImagenUrl { get; set; }
}

public class ProductoFila : ProductoForm
{
	public int ProductoId { get; set; }
	public bool Activo { get; set; }
	public string CategoriaNombre { get; set; } = string.Empty;
}