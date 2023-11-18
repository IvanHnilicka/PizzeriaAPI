using API_Pizzeria.Models;

namespace API_Pizzeria.DTOs
{
    public class NuevoEmpleadoDTO
    {
        public int NumEmpleado { get; set; }
        public string Nombre { get; set; }
        public string Telefono { get; set; }
        public string Correo { get; set; }
        public bool Admin { get; set; }
    }
}
