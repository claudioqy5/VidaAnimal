using System.ComponentModel.DataAnnotations;

namespace VidaAnimal.API.Models
{
    public class Categoria
    {
        [Key]
        public int CategoriaID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } // Ejemplo: Alimentos, Juguetes, Limpieza, etc.

        public bool Activo { get; set; } = true;
    }
}
