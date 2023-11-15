using API_Pizzeria.Data;
using API_Pizzeria.DTOs;
using API_Pizzeria.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_Pizzeria.Controllers
{
    [Route("detalleVenta")]
    [ApiController]
    [Authorize]
    public class DetalleVentaController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IMapper _mapper;

        public DetalleVentaController(DataContext datacontext, IMapper mapper)
        {
            _dataContext = datacontext;
            _mapper = mapper;
        }

        // Obtiene la lista de detalles de las ventas
        [HttpGet]
        public async Task<ActionResult<List<DetalleVenta>>> getDetalles()
        {
            var detalles = await _dataContext.DetalleVentas.Include(d => d.Producto).ToListAsync();
            List<DetallesDTO> detallesMap = new List<DetallesDTO>();
            detalles.ForEach(detalle =>
            {
                detallesMap.Add(_mapper.Map<DetallesDTO>(detalle));
            });
            return Ok(detallesMap);
        }


        // Crea el detalle de venta de la ultima venta creada
        [HttpPost]
        public async Task<ActionResult> crearDetalleVenta(NuevoDetalleVentaDTO detalleVenta)
        {
            DetalleVenta detalleMap = _mapper.Map<DetalleVenta>(detalleVenta);
            Venta venta = await _dataContext.Ventas.OrderBy(v => v.Id).LastAsync();
            
            detalleMap.VentaId = venta.Id;
            detalleMap.ProductoId = detalleVenta.IdProducto;
            
            _dataContext.Add(detalleMap);
            await _dataContext.SaveChangesAsync();
            return Ok(detalleMap);
        }
    }
}
