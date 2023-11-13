using System.ComponentModel.DataAnnotations;

namespace API_Pizzeria.Models
{
    public class Venta
    {
        public int Id { get; set; }
        public Empleado Empleado { get; set; }
        public DateTime Fecha { get; set; }
        public float Total { get; set; }
        public List<DetalleVenta> Detalles { get; }
    }
}
