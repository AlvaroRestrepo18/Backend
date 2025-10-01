using System;
using System.Collections.Generic;

namespace TechNova.API.Models;

public partial class Cliente
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;  // NOT NULL en BD

    public string? Apellido { get; set; }        // Puede ser NULL

    public string? TipoDoc { get; set; }         // Puede ser NULL

    public int? Documento { get; set; }          // Puede ser NULL en la BD

    public string? Correo { get; set; }          // Puede ser NULL

    public bool? Estado { get; set; }            // Puede ser NULL en la BD

    // Relación con Ventas (1 Cliente -> N Ventas)
    public virtual ICollection<Venta> Venta { get; set; } = new List<Venta>();
}
