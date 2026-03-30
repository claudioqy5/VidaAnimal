using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VidaAnimal.API.Models
{
    public class Compra
    {
        [Key]
        public int CompraID { get; set; }
        
        public int ProveedorID { get; set; }
        public virtual Proveedor? Proveedor { get; set; }
        public int? UsuarioID { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string NumeroComprobante { get; set; }
        
        public DateTime FechaCompra { get; set; } = DateTime.Now;
        
        [Column(TypeName = "decimal(12,2)")]
        public decimal Total { get; set; }
    }

    public class CompraDetalle
    {
        [Key]
        public int CompraDetalleID { get; set; }
        
        public int CompraID { get; set; }
        public int ProductoID { get; set; }
        
        [Column(TypeName = "decimal(12,3)")]
        public decimal Cantidad { get; set; }
        
        [Column(TypeName = "decimal(12,2)")]
        public decimal PrecioCostoUnitario { get; set; }
        
        [Column(TypeName = "decimal(12,2)")]
        public decimal SubTotal { get; set; }
    }
}
