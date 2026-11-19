namespace ProyectoFinal_WEB.Models
{
    public class ProductoCatalogoViewModel
    {
        public int ProductoId { get; set; }
        public int CategoriaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Existencias { get; set; }
        public string? ImagenUrl { get; set; }
        public string CategoriaNombre { get; set; } = string.Empty;
    }
}