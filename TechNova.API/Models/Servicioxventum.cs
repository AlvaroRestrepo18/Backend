using System;
using System.Collections.Generic;

namespace TechNova.API.Models
{
    public partial class Servicioxventum
    {
        public int Id { get; set; }

        public int FkServicio { get; set; }

        public int FkVenta { get; set; }

        public decimal Precio { get; set; }

        public string? Detalles { get; set; }  // opcional (nullable)

        public decimal? ValorTotal { get; set; }  // opcional (nullable)

        public virtual Servicio FkServicioNavigation { get; set; } = null!;

        public virtual Venta FkVentaNavigation { get; set; } = null!;
    }
}
