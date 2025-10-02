using System;
using System.Collections.Generic;

namespace TechNova.API.Models;

public partial class Productoxventum
{
    public int Id { get; set; }

    public int ProductoId { get; set; }

    public int VentaId { get; set; }

    public int? Cantidad { get; set; }  // en la BD puede ser null

    public decimal ValorUnitario { get; set; }

    public decimal? ValorTotal { get; set; }  // en la BD puede ser null

    // ✅ Objetos de navegación (no strings)
    public virtual Producto Producto { get; set; }

    public virtual Venta Venta { get; set; }
}
