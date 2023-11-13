using API_Pizzeria.Models;

namespace API_Pizzeria.DTOs
{
    public class VentaDTO
    {
        public int Id { get; set; }
        public int IdEmpleado { get; set; }
        public DateTime Fecha { get; set; }
        public float Total { get; set; }
    }
}
