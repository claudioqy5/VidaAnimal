using System;

namespace VidaAnimal.API.Models
{
    public class Producto
    {
        public int ProductoID { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
        public int CategoriaID { get; set; }
        public int? ProveedorID { get; set; }
        public string UnidadMedida { get; set; }
        public decimal PrecioCosto { get; set; }
        public decimal PrecioVenta { get; set; }
        public decimal StockActual { get; set; }
        public decimal StockMinimo { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }

        public string? ImagenURL { get; set; }
    }
}
