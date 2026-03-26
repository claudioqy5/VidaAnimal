using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VidaAnimal.API.DTOs
{
    public class CompraCreateDTO
    {
        [Required]
        public int ProveedorID { get; set; }
        
        [Required]
        public string NumeroComprobante { get; set; }
        
        public decimal Total { get; set; }
        
        [Required]
        public List<CompraDetalleDTO> Detalles { get; set; }
    }

    public class CompraDetalleDTO
    {
        public int ProductoID { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioCostoUnitario { get; set; }
        public decimal SubTotal { get; set; }
    }
}
