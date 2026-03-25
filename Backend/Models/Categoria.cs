using System;

namespace VidaAnimal.API.Models
{
    public class Categoria
    {
        public int CategoriaID { get; set; }
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
    }
}
