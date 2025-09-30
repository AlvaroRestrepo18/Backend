﻿using System;
using System.Collections.Generic;

namespace TechNova.API.Models;

public partial class Role
{
    public int IdRol { get; set; }

    public string NombreRol { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<Permisoxrol> Permisoxrols { get; set; } = new List<Permisoxrol>();

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
