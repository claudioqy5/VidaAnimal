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
        
        // Para productos que se venden por saco (UnidadMedida = SACO)
        // PrecioVenta = Precio por Kilo
        // PrecioMayorista = Precio por Saco Completo
        // CantidadMayorista = Peso de cada Saco en Kilos (ej. 40, 50)
        // StockActual = Cantidad de Sacos (admite decimales para porciones de saco)
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
