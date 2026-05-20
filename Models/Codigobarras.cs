namespace GestionVentas.Models
{
public class Codigobarras
{
    public int Id { get; set; }

    public int ProductoId { get; set; }

    public string Codigo { get; set; }

    public bool GeneradoAutomaticamente { get; set; }

    public DateTime FechaCreacion { get; set; }

    public Producto Producto { get; set; }
}
}