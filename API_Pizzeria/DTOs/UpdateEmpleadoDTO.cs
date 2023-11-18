using API_Pizzeria.Models;

namespace API_Pizzeria.DTOs
{
    public class UpdateEmpleadoDTO
    {
        public string Nombre { get; set; }
        public string Telefono { get; set; }
        public string Correo { get; set; }
        public string Contraseña { get; set; }
        public bool Admin { get; set; }
    }
}
