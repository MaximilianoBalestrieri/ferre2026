using Microsoft.AspNetCore.Mvc;
using GestionVentas.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using GestionVentas.Helpers;
namespace GestionVentas.Controllers
{
  //  [Authorize] 
    public class HomeController : Controller
    {
        private readonly ConexionDB db;

        public HomeController(ConexionDB db)
        {
            this.db = db;
        }

[AllowAnonymous] // Esto asegura que cualquiera pueda ver la landing
        public IActionResult Inicio()
        {
            return View();
        }

        [Authorize]
        public IActionResult Index()
        {
            var usuario = HttpContext.Session.GetString("Usuario");
            var nombreyApellido = HttpContext.Session.GetString("NombreyApellido");
            var rol = HttpContext.Session.GetString("Rol");
            var fotoPerfil = HttpContext.Session.GetString("FotoPerfil");

            if (string.IsNullOrEmpty(usuario))
            {
                return RedirectToAction("Index", "Login");
            }

            ViewBag.Usuario = usuario;
            ViewBag.NombreyApellido = nombreyApellido;
            ViewBag.Rol = rol;
            ViewBag.FotoPerfil = fotoPerfil;

            return View();
        }

        [Authorize]
        public IActionResult Perfil()
        {
            var usuarioNombre = HttpContext.Session.GetString("Usuario");
            if (string.IsNullOrEmpty(usuarioNombre))
            {
                return RedirectToAction("Index", "Login");
            }

            var usuario = db.ObtenerUsuarioPorNombre(usuarioNombre);
            if (usuario == null)
            {
                return RedirectToAction("Index", "Login");
            }

            ViewBag.Usuario = usuario.UsuarioNombre;
            ViewBag.NombreyApellido = usuario.NombreyApellido;
            ViewBag.Rol = usuario.Rol;
            ViewBag.FotoPerfil = usuario.FotoPerfil ?? "/imagenes/usuarios/default.png";

            return View();
        }

        public IActionResult About()
        {
            ViewBag.Message = "Descripción del Software.";
            return View();
        }

        public ActionResult Acercade()
        {
            return View();
        }
    }
}
