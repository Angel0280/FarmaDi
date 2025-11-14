using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.DTOs.SupplierDto;
using FarmaDiBusiness.Interfaces;
using FarmaDiBusiness.Services;
using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;


namespace FarmaDiApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SuppliersController : ControllerBase
    {

        private readonly ISupplierService _SuppliersService;

        //Constructor del controlador
        public SuppliersController(ISupplierService supplier)
        {
            _SuppliersService = supplier;
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddSupplierDto addsupplierDto)
        {
            var serviceResponse = await _SuppliersService.AddAsync(addsupplierDto);

            if (serviceResponse.IsSuccess)
            {
                var supplierDto = new SupplierDto
                {
                    SupplierId = serviceResponse.Data!.SupplierId,
                    SupplierName = serviceResponse.Data!.SupplierName,
                    RNC = serviceResponse!.Data.RNC,
                    Mail = serviceResponse!.Data.Mail,
                    SupplierPhone = serviceResponse!.Data.SupplierPhone,
                    SupplierAddress = serviceResponse!.Data.SupplierAddress,
                    IsActive = serviceResponse!.Data.IsActive,
      
                };
                return CreatedAtAction(
                    nameof(GetById),
                    new { id = supplierDto.SupplierId },
                    supplierDto);


            }
            var unSuccessfulResponse = new UnsuccessfulResponseDto();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.Conflict:
                    unSuccessfulResponse.Code = "409";
                    unSuccessfulResponse.Message = "El nombre de la  marca ya existe";
                    unSuccessfulResponse.Details = new { info = "No se puede duplicar el nombre de marca" };

                    return Conflict(unSuccessfulResponse);

                default:
                    unSuccessfulResponse.Code = "500";
                    unSuccessfulResponse.Message = "Ocurrió un error inesperado";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno inesperado" };


                    // aqui hice un cambio en badRequest 
                    return StatusCode(500, unSuccessfulResponse);



            }
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var serviceResponse = await _SuppliersService.GetAllAsync();

            if (serviceResponse.IsSuccess)
            {
                //mapeo de los datos recibidos a la estructura del DTO a enviar
                //En este caso mapear la estructura supplier a supplierDTO usando LINQ
                var SuppliersDtoCollection = serviceResponse.Data!.Select(c => new GetAllSupplierDto
                {
                    SupplierId = c.SupplierId,
                    SupplierName = c.SupplierName,
                    RNC = c.RNC,
                    Mail = c.Mail,
                    SupplierPhone = c.SupplierPhone,
                    SupplierAddress = c.SupplierAddress,
                    IsActive = c.IsActive,
                });

                //preparamos la respuesta ApiResponse
                var apiResponse = new ApiResponse<IEnumerable<GetAllSupplierDto>>
                {
                    Data = SuppliersDtoCollection,
                    Meta = new
                    {
                        TotalAmount = SuppliersDtoCollection.Count(),
                        message = serviceResponse.Message

                    }
                };
                return Ok(apiResponse);
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto();

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NoData:
                    unsuccessfulResponse.Code = "200";
                    unsuccessfulResponse.Message = "No se encontraron registros";
                    unsuccessfulResponse.Details = new { info = "Temporalmente no hay registros en la BD" };

                    return Ok(unsuccessfulResponse);

                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrió un error inesperado";
                    unsuccessfulResponse.Details = new { info = "Error interno en la aplicación" };

                    return StatusCode(500, unsuccessfulResponse);
            }

        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0 || id == null)
            {
                var response = new UnsuccessfulResponseDto()
                {
                    Code = "400",
                    Message = "Id proporcionado debe de ser mayor a 0",
                    Details = new { info = "Error en el formato de valor enviado" }
                };
                return BadRequest(response);
            }
            var serviceResponse = await _SuppliersService.GetByIdAsync(id);

            if (serviceResponse.IsSuccess)
            {
                var supplierDto = new SupplierDto
                {

                    SupplierId = serviceResponse.Data!.SupplierId,
                    SupplierName = serviceResponse.Data!.SupplierName,
                    RNC = serviceResponse!.Data.RNC,
                    Mail = serviceResponse!.Data.Mail,
                    SupplierPhone = serviceResponse!.Data.SupplierPhone,
                    SupplierAddress = serviceResponse!.Data.SupplierAddress,
                    IsActive = serviceResponse.Data!.IsActive,
                    
                    
                };

                return Ok(supplierDto);
            }
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    var unsuccessfulResponse = new UnsuccessfulResponseDto()
                    {
                        Code = "404",
                        Message = "No se encontró un proveedor asociado al Id proporcionado",
                        Details = new { info = serviceResponse.Message ?? "No se encontró el recurso solicitado" }
                    };

                    return NotFound(unsuccessfulResponse);

                default:
                    unsuccessfulResponse = new UnsuccessfulResponseDto()
                    {
                        Code = "500",
                        Message = "Ocurrió un error",
                        Details = new { info = serviceResponse.Message ?? "Error interno no esperado" }
                    };

                    return StatusCode(500, unsuccessfulResponse);

            }
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSupplierDto datasupplier)
        {
            // Nota: las validaciones de formato al campo supplierName, Description e IsActive
            // del UpdatesupplierDto las realiza el framework considerando las DataAnnotations.

            // Se invoca al servicio confiando en que el framework ya validó el DTO
            var serviceResponse = await _SuppliersService.UpdateAsync(id, datasupplier);

            if (serviceResponse.IsSuccess)
            {
                // Mapeo de la categoría recibida a formato CategoryDto
                var updatedsupplier = new SupplierDto
                {
                    SupplierId = serviceResponse.Data!.SupplierId,
                    SupplierName = serviceResponse.Data!.SupplierName,
                    RNC = serviceResponse!.Data.RNC,
                    Mail = serviceResponse!.Data.Mail,
                    SupplierPhone = serviceResponse!.Data.SupplierPhone,
                    SupplierAddress = serviceResponse!.Data.SupplierAddress,
                    IsActive = serviceResponse!.Data.IsActive,
                };

                // En este punto se enviará una respuesta exitosa de la solicitud (registro actualizado)
                return Ok(updatedsupplier);
            }

            var unSuccessfulResponse = new UnsuccessfulResponseDto();

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.ErrorValidation:
                    unSuccessfulResponse.Code = "404";
                    unSuccessfulResponse.Message = "No se encontró un proveedor con el Id proporcionado";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Recurso no encontrado" };
                    return NotFound(unSuccessfulResponse);

                case MessageCodes.Conflict:
                    unSuccessfulResponse.Code = "409";
                    unSuccessfulResponse.Message = "El registro no pudo guardarse por un conflicto";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Hubo conflicto en la actualización" };
                    return Conflict(unSuccessfulResponse);

                default:
                    unSuccessfulResponse.Code = "500";
                    unSuccessfulResponse.Message = "Ocurrió un error inesperado";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno inesperado" };
                    return StatusCode(500, unSuccessfulResponse);
            }
        }



        [HttpGet("byname/{name}")]
        public async Task<IActionResult> GetByName(string name)
        {
            var unSuccessfulResponse = new UnsuccessfulResponseDto();
            if (name.IsNullOrEmpty())
            {
                unSuccessfulResponse.Code = "400";
                unSuccessfulResponse.Message = "El dato proporcionado no es válido";
                unSuccessfulResponse.Details = new { Error = "El nombre no puede ser nulo o vacío" };

                return BadRequest(unSuccessfulResponse);

            }

            var ServiceResponse = await _SuppliersService.GetByNameAsync(name);

            if (ServiceResponse.IsSuccess)
            {
                var supplierDto = new SupplierDto()
                {
                    SupplierId = ServiceResponse.Data!.SupplierId,
                    SupplierName = ServiceResponse.Data!.SupplierName,
                    RNC = ServiceResponse!.Data.RNC,
                    Mail = ServiceResponse!.Data.Mail,
                    SupplierPhone = ServiceResponse!.Data.SupplierPhone,
                    SupplierAddress = ServiceResponse!.Data.SupplierAddress,
                };

                return Ok(supplierDto);
            }

            switch (ServiceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unSuccessfulResponse.Code = "404";
                    unSuccessfulResponse.Message = ServiceResponse.Message ?? "proveedor no encontrado";
                    unSuccessfulResponse.Details = new { Error = "El recurso solicitado no está disponible en el servidor" };
                    return NotFound(unSuccessfulResponse);

                default:
                    unSuccessfulResponse.Code = "500";
                    unSuccessfulResponse.Message = ServiceResponse.Message ?? "Ocurrió un error inesperado";

                    return StatusCode(500, unSuccessfulResponse);
            }
        }




        [HttpPatch("{id}/state")]
        public async Task<IActionResult> SetStateAsync(int id, [FromQuery] bool state)
        {
            // Llama al servicio que maneja la lógica de activación/desactivación
            var serviceResponse = await _SuppliersService.SetStateAsync(id, state);

            if (serviceResponse.IsSuccess)
            {
                // Devuelve la marca actualizada con mensaje
                return Ok(new
                {
                    SupplierId = serviceResponse.Data!.SupplierId,
                    SupplierName = serviceResponse.Data.SupplierName,
                    IsActive = serviceResponse.Data.IsActive,
                    Message = serviceResponse.Message
                });
            }

            // Devuelve error si no se encontró la categoría o hubo problema
            var unSuccessfulResponse = new UnsuccessfulResponseDto();

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unSuccessfulResponse.Code = "404";
                    unSuccessfulResponse.Message = "No se encontró un proveedor con el Id proporcionado";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Recurso no encontrado" };
                    return NotFound(unSuccessfulResponse);


                default:
                    unSuccessfulResponse.Code = "500";
                    unSuccessfulResponse.Message = "Ocurrió un error inesperado";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno inesperado" };
                    return StatusCode(500, unSuccessfulResponse);
            }
        }
    }
}
