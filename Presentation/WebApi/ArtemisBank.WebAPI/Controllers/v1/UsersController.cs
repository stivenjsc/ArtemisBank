using ArtemisBank.Core.Application.DTOs.User;
using ArtemisBank.Core.Application.Interfaces.IServices;
using ArtemisBank.Core.Domain.Enums;
using ArtemisBank.WebAPI.DTOs.User;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBank.WebAPI.Controllers.v1
{
    [ApiVersion("1.0")]
    [Authorize(Roles = "Admin")]
    public class UsersController : BaseApiController
    {
        private readonly IUserService _userService;
        private readonly ISavingsAccountService _savingsAccountService;
        private readonly ICommerceService _commerceService;

        public UsersController(
            IUserService userService,
            ISavingsAccountService savingsAccountService,
            ICommerceService commerceService)
        {
            _userService = userService;
            _savingsAccountService = savingsAccountService;
            _commerceService = commerceService;
        }

        /// <summary>
        /// Obtiene un listado paginado de usuarios (excepto usuarios con rol Comercio).
        /// </summary>
        /// <param name="page">Número de página</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <param name="role">Filtro opcional por rol (administrador, cajero, cliente)</param>
        /// <response code="200">Listado retornado exitosamente</response>
        /// <response code="401">Token ausente o inválido</response>
        /// <response code="403">Usuario sin permisos de administrador</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetUsers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? role = null)
        {
            UserRole? roleFilter = null;
            if (!string.IsNullOrEmpty(role))
            {
                roleFilter = role.ToLower() switch
                {
                    "administrador" => UserRole.Admin,
                    "cajero" => UserRole.Cashier,
                    "cliente" => UserRole.Client,
                    _ => null
                };
            }

            var result = await _userService.GetAllAsync(page, pageSize, roleFilter);

            return Ok(result);
        }

        /// <summary>
        /// Obtiene un listado paginado de usuarios con rol Comercio.
        /// </summary>
        /// <param name="page">Número de página</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <response code="200">Listado retornado exitosamente</response>
        /// <response code="401">Token ausente o inválido</response>
        /// <response code="403">Usuario sin permisos de administrador</response>
        [HttpGet("commerce")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetCommerceUsers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _userService.GetCommerceUsersAsync(page, pageSize);

            return Ok(result);
        }

        /// <summary>
        /// Crea un nuevo usuario (administrador, cajero o cliente).
        /// </summary>
        /// <param name="request">Datos del usuario a crear</param>
        /// <response code="201">Usuario creado exitosamente</response>
        /// <response code="400">Datos faltantes o inválidos</response>
        /// <response code="409">Usuario o correo ya registrado</response>
        /// <response code="401">Token ausente o inválido</response>
        /// <response code="403">Usuario sin permisos de administrador</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.RegisterAsync(
                request.FirstName,
                request.LastName,
                request.Cedula,
                request.UserName,
                request.Email,
                request.Password,
                request.Role,
                request.InitialAmount);

            if (!result)
                return Conflict(new { message = "Username or email already exists." });

            return StatusCode(201, new { message = "User created successfully." });
        }

        /// <summary>
        /// Crea un nuevo usuario de comercio asociado a un comercio específico.
        /// </summary>
        /// <param name="commerceId">ID del comercio</param>
        /// <param name="request">Datos del usuario de comercio</param>
        /// <response code="201">Usuario de comercio creado exitosamente</response>
        /// <response code="400">Datos faltantes, inválidos o el comercio ya tiene un usuario</response>
        /// <response code="409">Usuario o correo ya registrado</response>
        /// <response code="401">Token ausente o inválido</response>
        /// <response code="403">Usuario sin permisos de administrador</response>
        [HttpPost("commerce/{commerceId}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateCommerceUser(
            [FromRoute] int commerceId,
            [FromBody] CreateCommerceUserRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var commerce = await _commerceService.GetByIdAsync(commerceId);
            if (commerce == null)
                return BadRequest(new { message = "Commerce not found." });

            var hasUser = await _commerceService.CommerceHasActiveUserAsync(commerceId);
            if (hasUser)
                return BadRequest(new { message = "This commerce already has an active user." });

            var result = await _userService.RegisterCommerceUserAsync(
                request.FirstName,
                request.LastName,
                request.Cedula,
                request.UserName,
                request.Email,
                request.Password,
                commerceId);

            if (!result)
                return Conflict(new { message = "Username or email already exists." });

            return StatusCode(201, new { message = "Commerce user created successfully." });
        }

        /// <summary>
        /// Actualiza los datos de un usuario existente.
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <param name="request">Datos actualizados del usuario</param>
        /// <response code="204">Usuario actualizado correctamente</response>
        /// <response code="400">Errores de validación</response>
        /// <response code="404">Usuario no encontrado</response>
        /// <response code="409">Correo o usuario duplicado</response>
        /// <response code="401">Token ausente o inválido</response>
        /// <response code="403">Usuario sin permisos de administrador</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateUser(
            [FromRoute] string id,
            [FromBody] UpdateUserRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userService.GetByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found." });

            if (!string.IsNullOrEmpty(request.Password) && request.Password != request.ConfirmPassword)
                return BadRequest(new { message = "Passwords do not match." });

            var updateDto = new UpdateUserDto
            {
                Id = id,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Cedula = request.Cedula,
                Email = request.Email,
                Username = request.UserName,
                Password = request.Password,
                ConfirmPassword = request.ConfirmPassword,
                AdditionalAmount = request.AdditionalAmount
            };

            var result = await _userService.UpdateAsync(updateDto);

            if (!result)
                return Conflict(new { message = "Username or email already exists." });

            return NoContent();
        }

        /// <summary>
        /// Cambia el estado (activo/inactivo) de un usuario.
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <param name="request">Estado a asignar</param>
        /// <response code="204">Estado cambiado correctamente</response>
        /// <response code="400">Estructura inválida</response>
        /// <response code="403">Intento de auto-modificación o no tiene permisos</response>
        /// <response code="404">Usuario no encontrado</response>
        /// <response code="401">Token ausente o inválido</response>
        [HttpPatch("{id}/status")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangeUserStatus(
            [FromRoute] string id,
            [FromBody] ChangeUserStatusRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var adminId = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(adminId))
                return Unauthorized();

            if (adminId == id)
                return StatusCode(403, new { message = "You cannot modify your own status." });

            var user = await _userService.GetByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found." });

            var result = await _userService.ChangeStatusAsync(adminId, id, request.Status);

            if (!result)
                return BadRequest(new { message = "Unable to change user status." });

            return NoContent();
        }

        /// <summary>
        /// Obtiene el detalle de un usuario específico.
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <response code="200">Detalle retornado</response>
        /// <response code="404">Usuario no encontrado</response>
        /// <response code="401">Token ausente o inválido</response>
        /// <response code="403">Usuario sin permisos de administrador</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetUserById([FromRoute] string id)
        {
            var user = await _userService.GetByIdAsync(id);

            if (user == null)
                return NotFound(new { message = "User not found." });

            return Ok(user);
        }
    }
}
