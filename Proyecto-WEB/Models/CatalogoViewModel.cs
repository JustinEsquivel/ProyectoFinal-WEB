namespace ProyectoFinal_WEB.Models
{
    public class CatalogoViewModel
    {
        public List<ProductoCatalogoViewModel> Productos { get; set; }
            = new List<ProductoCatalogoViewModel>();

        public List<CategoriaCatalogoViewModel> Categorias { get; set; }
            = new List<CategoriaCatalogoViewModel>();

        public string? Busqueda { get; set; }

        public int? CategoriaId { get; set; }
    }
}