using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using GestionVentas.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace GestionVentas.Controllers
{
    public class ProductosVendidosController : Controller
    {
        private readonly ConexionDB db;

        public ProductosVendidosController(IConfiguration config)
        {
            db = new ConexionDB(config);
        }
public ActionResult Index(DateTime? desde, DateTime? hasta)
{
    List<ProductoVendidoViewModel> lista = new List<ProductoVendidoViewModel>();

    ViewBag.Desde = desde?.ToString("yyyy-MM-dd");
    ViewBag.Hasta = hasta?.ToString("yyyy-MM-dd");

    // Validar que la fecha Desde no sea mayor que Hasta
if (desde.HasValue && hasta.HasValue && desde.Value.Date > hasta.Value.Date)
{
    TempData["Error"] = "La fecha 'Desde' no puede ser mayor que la fecha 'Hasta'.";
    return View(lista);
}

    // Al ingresar por primera vez no mostramos ningún registro
    if (!desde.HasValue && !hasta.HasValue)
    {
        return View(lista);
    }

    using (SqlConnection conn = db.ObtenerConexion())
    {
        conn.Open();

        string consulta = @"
            SELECT
                fi.nombreProd AS ProductoNombre,
                SUM(fi.Cantidad) AS TotalCantidad,
                fi.Precio AS PrecioUnitario
            FROM facturaitem fi
            INNER JOIN facturas f ON fi.IdFactura = f.idFactura
            WHERE 1 = 1
        ";

        if (desde.HasValue)
            consulta += " AND f.diaVenta >= @desde ";

        if (hasta.HasValue)
            consulta += " AND f.diaVenta < @hastaMasUnDia ";

        consulta += @"
            GROUP BY fi.nombreProd, fi.Precio
            ORDER BY TotalCantidad DESC";

        using (SqlCommand cmd = new SqlCommand(consulta, conn))
        {
            if (desde.HasValue)
                cmd.Parameters.Add("@desde", SqlDbType.DateTime).Value = desde.Value.Date;

            if (hasta.HasValue)
                cmd.Parameters.Add("@hastaMasUnDia", SqlDbType.DateTime).Value = hasta.Value.Date.AddDays(1);

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    lista.Add(new ProductoVendidoViewModel
                    {
                        Nombre = reader["ProductoNombre"].ToString(),
                        Cantidad = Convert.ToInt32(reader["TotalCantidad"]),
                        PrecioUnitario = Convert.ToDecimal(reader["PrecioUnitario"])
                    });
                }
            }
        }
    }

    return View(lista);
}

    }
}

