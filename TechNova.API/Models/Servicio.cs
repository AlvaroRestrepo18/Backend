using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TechNova.API.Models
{
    public partial class Servicio
    {
        public int? Id { get; set; }

        public int? CategoriaId { get; set; }

        public string? Nombre { get; set; } = null!;

        public string? Detalles { get; set; }

        public decimal? Precio { get; set; }

        // 🔹 Ignoramos estas en JSON para evitar problemas en POST
        public virtual Categoria? Categoria { get; set; }

        public virtual ICollection<Servicioxventum> Servicioxventa { get; set; } = new List<Servicioxventum>();
    }
}
