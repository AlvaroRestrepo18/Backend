using System.Text.Json.Serialization;
using TechNova.API.Models;

public partial class Venta
{
    public int Id { get; set; }

    public int ClienteId { get; set; }

    public DateTime fecha { get; set; }


    public decimal Total { get; set; }

    public bool Estado { get; set; }
// 👈 evita que el backend espere este campo en el JSON
    public virtual Cliente? Cliente { get; set; }


    public virtual ICollection<Productoxventum> Productoxventa { get; set; } = new List<Productoxventum>();

    public virtual ICollection<Servicioxventum> Servicioxventa { get; set; } = new List<Servicioxventum>();
}
