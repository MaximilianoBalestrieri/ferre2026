namespace GestionVentas.Models
{

public class ImprimirCodigoBarrasViewModel
{
    public int ProductoId { get; set; }

    public string NombreProducto { get; set; }

    public string Codigo { get; set; }

    public int Cantidad { get; set; }

    public string BarcodeBase64 { get; set; }
}
}