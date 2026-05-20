using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GestionVentas.Models
{

    public class Productos
    {
        public int IdProducto { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Categoria { get; set; }
        public decimal PrecioCosto { get; set; }
        public decimal RecargoPorcentaje { get; set; }
        public decimal PrecioVenta
        {
            get
            {
                return PrecioCosto + (PrecioCosto * RecargoPorcentaje / 100);
            }

        }
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }


        
       
        public string? Imagen { get; set; }
        public string? NombreProveedor { get; set; }
      
        public List<SelectListItem>? Proveedores { get; set; }

       public virtual ICollection<Codigobarras> CodigosBarras { get; set; }
    = new List<Codigobarras>();

    }
}