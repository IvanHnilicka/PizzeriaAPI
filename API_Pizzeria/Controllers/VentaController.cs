using API_Pizzeria.Data;
using API_Pizzeria.DTOs;
using API_Pizzeria.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_Pizzeria.Controllers
{
    [Route("ventas")]
    [ApiController]
    [Authorize]
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
        public async Task<ActionResult<List<Venta>>> GetVentasMes(int mes)
        {
            var ventas = await _dataContext.Ventas.Where(v => v.Fecha.Month == mes).ToListAsync();
            return Ok(ventas);
        }


        // Crea una nueva venta
        [HttpPost("{numEmpleado:int}")]
        public async Task<ActionResult> CrearVenta(int numEmpleado)
        {
            Empleado empleado = await _dataContext.Empleados.Where(e => e.NumEmpleado == numEmpleado).FirstAsync();
            DateTime fecha = new();

            if (empleado == null)
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


        // Calcula y actualiza el total de la venta
        [HttpPut("/update")]
        public async Task<ActionResult> UpdateTotal()
        {
            Venta venta = await _dataContext.Ventas.OrderBy(v => v.Id).LastAsync();
            List<DetalleVenta> detalles = await _dataContext.DetalleVentas.Where(d => d.VentaId == venta.Id).ToListAsync();

            float total = 0;
            detalles.ForEach(d =>
            {
                float precio = _dataContext.Productos.FirstOrDefault(p => p.Id == d.ProductoId).Costo;
                total += d.Cantidad * precio;
            });

            venta.Total = total;
            venta.Fecha = DateTime.Now;

            _dataContext.Update(venta);
            await _dataContext.SaveChangesAsync();
            return Ok();
        }
    }

}
