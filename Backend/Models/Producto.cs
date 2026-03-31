using System;

namespace VidaAnimal.API.Models
{
    public class Producto
    {
        public int ProductoID { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
        public int? ProveedorID { get; set; }
        public string UnidadMedida { get; set; }
        public decimal PrecioCosto { get; set; }
        public decimal PrecioVenta { get; set; }
        
        // Campos para venta por mayor (ej: Sacos/Costales)
        public decimal? PrecioMayorista { get; set; }
        public decimal? CantidadMayorista { get; set; }
        public string? NombreUnidadMayorista { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "decimal(18,3)")]
        public decimal StockActual { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "decimal(18,3)")]
        public decimal StockMinimo { get; set; }

        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string? ImagenURL { get; set; }
    }
}
