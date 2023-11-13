using API_Pizzeria.Data;
using API_Pizzeria.DTOs;
using API_Pizzeria.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_Pizzeria.Controllers
{
    [Route("ventas")]
    [ApiController]
    public class VentaController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IMapper _mapper;

        public VentaController(DataContext datacontext, IMapper mapper)
        {
            _dataContext = datacontext;
            _mapper = mapper;
        }


        // Retorna la lista de ventas
        [HttpGet]
        public async Task<ActionResult> getVentas()
        {
            // Obtiene la lista de ventas incluyendo los datos del empleado que la realizó
            var ventas = await _dataContext.Ventas.Include(v => v.Empleado).ToListAsync();
            List<VentaDTO> ventasMap = new List<VentaDTO>();
            ventas.ForEach(venta =>
            {
                VentaDTO ventaMapped = _mapper.Map<VentaDTO>(venta);
                ventaMapped.IdEmpleado = venta.Empleado.Id;
                ventasMap.Add(ventaMapped);
            });

            return Ok(ventasMap);
        }


        // Obtiene la lista de ventas por el numero de mes
        [HttpGet("mes/{mes:int}")]
        public async Task<ActionResult<List<Venta>>> getVentasMes(int mes)
        {
            var ventas = await _dataContext.Ventas.Where(v => v.Fecha.Month == mes).ToListAsync();
            return Ok(ventas);
        }


        // Crea una nueva venta
        [HttpPost("{numEmpleado:int}")]
        public async Task<ActionResult> crearVenta(int numEmpleado)
        {
            Empleado empleado = await _dataContext.Empleados.Where(e => e.NumEmpleado == numEmpleado).FirstAsync();
            DateTime fecha = DateTime.Now;

            if(empleado == null)
            {
                return BadRequest("Numero de empleado no encontrado");
            }

            Venta venta = new Venta()
            {
                Empleado = empleado,
                Fecha = fecha,
                Total = 0,
            };

            _dataContext.Add(venta);
            await _dataContext.SaveChangesAsync();
            return Ok(venta);
        }
    }

}
