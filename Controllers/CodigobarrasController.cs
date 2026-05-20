using Microsoft.AspNetCore.Mvc;
using GestionVentas.Models;
using ZXing;
using ZXing.Common;
using System.Drawing;
using System.IO;

public class CodigobarrasController : Controller
{
    private readonly ConexionDB conexion;

    public CodigobarrasController(ConexionDB conexion)
    {
        this.conexion = conexion;
    }

    // GENERAR CÓDIGO AUTOMÁTICO
    [HttpGet]
    public JsonResult GenerarCodigo()
    {
        Random random = new Random();

        string codigo;

        do
        {
            codigo = "779" + random.Next(100000000, 999999999);
        }
        while (conexion.ObtenerProductos().Any(p => p.Codigo == codigo));

        return Json(new { codigo });
    }

    // ABRIR VISTA DE IMPRESIÓN
   [HttpGet]
public IActionResult Imprimir(int? id)
{
    ViewBag.Productos = conexion.ObtenerProductos();

    if (id == null)
    {
        return View();
    }

    var producto = conexion.ObtenerProductos()
        .FirstOrDefault(p => p.IdProducto == id);

    if (producto == null)
    {
        return NotFound();
    }

    var model = new ImprimirCodigoBarrasViewModel
    {
        ProductoId = producto.IdProducto,
        NombreProducto = producto.Nombre,
        Codigo = producto.Codigo,
        Cantidad = 1
    };

    return View(model);
}

    // POST DE IMPRESIÓN
    [HttpPost]
    public IActionResult Imprimir(ImprimirCodigoBarrasViewModel model)
    {
        return View("VistaImpresion", model);
    }

    // GENERAR IMAGEN CODE-128
    [HttpGet]
    public IActionResult GenerarImagen(string codigo)
    {
        var writer = new BarcodeWriterPixelData
        {
            Format = BarcodeFormat.CODE_128,
            Options = new EncodingOptions
            {
                Width = 500,
                Height = 160,
                Margin = 4
            }
        };

        var pixelData = writer.Write(codigo);

        using var bitmap = new Bitmap(pixelData.Width, pixelData.Height);

        for (int y = 0; y < pixelData.Height; y++)
        {
            for (int x = 0; x < pixelData.Width; x++)
            {
                int index = (y * pixelData.Width + x) * 4;

                var color = pixelData.Pixels[index] == 0
                    ? Color.Black
                    : Color.White;

                bitmap.SetPixel(x, y, color);
            }
        }

        using var ms = new MemoryStream();

        bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

        return File(ms.ToArray(), "image/png");
    }
}