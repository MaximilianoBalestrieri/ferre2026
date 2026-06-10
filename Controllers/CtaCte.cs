using GestionVentas.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http; // Para usar Context.Session
using System;
using System.Collections.Generic;
using System.Linq;
using GestionVentas.Helpers;

namespace GestionVentas.Controllers
{
    public class CtaCteController : Controller
    {
        private readonly ConexionDB db;

        public CtaCteController(IConfiguration config)
        {
            this.db = new ConexionDB(config);
        }

        // 1. Detalle de la Libreta de Cuenta Corriente
   public IActionResult Index(int idCliente)
{
    // 1. Buscamos al cliente en la tabla ClientesCtaCte
    var cliente = db.ObtenerClienteCtaCtePorId(idCliente);
    if (cliente == null) return RedirectToAction("Index", "ClientesCtaCte");

    // 2. Usamos el IdClienteOriginal (el "cable" a la tabla maestra)
    int idReal = cliente.IdClienteOriginal;

    // Validación de seguridad: Si no hay vínculo, la lista vendrá vacía
    List<MovimientoCtaCte> historial = new List<MovimientoCtaCte>();
    if (idReal > 0)
    {
        historial = db.ObtenerHistorialCtaCte(idReal);
    }

    decimal totalActualizado = 0;
    foreach (var mov in historial)
    {
        // Solo recalculamos si es un producto cargado con ID
        if (mov.IdProducto > 0) 
        {
            var prodActual = db.ObtenerProductoPorId(mov.IdProducto);
            if (prodActual != null)
            {
                mov.Monto = prodActual.PrecioVenta * mov.Cantidad;
            }
        }
        totalActualizado += mov.Monto;
    }

    // 3. Actualizamos el saldo en la tabla de la libreta
    db.SobreescribirSaldoCliente(idCliente, totalActualizado);

    // 4. Pasamos los datos a la Vista
    // IMPORTANTE: Uso cliente.Nombre porque así está en tu SELECT de ClientesCtaCte
    ViewBag.NombreCliente = cliente.NombreCliente; 
    ViewBag.IdCliente = idCliente; 
    ViewBag.TotalDeuda = totalActualizado; 
    ViewBag.ListaProductos = db.ObtenerProductos();

    return View(historial); 
}

[HttpGet]
public JsonResult BuscarProductoPorCodigo(string codigo)
{
    var producto = db.ObtenerProductoPorCodigo(codigo); // Asegurate de tener este método en ConexionDB
    if (producto != null)
    {
        return Json(new { 
            success = true, 
            id = producto.IdProducto, 
            nombre = producto.Nombre, 
            precio = producto.PrecioVenta 
        });
    }
    return Json(new { success = false });
}
        // 2. Acción para agregar un producto a la libreta
        [HttpPost]
        public IActionResult AgregarItemCuenta(int idCliente, int idProducto, int cantidad, string vendedor, DateTime fecha)
        {
            if (idCliente > 0 && idProducto > 0 && cantidad > 0)
            {
                var producto = db.ObtenerProductoPorId(idProducto);
                if (producto != null)
                {
                    decimal totalItem = producto.PrecioVenta * cantidad;
                    
                    // Creamos un detalle claro para la base de datos
                    // Si tu SQL da error en 'resumenProductos', es porque el SP o el INSERT
                    // espera un nombre de columna distinto. Aquí mandamos el string:
                    string detalleParaDB = $"{producto.Nombre} (x{cantidad})";

                    // REGISTRAMOS EL GASTO
                    // Asegurate que en ConexionDB, el método use @idCli, @monto, @vendedor, @detalle
                    db.InsertarGastoCtaCte(idCliente, totalItem, vendedor, detalleParaDB, fecha);
                    
                    // IMPORTANTE: También deberías insertar el FacturaItem correspondiente 
                    // si querés que aparezca en el desglose de la tabla.
                }
            }
            return RedirectToAction("Index", new { idCliente = idCliente });
        }

        // 3. Liquidación total de la cuenta
        [HttpPost]
        public JsonResult CobrarCuentaCompleta(int idCliente, decimal totalADebitar)
        {
            try 
            {
                int idCaja = db.ObtenerIdCajaAbierta();
                
                if (idCaja == 0)
                    return Json(new { success = false, message = "No hay una caja abierta." });

                var resultado = db.LiquidarCuentaCorriente(idCliente, totalADebitar, idCaja);

                return Json(new { success = resultado.success, message = resultado.error });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

[HttpPost]
public IActionResult GuardarVentaCtaCte(int idCliente, List<int> ids, List<int> cants)
{
    // 1. Validación de seguridad: si no hay productos, volvemos al índice
    if (ids == null || ids.Count == 0) return RedirectToAction("Index", new { idCliente });

    // 2. Buscamos al cliente en la libreta (ClientesCtaCte) para obtener el "Id Real"
    var clienteCta = db.ObtenerClienteCtaCtePorId(idCliente);
    
    // 3. Verificamos que el cable esté conectado (que el IdClienteOriginal no sea 0)
    if (clienteCta == null || clienteCta.IdClienteOriginal == 0)
    {
        // Si entra aquí, es porque ese cliente no se vinculó correctamente con la tabla maestra
        TempData["Error"] = "Error: El cliente no tiene un ID maestro vinculado en la base de datos.";
        return RedirectToAction("Index", new { idCliente });
    }

    // Este es el ID de la tabla 'clientes' (el 10, 9, 8, etc.) que SQL sí acepta
    int idMaestro = clienteCta.IdClienteOriginal; 
    decimal totalVentaGeneral = 0;

    // 4. Procesamos cada producto de la venta
    for (int i = 0; i < ids.Count; i++)
    {
        var p = db.ObtenerProductoPorId(ids[i]);
        if (p != null)
        {
            decimal subtotalItem = p.PrecioVenta * cants[i];
            totalVentaGeneral += subtotalItem;
            
            // Armamos el concepto (Ej: "Coca Cola (x2)")
            string conceptoIndividual = $"{p.Nombre} (x{cants[i]})";
            
            // REGISTRAMOS EN EL HISTORIAL usando el ID MAESTRO
            db.RegistrarEnHistorialCtaCte(idMaestro, conceptoIndividual, subtotalItem, p.IdProducto, cants[i]);

            // RESTAMOS DEL STOCK
            db.RestarStockProducto(p.IdProducto, cants[i]);
        }
    }

    // 5. Actualizamos el saldo acumulado en la tabla de la libreta (usando el id de libreta)
    db.ActualizarSaldoClienteCtaCte(idCliente, totalVentaGeneral);

    TempData["Success"] = "Venta guardada con éxito.";
    return RedirectToAction("Index", new { idCliente = idCliente });
}

    }
}