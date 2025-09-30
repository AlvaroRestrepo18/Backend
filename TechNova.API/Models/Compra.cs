using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TechNova.API.Models;

public partial class Compra
{
    public int Id { get; set; }

    public int ProveedorId { get; set; }

    public DateTime FechaCompra { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public string? MetodoPago { get; set; }

    public bool? Estado { get; set; }

    public decimal? Subtotal { get; set; }

    public decimal? Iva { get; set; }

    public decimal Total { get; set; }

    public virtual Proveedore Proveedor { get; set; } = null!;

    [JsonPropertyName("detallesCompra")]
    public virtual ICollection<DetalleCompra> DetallesCompra { get; set; } = new List<DetalleCompra>();


}
