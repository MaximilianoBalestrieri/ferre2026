using GestionVentas.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Http;

namespace GestionVentas.Controllers
{
    public class StockController : Controller
    {
        private readonly ConexionDB db;

        public StockController(IConfiguration config)
        {
            db = new ConexionDB(config);
        }

        // Carga la página principal
        public IActionResult Index()
        {
            ViewBag.FotoPerfil = HttpContext.Session.GetString("FotoPerfil");
            return View();
        }

        // Retorna la lista de productos para el buscador de Vue.js
    [HttpGet]
public JsonResult ObtenerProductos()
{
    List<Productos> productos = new List<Productos>();

    using (SqlConnection conn = db.ObtenerConexion())
    {
        conn.Open();

        string consulta = @"
            SELECT IdProducto, Nombre, Codigo, StockActual, StockMinimo, NombreProveedor
            FROM productos
            ORDER BY Nombre ASC";

        using (SqlCommand cmd = new SqlCommand(consulta, conn))
        {
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    productos.Add(new Productos
                    {
                        IdProducto = reader.GetInt32(0),
                        Nombre = reader.GetString(1),
                        Codigo = reader.GetString(2),
                        StockActual = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                        StockMinimo = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                        NombreProveedor = reader.IsDBNull(5) ? "Sin Proveedor" : reader.GetString(5)
                    });
                }
            }
        }
    }

    return Json(productos);
}

        // Método para sumar stock a la base de datos
        [HttpPost]
        public IActionResult SumarStock([FromBody] StockRequest req)
        {
            try
            {
                using (SqlConnection conn = db.ObtenerConexion())
                {
                    conn.Open();
                    // Usamos UPDATE con suma directa para evitar problemas de concurrencia
                    string consulta = "UPDATE productos SET StockActual = StockActual + @cantidad WHERE IdProducto = @id";

                    using (SqlCommand cmd = new SqlCommand(consulta, conn))
                    {
                        cmd.Parameters.AddWithValue("@cantidad", req.Cantidad);
                        cmd.Parameters.AddWithValue("@id", req.IdProducto);

                        int filasAfectadas = cmd.ExecuteNonQuery();

                        if (filasAfectadas > 0)
                            return Ok(new { success = true });
                        else
                            return BadRequest(new { error = "No se encontró el producto." });
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}