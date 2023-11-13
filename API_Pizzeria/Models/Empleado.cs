using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace API_Pizzeria.Models
{
    public class Empleado
    {
        public int Id { get; set; }
        public int NumEmpleado { get; set; }
        public string Nombre { get; set; }
        public string Telefono { get; set; }
        public string Correo { get; set; }
        public bool Admin { get; set; }
        public ICollection<Venta> Ventas { get; }
    }
}
