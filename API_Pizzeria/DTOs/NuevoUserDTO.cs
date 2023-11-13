using API_Pizzeria.Models;
using API_Pizzeria.Validaciones;
using System.ComponentModel.DataAnnotations;

namespace API_Pizzeria.DTOs
{
    public class NuevoUserDTO
    {
        public string Nombre { get; set; }

        [Phone(ErrorMessage = "Telefono no válido")]
        [MinLength(10, ErrorMessage = "Telefono no válido")]
        public string Telefono { get; set; }

        [EmailAddress(ErrorMessage = "Correo no válido")]
        public string Correo { get; set; }

        [Password]
        [MinLength(8, ErrorMessage ="Contraseña debe contener al menos 8 caracteres")]
        public string Contraseña { get; set; }
        public bool Admin { get; set; }
    }
}
