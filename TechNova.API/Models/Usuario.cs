using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TechNova.API.Models;

public partial class Usuario
{
    [Key]
    public int IdUsuario { get; set; }

    public int FkRol { get; set; }

    public string Nombre { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Contrasena { get; set; } = null!;

    public bool Estado { get; set; }

    public string TipoDoc { get; set; } = null!;

    public string Documento { get; set; } = null!;

    public string Celular { get; set; } = null!;

    public string Direccion { get; set; } = null!;

    [Column("CodigoRecuperacion")]
    public string? CodigoRecuperacion { get; set; }

    [Column("CodigoExpira")]
    public DateTime? CodigoExpira { get; set; }

    public virtual Role? FkRolNavigation { get; set; }

}
