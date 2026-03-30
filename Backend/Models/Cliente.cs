using System;
using System.ComponentModel.DataAnnotations;

namespace VidaAnimal.API.Models
{
    public class Cliente
    {
        [Key]
        public int ClienteID { get; set; }
        
        [Required]
        [MaxLength(20)]
        public string DocumentoIdentidad { get; set; } // DNI o RUC
        
        [Required]
        [MaxLength(150)]
        public string NombreCompleto { get; set; }
        
        [MaxLength(20)]
        public string? Telefono { get; set; }
        
        [MaxLength(100)]
        public string? Correo { get; set; }
        
        [MaxLength(200)]
        public string? Direccion { get; set; }
        
        public DateTime? FechaNacimiento { get; set; }
        
        public bool Activo { get; set; } = true;
        
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
    }
}
