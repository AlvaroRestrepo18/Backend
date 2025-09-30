using System;
using System.Collections.Generic;

namespace TechNova.API.Models;

public partial class Permisoxrol
{
    public int IdPermisoRol { get; set; }

    public int FkRol { get; set; }

    public int FkPermiso { get; set; }

    public virtual Permiso FkPermisoNavigation { get; set; } = null!;

    public virtual Role FkRolNavigation { get; set; } = null!;
}
