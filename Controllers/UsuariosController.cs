using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GestionVentas.Models;
using System.Security.AccessControl;
using Microsoft.AspNetCore.Identity;

namespace GestionVentas.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly ConexionDB db;
        private readonly IWebHostEnvironment _env;

        public UsuariosController(IWebHostEnvironment env, IConfiguration config)
        {
            _env = env;
            db = new ConexionDB(config);
        }

        public ActionResult Index(string searchString, string sortOrder)
        {
            var nombreUsuario = HttpContext.Session.GetString("Usuario");
            if (string.IsNullOrEmpty(nombreUsuario))
                return RedirectToAction("Index", "Login");

            var usuario = db.ObtenerUsuarioPorNombre(nombreUsuario);
            if (usuario == null) return RedirectToAction("Index", "Login");

            if (string.IsNullOrEmpty(usuario.FotoPerfil))
                usuario.FotoPerfil = "/imagenes/usuarios/default.png";

            ViewBag.Usuario = usuario.UsuarioNombre;
            ViewBag.FotoPerfil = usuario.FotoPerfil;
            ViewBag.Apellido = usuario.NombreyApellido;
            ViewBag.Rol = usuario.Rol;

            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.RolSortParm = sortOrder == "Rol" ? "rol_desc" : "Rol";
            ViewBag.SearchString = searchString;

            var lista = db.ObtenerUsuarios();

            foreach (var u in lista)
            {
                if (string.IsNullOrEmpty(u.FotoPerfil))
                    u.FotoPerfil = "/imagenes/usuarios/default.png";
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                var lowerSearch = searchString.ToLower();
                lista = lista.Where(u => u.NombreyApellido != null &&
                                         u.NombreyApellido.ToLower().Contains(lowerSearch)).ToList();
            }

            switch (sortOrder)
            {
                case "name_desc":
                    lista = lista.OrderByDescending(u => u.NombreyApellido).ToList();
                    break;
                case "Rol":
                    lista = lista.OrderBy(u => u.Rol).ToList();
                    break;
                case "rol_desc":
                    lista = lista.OrderByDescending(u => u.Rol).ToList();
                    break;
                default:
                    lista = lista.OrderBy(u => u.NombreyApellido).ToList();
                    break;
            }

            return View(lista);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(Usuario usuario)
        {
            if (!ModelState.IsValid)
            {
                TempData["Errores"] = string.Join("<br/>", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return View(usuario);
            }

            if (usuario.FotoSubida != null && usuario.FotoSubida.Length > 0)
            {
                string rutaFotos = Path.Combine(_env.WebRootPath, "imagenes", "usuarios");

                // Fuerza la creación de la carpeta si por alguna razón no existe
                if (!Directory.Exists(rutaFotos))
                {
                    Directory.CreateDirectory(rutaFotos);
                }

                DirectoryInfo dInfo = new DirectoryInfo(rutaFotos);
                DirectorySecurity dSecurity = dInfo.GetAccessControl();
                // Aquí se intentaría añadir permisos, pero es más efectivo hacerlo desde el panel.

                //    Directory.CreateDirectory(rutaFotos);
                string nombreArchivo = Guid.NewGuid() + Path.GetExtension(usuario.FotoSubida.FileName).ToLower();
                string rutaCompleta = Path.Combine(rutaFotos, nombreArchivo);

                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                    await usuario.FotoSubida.CopyToAsync(stream);

                usuario.FotoPerfil = "/imagenes/usuarios/" + nombreArchivo;
            }
            else
            {
                usuario.FotoPerfil = "/imagenes/usuarios/default.png";
            }
            var hasher = new PasswordHasher<Usuario>();
            usuario.PasswordHash = hasher.HashPassword(usuario, usuario.Contraseña);
            db.AgregarUsuario(usuario);
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var usuario = db.ObtenerUsuarioPorId(id);
            return usuario == null ? NotFound() : View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Usuario usuario, IFormFile? FotoNueva)
        {
            if (!ModelState.IsValid) return View(usuario);

            var usuarioOriginal = db.ObtenerUsuarioPorId(usuario.IdUsuario);
            if (usuarioOriginal == null) return NotFound();

            if (FotoNueva != null && FotoNueva.Length > 0)
            {
                string rutaCarpeta = Path.Combine(_env.WebRootPath, "imagenes", "usuarios");
                Directory.CreateDirectory(rutaCarpeta);
                string nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(FotoNueva.FileName).ToLower();
                string rutaCompleta = Path.Combine(rutaCarpeta, nombreArchivo);

                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                    FotoNueva.CopyTo(stream);

                usuarioOriginal.FotoPerfil = "/imagenes/usuarios/" + nombreArchivo;
            }

            usuarioOriginal.UsuarioNombre = usuario.UsuarioNombre;
            usuarioOriginal.NombreyApellido = usuario.NombreyApellido;
            if (!string.IsNullOrWhiteSpace(usuario.Contraseña))
            {
                var hasher = new PasswordHasher<Usuario>();
                usuarioOriginal.PasswordHash = hasher.HashPassword(usuarioOriginal, usuario.Contraseña);
            }
            usuarioOriginal.Rol = usuario.Rol;

            db.ActualizarUsuario(usuarioOriginal);
            return RedirectToAction("Index");
        }

       [HttpGet]
public JsonResult BuscarUsuarios(string filtro)
{
    var lista = db.ObtenerUsuarios();

    if (!string.IsNullOrEmpty(filtro))
    {
        lista = lista.Where(u =>
            u.NombreyApellido != null &&
            u.NombreyApellido.ToLower().Contains(filtro.ToLower()))
            .ToList();
    }

    return Json(lista.Select(u => new
    {
        idUsuario = u.IdUsuario,
        usuarioNombre = u.UsuarioNombre,
        nombreyApellido = u.NombreyApellido,
        rol = u.Rol,
        fotoPerfil = string.IsNullOrEmpty(u.FotoPerfil)
            ? "/imagenes/usuarios/default.png"
            : u.FotoPerfil
    }));
}
        public ActionResult Delete(int id)
        {
            var usuario = db.ObtenerUsuarioPorId(id);
            if (usuario == null)
            {
                TempData["MensajeError"] = "El usuario que intentas eliminar no existe.";
                return RedirectToAction("Index");
            }
            return View(usuario);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Delete(Usuario u)
        {
            try
            {
                db.EliminarUsuario(u.IdUsuario);
                TempData["Mensaje"] = "Usuario eliminado exitosamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "No se pudo eliminar el usuario: " + ex.Message;
                return View(u);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarPerfil(IFormFile FotoNueva, string Usuario, string NuevaContrasena, string accion)
        {
            var rutaFotos = Path.Combine(_env.WebRootPath, "imagenes", "usuarios");
            var usuario = db.ObtenerUsuarioPorNombre(Usuario);

            if (accion == "cargar" && FotoNueva != null && FotoNueva.Length > 0)
            {
                // string nombreArchivo = Guid.NewGuid() + Path.GetExtension(FotoNueva.FileName);
                string nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(FotoNueva.FileName).ToLower();
                string rutaCompleta = Path.Combine(rutaFotos, nombreArchivo);

                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                    await FotoNueva.CopyToAsync(stream);

                usuario.FotoPerfil = "/imagenes/usuarios/" + nombreArchivo;
                HttpContext.Session.SetString("FotoPerfil", usuario.FotoPerfil);
                db.ActualizarRutaFoto(usuario);

                return Json(new { ok = true, nuevaRuta = usuario.FotoPerfil });
            }

            if (accion == "eliminar")
            {
                if (!string.IsNullOrEmpty(usuario.FotoPerfil))
                {
                    string rutaAnterior = Path.Combine(_env.WebRootPath, usuario.FotoPerfil.TrimStart('/'));
                    if (System.IO.File.Exists(rutaAnterior))
                        System.IO.File.Delete(rutaAnterior);
                }

                usuario.FotoPerfil = null;
                db.ActualizarRutaFoto(usuario);
                return Json(new { ok = true });
            }

            if (accion == "guardarPerfil" && !string.IsNullOrEmpty(NuevaContrasena))
            {
                var hasher = new PasswordHasher<Usuario>();
                usuario.PasswordHash = hasher.HashPassword(usuario, NuevaContrasena);

                db.ActualizarClave(usuario);
            }

            return RedirectToAction("Index", "Home");
        }

      [HttpGet]
public IActionResult ResetPasswordAdmin()
{
    var usuario = db.ObtenerUsuarioPorNombre("Mbalestrieri");

    if (usuario == null)
        return Content("Usuario no encontrado");

    var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<Usuario>();

    usuario.PasswordHash = hasher.HashPassword(usuario, "Admin#2026!");

    db.ActualizarClave(usuario);

    return Content("Contraseña reseteada correctamente.");
}


    }
}
