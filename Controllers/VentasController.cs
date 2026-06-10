using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using GestionVentas.Models;
using System.Linq;
using Microsoft.Extensions.Configuration;
using GestionVentas.Helpers;

namespace GestionVentas.Controllers
{
    public class VentasController : Controller
    {
        private readonly ConexionDB db;

        public VentasController(IConfiguration config)
        {
            db = new ConexionDB(config);
        }

       public ActionResult Create()
{
    bool cajaViejaCerrada = db.VerificarYCerrarCajaVencida();

    if (cajaViejaCerrada)
    {
        TempData["MensajeInfo"] =
            "La caja del día anterior fue cerrada automáticamente.";
    }

    bool hayCaja = db.VerificarSiHayCajaAbierta();

    if (!hayCaja)
    {
        TempData["MensajeError"] =
            "DEBE ABRIR CAJA PRIMERO PARA PODER VENDER.";

        return RedirectToAction("AbrirCaja", "Caja");
    }

    var productos = db.ObtenerProductos() ?? new List<Productos>();

    ViewBag.nombreyApellido = User.Identity.Name ?? "Vendedor";

    return View(productos);
}


        public IActionResult ImprimirTicket(int id)
        {
            var factura = db.ObtenerFacturaConItems(id); // Tu método que trae factura + lista de productos
            if (factura == null) return NotFound();

            return View(factura);
        }

        [HttpPost]
        public JsonResult RegistrarVenta([FromBody] VentaCompleta datos)
        {
            try
            {
                if (datos == null || datos.Items == null || datos.Items.Count == 0)
                    return Json(new { success = false, message = "No hay productos seleccionados." });

                // 1. Registramos la venta en la DB. 
                // IMPORTANTE: Asegúrate que db.RegistrarVenta guarde la factura con Estado = 'Pendiente' si es CtaCte
                var resultado = db.RegistrarVenta(datos);

                if (resultado.success)
                {
                    // --- LÓGICA DE IMPACTO EN CAJA ---
                    var cajaAbierta = db.Cajas.FirstOrDefault(c => c.EstaAbierta);

                    if (cajaAbierta != null)
                    {
                        string medio = datos.MedioPago ?? "Efectivo";

                        // FILTRO CLAVE: Solo impacta en caja si NO es CtaCte/Fiado
                        bool esCtaCte = medio.Equals("CtaCte", StringComparison.OrdinalIgnoreCase) ||
                                        medio.Equals("Cuenta Corriente", StringComparison.OrdinalIgnoreCase) ||
                                        medio.Equals("Fiado", StringComparison.OrdinalIgnoreCase);

                        if (!esCtaCte)
                        {
                            // Si es Efectivo o Transferencia, registramos el movimiento de dinero ahora
                            string etiquetaMedio = medio.Contains("Transferencia") ? "(Transferencia)" : "(Efectivo)";

                            var mov = new MovimientoCaja
                            {
                                CajaId = cajaAbierta.Id,
                                Fecha = DateTime.Now,
                                Monto = datos.MontoVenta,
                                Concepto = $"Venta Factura N° {resultado.idFactura} {etiquetaMedio}",
                                Tipo = TipoMovimiento.Ingreso,
                                Usuario = User.Identity.Name
                            };

                            db.MovimientosCaja.Add(mov);

                            // Sumamos al esperado porque el dinero (o el saldo bancario) entró realmente
                            cajaAbierta.MontoEsperado += datos.MontoVenta;

                            db.Cajas.Update(cajaAbierta);
                            db.SaveChanges();
                        }
                        // NOTA: Si es esCtaCte == true, NO entra en este bloque. 
                        // El dinero se registrará en caja recién cuando el cliente pague su deuda en CtaCte/Index.
                    }

                    string nroFacturaFormateado = "0001-" + resultado.idFactura.ToString("D8");
                    return Json(new
                    {
                        success = true,
                        idFactura = resultado.idFactura,
                        nroFactura = nroFacturaFormateado
                    });
                }
                else
                {
                    return Json(new { success = false, message = resultado.error });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpGet]
        public JsonResult ObtenerProximoNroFactura()
        {
            try
            {
                int proximoId = db.ObtenerProximoAutoIncremento("facturas", "gestionventas");
                string nroFormateado = "0001-" + proximoId.ToString("D8");
                return Json(new { success = true, nroFactura = nroFormateado });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}