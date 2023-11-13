using API_Pizzeria.Models;

namespace API_Pizzeria.DTOs
{
    public class UpdateEmpleadoDTO
    {
        public int NumEmpleado { get; set; }
        public string Nombre { get; set; }
        public string Telefono { get; set; }
        public string Correo { get; set; }
        public string Contraseña { get; set; }
        public float Sueldo { get; set; }
        public bool Admin { get; set; }
    }
}
