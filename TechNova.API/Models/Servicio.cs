using System;
using System.Collections.Generic;

namespace TechNova.API.Models;

public partial class Servicio
{
    public int Id { get; set; }

    public int FkCategoria { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public decimal Precio { get; set; }

    public bool Activo { get; set; }

    public virtual Categoria FkCategoriaNavigation { get; set; } = null!;

    public virtual ICollection<Servicioxventum> Servicioxventa { get; set; } = new List<Servicioxventum>();
}
