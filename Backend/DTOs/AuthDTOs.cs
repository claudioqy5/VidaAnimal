namespace VidaAnimal.API.DTOs
{
    public class LoginDTO
    {
        public string Correo { get; set; }
        public string Password { get; set; }
    }

    public class UsuarioCreateDTO
    {
        public string DNI { get; set; }
        public string NombreCompleto { get; set; }
        public string Correo { get; set; }
        public string Password { get; set; }
        public string Rol { get; set; } // 'ADMINISTRADOR' o 'CAJERO'
    }
}
