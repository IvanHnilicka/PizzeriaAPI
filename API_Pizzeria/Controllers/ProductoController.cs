using API_Pizzeria.Data;
using API_Pizzeria.DTOs;
using API_Pizzeria.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_Pizzeria.Controllers
{
    [Route("productos")]
    [ApiController]
    [Authorize]
    public class ProductoController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IMapper _mapper;

        public ProductoController(DataContext dataContext, IMapper mapper)
        {
            _dataContext = dataContext;
            _mapper = mapper;
        }


        // Retorna la lista de productos
        [HttpGet]
        public async Task<ActionResult<List<Producto>>> GetProductos() 
        {
            // Obtiene lista de productos y los mapea para no incluir DetalleVentas a los que pertenece
            var productos = await _dataContext.Productos.ToListAsync();
            var productosMap = new List<ProductoDTO>();
            productos.ForEach(p =>
            {
                productosMap.Add(_mapper.Map<ProductoDTO>(p));
            });
            return Ok(productosMap);
        }


        [HttpGet("{id:int}")]
        public async Task<ActionResult> GetProductoById(int id)
        {
            var existeProducto = await _dataContext.Productos.AnyAsync(p => p.Id == id);
            if(!existeProducto)
            {
                return BadRequest();
            }

            Producto producto = await _dataContext.Productos.FirstAsync(p => p.Id == id);
            ProductoDTO datosProducto = _mapper.Map<ProductoDTO>(producto);
            return Ok(datosProducto);
        }


        // Agrega un nuevo producto
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult> AgregarProducto(NuevoProductoDTO productoNuevo)
        {
            // Verifica que no exista algun producto con el mismo nombre
            var producto = await _dataContext.Productos.AnyAsync(p => p.Nombre.Trim().ToUpper() == productoNuevo.Nombre.TrimEnd().ToUpper());
            if (producto)
            {
                return BadRequest("Producto ya existe");
            }

            // Guarda el nuevo producto en la base de datos
            _dataContext.Add(_mapper.Map<Producto>(productoNuevo));
            await _dataContext.SaveChangesAsync();
            return Ok();
        }


        [HttpPut("{id:int}")]
        public async Task<ActionResult> UpdateProducto(int id, NuevoProductoDTO datosProducto)
        {
            var existeProducto = await _dataContext.Productos.AnyAsync(p => p.Id == id);
            if (!existeProducto)
            {
                return BadRequest();
            }

            Producto producto = await _dataContext.Productos.FirstAsync(p => p.Id == id);
            producto.Nombre = datosProducto.Nombre;
            producto.Costo = datosProducto.Costo;

            _dataContext.Update(producto);
            await _dataContext.SaveChangesAsync();
            return Ok();
        }


        [Authorize(Roles = "Admin")]
        [HttpDelete("{nombre}")]
        public async Task<ActionResult> DeleteProducto(string nombre)
        {
            var producto = await _dataContext.Productos.FirstOrDefaultAsync(p => p.Nombre.Trim().ToUpper() == nombre.TrimEnd().ToUpper());
            if(producto == null)
            {
                return BadRequest("Producto no encontrado");
            }

            _dataContext.Remove(producto);
            await _dataContext.SaveChangesAsync();
            return Ok();
        }
    }
}
