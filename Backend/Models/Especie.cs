using System.ComponentModel.DataAnnotations;

namespace VidaAnimal.API.Models
{
    public class Especie
    {
        [Key]
        public int EspecieID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } // Ejemplo: Perro, Gato, Ave, etc.

        public bool Activo { get; set; } = true;
    }
}
