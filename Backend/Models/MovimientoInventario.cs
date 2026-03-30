using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VidaAnimal.API.Models
{
    public class MovimientoInventario
    {
        [Key]
        public int MovimientoID { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(20)]
        public string Tipo { get; set; } = string.Empty; // "ENTRADA", "SALIDA", "AJUSTE"

        public int ProductoID { get; set; }
        
        [ForeignKey("ProductoID")]
        public virtual Producto? Producto { get; set; }

        [Column(TypeName = "decimal(12,3)")]
        public decimal Cantidad { get; set; }

        public int UsuarioID { get; set; }
        
        [ForeignKey("UsuarioID")]
        public virtual Usuario? Usuario { get; set; }

        public int? ReferenciaID { get; set; } // ID de Venta o Compra

        [MaxLength(250)]
        public string? Observaciones { get; set; }

        [Column(TypeName = "decimal(12,3)")]
        public decimal? StockAnterior { get; set; }

        [Column(TypeName = "decimal(12,3)")]
        public decimal? StockNuevo { get; set; }
    }
}
