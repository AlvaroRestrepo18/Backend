using System;
using System.Collections.Generic;

namespace TechNova.API.Models;

public partial class Venta
{
    public int Id { get; set; }

    public int FkCliente { get; set; }

    public DateTime Fecha { get; set; }

    public decimal Total { get; set; }

    public virtual Cliente FkClienteNavigation { get; set; } = null!;

    public virtual ICollection<Productoxventum> Productoxventa { get; set; } = new List<Productoxventum>();

    public virtual ICollection<Servicioxventum> Servicioxventa { get; set; } = new List<Servicioxventum>();
}
