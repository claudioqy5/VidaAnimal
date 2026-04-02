using System;

namespace VidaAnimal.API.Models
{
    public class Usuario
    {
        public int UsuarioID { get; set; }
        public string DNI { get; set; }
        public string NombreCompleto { get; set; }
        public string Correo { get; set; }
        public string PasswordHash { get; set; }
        public string Rol { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}
