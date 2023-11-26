using API_Pizzeria.Data;
using API_Pizzeria.DTOs;
using API_Pizzeria.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Any;

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
        [AllowAnonymous]
        public async Task<ActionResult<List<VentaProductoDTO>>> GetVentasMes(int mes)
        {
            // Obtiene los id de las ventas realizadas en el mes
            var ventas = _dataContext.Ventas.Where(v => v.Fecha.Month == mes).Select(v => v.Id).ToList();

            // Obtiene los detalles de venta de todos los productos
            var productos = await _dataContext.Productos.Include(p => p.Detalle).ToListAsync();          
            
            var ventasProducto = new List<VentaProductoDTO>();
            productos.ForEach(p =>
            {
                VentaProductoDTO venta = new VentaProductoDTO();
                
                // Si el producto esta presente en algun detalle de venta se llenan los datos segun corresponda
                if(p.Detalle.Count > 0)
                {
                    p.Detalle.ForEach(d =>
                    {
                        venta.Producto = p.Nombre;
                        venta.Precio = p.Costo;

                        if (ventas.IndexOf(d.VentaId) != -1)
                        {
                            venta.CantidadVendida += d.Cantidad;
                            venta.Ganancias = p.Costo * venta.CantidadVendida;
                        }
                    });
                }
                // Si no esta presente en ninguna venta se le asigna 0 en cantidad y ganancias
                else
                {
                    venta.Producto = p.Nombre;
                    venta.Precio = p.Costo;
                    venta.CantidadVendida = 0;
                    venta.Ganancias = 0;
                }

                ventasProducto.Add(venta);
            });          

            return Ok(ventasProducto);
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
