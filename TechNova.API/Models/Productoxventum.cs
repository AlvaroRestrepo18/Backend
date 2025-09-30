using System;
using System.Collections.Generic;

namespace TechNova.API.Models;

public partial class Productoxventum
{
    public int Id { get; set; }

    public int ProductoId { get; set; }

    public int VentaId { get; set; }

    public int? Cantidad { get; set; }  // en la BD no es NOT NULL, así que puede ser null

    public decimal ValorUnitario { get; set; }

    public decimal? ValorTotal { get; set; } // también puede ser null en BD

    public virtual Producto Producto { get; set; } = null!;

    public virtual Venta Venta { get; set; } = null!;
}
