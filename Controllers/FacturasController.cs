using GestionVentas.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GestionVentas.Helpers;
namespace GestionVentas.Controllers
{
    public class FacturasController : Controller
    {
        private readonly ConexionDB conexion;

        public FacturasController(ConexionDB conexion)
        {
            this.conexion = conexion;
        }

        public IActionResult Index()
        {
            ViewBag.Rol = HttpContext.Session.GetString("Rol");

            var facturas = conexion.ObtenerFacturas(); 
            return View(facturas);
        }

        public IActionResult Detalles(int id)
        {
            var factura = conexion.ObtenerFacturaConItems(id);
            if (factura == null)
            {
                return NotFound();
            }

            return View(factura);
        }

        public ActionResult Eliminar(int id)
        {
            var factura = conexion.ObtenerFacturaConItems(id);
            if (factura == null)
            {
                return RedirectToAction("Index");
            }

            return View(factura);
        }

        [Authorize(Roles = "Administrador")] 
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarConfirmado(int id)
        {
            conexion.EliminarFactura(id);
            return RedirectToAction("Index");
        }
    }
}
