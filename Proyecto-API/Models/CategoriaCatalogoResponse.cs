namespace ProyectoFinal_API.Models
{
    public class CategoriaCatalogoResponse
    {
        public int CategoriaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
    }
}