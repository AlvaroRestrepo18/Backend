using System;
using System.Collections.Generic;

namespace TechNova.API.Models;

public partial class Cliente
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Telefono { get; set; }

    public string? Correo { get; set; }

    public string? Direccion { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<Venta> Venta { get; set; } = new List<Venta>();
}
