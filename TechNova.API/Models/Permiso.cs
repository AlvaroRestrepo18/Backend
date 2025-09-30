using System;
using System.Collections.Generic;

namespace TechNova.API.Models;

public partial class Permiso
{
    public int IdPermiso { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Permisoxrol> Permisoxrols { get; set; } = new List<Permisoxrol>();
}
