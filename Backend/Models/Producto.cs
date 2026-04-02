using System;

namespace VidaAnimal.API.Models
{
    public class Producto
    {
        public int ProductoID { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int? ProveedorID { get; set; }
        public string UnidadMedida { get; set; } = "UND";
        
        public decimal PrecioCosto { get; set; } // precio de costo por saco
        public decimal PrecioVenta { get; set; }  // PrecioVenta = Precio por Kilo
        
        // Para productos que se venden por saco (UnidadMedida = SACO)
        
        
        
        
        public decimal? PrecioMayorista { get; set; } // PrecioMayorista = Precio por Saco Completo
        public decimal? CantidadMayorista { get; set; } // CantidadMayorista = Peso de cada Saco en Kilos (ej. 40, 50)
        public string? NombreUnidadMayorista { get; set; } // guarda el nombre de la unidad de medida

        [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "decimal(18,3)")]
        public decimal StockActual { get; set; } // StockActual = Cantidad de Sacos (admite decimales para porciones de saco)

        [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "decimal(18,3)")]
        public decimal StockMinimo { get; set; }

        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string? ImagenURL { get; set; }
    }
}
