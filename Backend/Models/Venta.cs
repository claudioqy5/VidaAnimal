using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VidaAnimal.API.Models
{
    public class Venta
    {
        [Key]
        public int VentaID { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        public int UsuarioID { get; set; }
        [ForeignKey("UsuarioID")]
        public virtual Usuario? Usuario { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal SubTotal { get; set; } = 0;

        [Column(TypeName = "decimal(12,2)")]
        public decimal Total { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal Descuento { get; set; } = 0;

        [MaxLength(500)]
        public string? Observaciones { get; set; }

        [MaxLength(50)]
        public string? SerieComprobante { get; set; }

        [MaxLength(50)]
        public string? NumeroComprobante { get; set; }

        [MaxLength(50)]
        public string MetodoPago { get; set; } = "Efectivo";

        [MaxLength(50)]
        public string Estado { get; set; } = "Completada";

        public int? ClienteID { get; set; }
        public virtual Cliente? Cliente { get; set; }

        public virtual ICollection<VentaDetalle> VentaDetalles { get; set; } = new List<VentaDetalle>();
    }

    // Y el de detalles, mapeado directamente a "DetalleVentas" como lo tienes en SQL Server
    [Table("DetalleVentas")]
    public class VentaDetalle
    {
        [Key]
        [Column("DetalleVentaID")]
        public int VentaDetalleID { get; set; }

        public int VentaID { get; set; }

        [ForeignKey("VentaID")]
        public virtual Venta? Venta { get; set; }

        public int ProductoID { get; set; }

        [ForeignKey("ProductoID")]
        public virtual Producto? Producto { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal Cantidad { get; set; }

        [Column("PrecioUnitario", TypeName = "decimal(12,2)")]
        public decimal PrecioVentaUnitario { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal? PrecioCostoUnitario { get; set; } // Nuevo: Para el margen

        [Column(TypeName = "decimal(12,2)")]
        public decimal? Ganancia { get; set; } // Nuevo: Para el Dashboard

        [Column(TypeName = "decimal(12,2)")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal SubTotal { get; set; }

        public string? UnidadVenta { get; set; }
    }
}
