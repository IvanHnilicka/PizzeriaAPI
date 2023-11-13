using API_Pizzeria.DTOs;
using API_Pizzeria.Models;
using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace API_Pizzeria.Mapper
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<ProductoDTO, Producto>();
            CreateMap<Producto, ProductoDTO>();
            CreateMap<Empleado, DatosEmpleadoDTO>();
            CreateMap<NuevoEmpleadoDTO, Empleado>();
            CreateMap<UpdateEmpleadoDTO, Empleado>();
            CreateMap<Empleado, UpdateEmpleadoDTO>();
            CreateMap<DetalleVenta, DetallesDTO>();
            CreateMap<Venta, VentaDTO>();
        }
    }
}
