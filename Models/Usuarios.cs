
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace GestionVentas.Models
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string UsuarioNombre { get; set; }
        public string Contraseña { get; set; }  
        public string? PasswordHash { get; set; }
        public string Rol { get; set; }
        public string NombreyApellido { get; set; }
        public string? FotoPerfil { get; set; }
        [NotMapped]
        public IFormFile? FotoSubida { get; set; }
    }
}
