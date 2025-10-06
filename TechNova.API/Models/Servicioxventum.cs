using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TechNova.API.Models
{
    public partial class Servicioxventum
    {
        public int Id { get; set; }

        public int FkServicio { get; set; }

        public int FkVenta { get; set; }

        public decimal Precio { get; set; }

        public string? Detalles { get; set; }

        public decimal? ValorTotal { get; set; }

        // ✅ AGREGAR JsonIgnore PARA EVITAR VALIDACIÓN
        [JsonIgnore]
        public virtual Servicio? FkServicioNavigation { get; set; }
        [JsonIgnore]
        public virtual Venta? FkVentaNavigation { get; set; }
    }
}