using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TechNova.API.Models
{
    public partial class Productoxventum
    {
        public int Id { get; set; }

        public int ProductoId { get; set; }

        public int VentaId { get; set; }

        public int? Cantidad { get; set; }

        public decimal ValorUnitario { get; set; }

        public decimal? ValorTotal { get; set; }

        [JsonIgnore] // 👈 AGREGA ESTO
        public virtual Producto? FkproductoNavigation { get; set; }

        [JsonIgnore] // 👈 AGREGA ESTO
        public virtual Venta? FkVentaNavigation { get; set; }
    }
}