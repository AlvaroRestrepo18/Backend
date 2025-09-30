using System;
using System.Collections.Generic;

namespace TechNova.API.Models;

public partial class Servicioxventum
{
    public int Id { get; set; }

    public int FkServicio { get; set; }

    public int FkVenta { get; set; }

    public decimal Precio { get; set; }

    public virtual Servicio FkServicioNavigation { get; set; } = null!;

    public virtual Venta FkVentaNavigation { get; set; } = null!;
}
