using GestionVentas.Models;
using Microsoft.AspNetCore.Mvc;
using GestionVentas.Helpers;

namespace GestionVentas.Controllers
{
    public class PresupuestoController : Controller
    {
        private readonly ConexionDB db;

        // Constructor con DI correcto
        public PresupuestoController(ConexionDB db)
        {
            this.db = db;
        }

        // GET: Presupuesto
        public ActionResult Index()
        {
            var presupuestos = db.ObtenerPresupuestos();
            return View(presupuestos);
        }

        // GET: Presupuesto/Details/5
        public ActionResult Details(int id)
        {
            var presupuesto = db.ObtenerPresupuestoPorId(id);
            if (presupuesto == null)
            {
                return NotFound();
            }

            return View(presupuesto);
        }

        // GET: Presupuesto/Create
        public ActionResult Create()
        {
            var model = new Presupuesto
            {
                Fecha = DateTime.Now
            };

            return View(model);
        }

        // POST: Presupuesto/Create
      [HttpPost]
public ActionResult Create(Presupuesto presupuesto)
{
    try
    {
        // 1. CORRECCIÓN DE DECIMALES:
        // Recorremos los items para arreglar el precio que .NET entendió mal
        for (int i = 0; i < presupuesto.Items.Count; i++)
        {
            // Buscamos el nombre exacto que tiene el input en el HTML
            string key = $"Items[{i}].PrecioUnitario";
            string precioRaw = Request.Form[key];

            if (!string.IsNullOrEmpty(precioRaw))
            {
                // Reemplazamos cualquier coma por punto (por si las dudas) 
                // y parseamos usando InvariantCulture para que el punto SIEMPRE sea decimal.
                string precioLimpio = precioRaw.Replace(",", ".");
                
                if (decimal.TryParse(precioLimpio, 
                    System.Globalization.NumberStyles.Any, 
                    System.Globalization.CultureInfo.InvariantCulture, 
                    out decimal precioCorrecto))
                {
                    presupuesto.Items[i].PrecioUnitario = precioCorrecto;
                }
            }
        }

        // 2. Lógica normal de guardado
        presupuesto.Fecha = FechaHelper.AhoraArgentina();
        db.AgregarPresupuesto(presupuesto);

        return RedirectToAction("Index");
    }
    catch (Exception ex)
    {
        ViewBag.Error = "Ocurrió un error al guardar el presupuesto: " + ex.Message;
        return View(presupuesto);
    }
}

[HttpPost]
[IgnoreAntiforgeryToken]
public IActionResult ActualizarTelefono(int id, string telefono)
{
    try 
    {
        db.ActualizarTelefonoPresupuesto(id, telefono);
        return Ok();
    }
    catch (Exception ex)
    {
        // Esto ayuda a ver si hay un error de conexión o de nombre de tabla
        return BadRequest("Error: " + ex.Message);
    }
}
        [HttpGet]
        public IActionResult ObtenerSiguienteNroPresupuesto()
        {
            int maxNro = db.ObtenerMaximoNroPresupuesto();
            return Json(maxNro + 1);
        }

        // GET: Presupuesto/Delete/5
        public ActionResult Delete(int id)
        {
            var presupuesto = db.ObtenerPresupuestoPorId(id);
            if (presupuesto == null)
            {
                return NotFound();
            }

            return View(presupuesto);
        }

        // POST: Presupuesto/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                db.EliminarPresupuesto(id);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar el presupuesto: " + ex.Message;
                return RedirectToAction("Delete", new { id });
            }
        }

        public JsonResult ObtenerProductos()
        {
            var productos = db.ObtenerProductos();

            var lista = productos.Select(p => new
            {
                id = p.IdProducto,
                nombre = p.Nombre,
                descripcion = p.Descripcion,
                precio = Math.Round(p.PrecioCosto * (1 + p.RecargoPorcentaje / 100), 2)
            }).ToList();

            return Json(lista);
        }

        
    }
}
