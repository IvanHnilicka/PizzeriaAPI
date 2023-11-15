using API_Pizzeria.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Any;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API_Pizzeria.Controllers
{
    [Route("authentication")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthenticationController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }


        // Obtiene lista de usuarios
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult> GetUsuarios()
        {
            var usuarios = await _userManager.Users.ToListAsync();
            List<DatosUsuarioDTO> datosUsuarios = new List<DatosUsuarioDTO>();

            // Mapea manualmente los datos de los usuarios
            foreach(var usuario in usuarios)
            {
                // Verificamos si el usuario tiene rol de administrador para asignarlo a la propiedad Admin
                var rolesUsuario = await _userManager.GetRolesAsync(usuario);
                bool isAdmin = rolesUsuario.Contains("Admin");

                DatosUsuarioDTO usuarioMap = new DatosUsuarioDTO
                {
                    NumEmpleado = int.Parse(usuario.Id),
                    Nombre = usuario.UserName.Replace('_', ' '),
                    Correo = usuario.Email,
                    Telefono = usuario.PhoneNumber,
                    Admin = isAdmin,
                };

                datosUsuarios.Add(usuarioMap);
            };

            return Ok(datosUsuarios);
        }


        // Obtiene los datos del usuario por su numero de empleado
        [HttpGet("{numEmpleado:int}")]
        public async Task<ActionResult> GetUsuarioById(int numEmpleado)
        {
            var usuario = await _userManager.FindByIdAsync(numEmpleado.ToString());
            if(usuario == null)
            {
                return BadRequest("Empleado no encontrado");
            }

            // Verificamos si el usuario tiene rol de administrador para asignarlo a la propiedad Admin
            var rolesUsuario = await _userManager.GetRolesAsync(usuario);
            bool isAdmin = rolesUsuario.Contains("Admin");

            // Mapeo manual de los datos del usuario
            DatosUsuarioDTO datosUsuario = new DatosUsuarioDTO
            {
                NumEmpleado = int.Parse(usuario.Id),
                Nombre = usuario.UserName.Replace('_', ' '),
                Correo = usuario.Email,
                Telefono = usuario.PhoneNumber,
                Admin = isAdmin,
            };

            return Ok(datosUsuario);
        }

        [HttpGet("nombreUser")]
        public async Task<ActionResult> GetNombreLoggedUser()
        {
            int numEmpleado = getNumEmpleadoLoggeado();
            if(numEmpleado == -1)
            {
                return BadRequest();
            }

            var datosEmpleado = GetUsuarioById(numEmpleado);
            if(datosEmpleado == null)
            {
                return BadRequest();
            }

            return Ok(datosEmpleado.Result);
        }


        // Retorna el numero de empleado del usuario loggeado
        [HttpGet("loggedId")]
        public int getNumEmpleadoLoggeado()
        {
            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(token);
                    int numEmpleado = int.Parse(jwtToken.Claims.FirstOrDefault(c => c.Type == "numEmpleado").Value);
                    return numEmpleado != null ? numEmpleado : -1;
                
                }catch(Exception ex)
                {
                    return -1;
                }

            }

            return -1;
        }


        [HttpGet("isAdmin")]
        public bool isAdmin()
        {
            int numEmpleado = getNumEmpleadoLoggeado();
            if (numEmpleado == -1)
            {
                return false;
            }

            var usuario = _userManager.Users.FirstOrDefault(u => u.Id == numEmpleado.ToString());
            if (usuario == null)
            {
                return false;
            }

            var roles = _userManager.GetRolesAsync(usuario).Result;
            return roles.Contains("Admin");
        }


        // Crea un nuevo usuario
        [Authorize(Roles = "Admin")]
        [HttpPost("register")]
        public async Task<ActionResult> RegistrarUsuario(NuevoUserDTO newUser)
        {
            // Valida que no exista el numero de empleado
            int numEmpleado = createNumEmpleado();
            while(await _userManager.FindByIdAsync(numEmpleado.ToString()) != null)
            {
                numEmpleado = createNumEmpleado();
            }

            // Mapea manualmente los datos del empleado al tipo IdentityUser
            IdentityUser datosUsuario = new()
            {
                Id = createNumEmpleado().ToString(),
                Email = newUser.Correo,
                UserName = newUser.Nombre.Replace(' ','_'),
                PhoneNumber = newUser.Telefono,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            // Crea el usuario
            var result = await _userManager.CreateAsync(datosUsuario, newUser.Contraseña);
            if (!result.Succeeded)
            {
                return Problem("Ha ocurrido un error");
            }

            // Agrega rol correspondiente
            if (newUser.Admin)
            {
                await _userManager.AddToRoleAsync(datosUsuario, "Admin");
            }
            else
            {
                await _userManager.AddToRoleAsync(datosUsuario, "User");
            }

            return Ok("Usuario creado");
        }

        // Genera el numero de Empleado aleatoriamente
        private int createNumEmpleado()
        {
            Random random = new Random();
            var Id = random.Next(1000, 9999);
            return Id;
        }


        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginDTO datosLogin)
        {
            // Obtiene los datos del usuario por su numero de empleado
            var user = await _userManager.FindByIdAsync(datosLogin.NumEmpleado.ToString());
            if(user != null && await _userManager.CheckPasswordAsync(user, datosLogin.Contraseña))
            {
                // Lista de claims del token
                var authClaims = new List<Claim>
                {
                    new Claim("numEmpleado", user.Id),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                // Agrega roles en la lista de claims
                var userRoles = await _userManager.GetRolesAsync(user);
                foreach(var role in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, role));
                }
                
                // Obtiene y retorna el token
                var jwtToken = GetToken(authClaims);
                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                    expiration = jwtToken.ValidTo
                });
            }
            
            return BadRequest("Credenciales incorrectas");
        }

        // Crea el token de login
        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(8),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }


        // Actualiza los datos del usuario
        [Authorize(Roles = "Admin")]
        [HttpPut("{numEmpleado:int}")]
        public async Task<ActionResult> UpdateUsuario(int numEmpleado, NuevoUserDTO nuevosDatos)
        {
            // Busca al usuario por su numero de Empleado
            var user = await _userManager.FindByIdAsync(numEmpleado.ToString());
            if(user == null)
            {
                return BadRequest("Numero de empleado no encontrado");
            }

            // Actualiza los datos del usuario con los nuevos datos
            user.UserName = nuevosDatos.Nombre.Replace(' ', '_');
            user.NormalizedUserName = user.UserName.ToUpper();
            user.Email = nuevosDatos.Correo;
            user.NormalizedEmail = user.Email.ToUpper();
            user.PhoneNumber = nuevosDatos.Telefono;
            user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, nuevosDatos.Contraseña);

            // Cambia los roles si son modificados
            var isAdmin = await _userManager.GetRolesAsync(user);
            if(nuevosDatos.Admin && !isAdmin.Contains("Admin"))
            {
                await _userManager.AddToRoleAsync(user, "Admin");
                await _userManager.RemoveFromRoleAsync(user, "User");
            }
            else if(!nuevosDatos.Admin && isAdmin.Contains("Admin"))
            {
                await _userManager.RemoveFromRoleAsync(user, "Admin");
                await _userManager.AddToRoleAsync(user, "User");
            }

            // Actualiza los datos en la base de datos
            var res = await _userManager.UpdateAsync(user);
            if(res != null)
            {
                return Ok("Los datos han sido actualizados");
            }
            else
            {
                return Problem("Ha ocurrido un error");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{numEmpleado:int}")]
        public async Task<ActionResult> DeleteUsuario(int numEmpleado)
        {
            // Obtiene el usuario por su numero de Empleado
            var user = await _userManager.FindByIdAsync(numEmpleado.ToString());
            if(user == null)
            {
                return BadRequest("Numero de empleado no encontrado");
            }

            // Elimina al usuario de la base de datos
            var result = await _userManager.DeleteAsync(user);
            if(result == null)
            {
                return Problem("Ha ocurrido un error");
            }
            else
            {
                return Ok("Usuario " + numEmpleado + " eliminado");
            }
        }
    }
}
