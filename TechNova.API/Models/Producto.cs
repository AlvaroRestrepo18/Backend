using TechNova.API.Models;

public partial class Producto
{
    public int Id { get; set; }

    public int? CategoriaId { get; set; }
    public string? Nombre { get; set; }
    public int? Cantidad { get; set; }
    public decimal? Precio { get; set; }
    public DateTime? FechaCreacion { get; set; }

    public virtual Categoria? Categoria { get; set; }

    public virtual ICollection<Productoxventum> Productoxventa { get; set; } = new List<Productoxventum>();
}
