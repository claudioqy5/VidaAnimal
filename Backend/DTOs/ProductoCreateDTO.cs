using Microsoft.AspNetCore.Http;

namespace VidaAnimal.API.DTOs
{
    public class ProductoCreateDTO
    {
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
        public int? ProveedorID { get; set; }
        public string UnidadMedida { get; set; }
        public decimal PrecioCosto { get; set; }
        public decimal PrecioVenta { get; set; }
        public decimal StockActual { get; set; }
        public decimal StockMinimo { get; set; }

        public decimal? PrecioMayorista { get; set; }
        public decimal? CantidadMayorista { get; set; }
        public string? NombreUnidadMayorista { get; set; }

        public IFormFile? ImagenFoto { get; set; }
        public bool EliminarImagen { get; set; }
    }
}
