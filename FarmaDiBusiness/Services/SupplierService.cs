using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.DTOs.SupplierDto;
using FarmaDiBusiness.Interfaces;
using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using FarmaDiDataAccess.Interfaces;
using FarmaDiDataAccess.Repositories;
using Microsoft.IdentityModel.Tokens;



namespace FarmaDiBusiness.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly ISuppliersRepository _supplierRepository;
        public SupplierService(ISuppliersRepository supplierRepository)
        {
            _supplierRepository = supplierRepository;
        }


        public async Task<ServiceResponse<Suppliers>> AddAsync(AddSupplierDto newsupplier)
        {

            try
            {
                //validar si existe registro (un proveedor) con nombre similar al que se desea crear
                var existing = await _supplierRepository.GetByNameAsync(newsupplier.SupplierName);

                if (existing.Data!.SupplierId != 0 && !existing.Data.SupplierName.IsNullOrEmpty())
                {
                    return new ServiceResponse<Suppliers>
                    {
                        Data = null,
                        IsSuccess = false, ///.//
                        MessageCode = MessageCodes.Conflict,
                        Message = "Existe un registro con el nombre proporcionado"

                    };

                }


                var supplier = new Suppliers()
                {
                    SupplierName = newsupplier.SupplierName,
                    RNC = newsupplier.RNC,
                    Mail = newsupplier.Mail,
                    SupplierPhone = newsupplier.SupplierPhone,
                    SupplierAddress = newsupplier.SupplierAddress,
                   

                };

                var result = await _supplierRepository.AddAsync(supplier);

                return new ServiceResponse<Suppliers>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "proveedor registrado correctamente"
                };



            }
            catch (Exception)
            {
                return new ServiceResponse<Suppliers>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado",
                };
            }
        }


        public async Task<ServiceResponse<IEnumerable<Suppliers>>> GetAllAsync()
        {
            var result = await _supplierRepository.GetAllAsync();

            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<IEnumerable<Suppliers>>()
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Operacion exitosa"
                };


            }
            switch (result.OperationStatusCode)
            {
                case 50009:
                    return new ServiceResponse<IEnumerable<Suppliers>>
                    {
                        Data = result.Data,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NoData,
                        Message = "No se encontraron registros"
                    };


                default:
                    return new ServiceResponse<IEnumerable<Suppliers>>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NoData,
                        Message = "Ocurrió un error inesperado"
                    };

            }

        }

        public async Task<ServiceResponse<Suppliers>> GetByIdAsync(int id)
        {
            var result = await _supplierRepository.GetByIdAsync(id);
            try
            {
                if (result.OperationStatusCode == 0)
                {
                    return new ServiceResponse<Suppliers>
                    {
                        Data = result.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = result.Message ?? "Operación exitosa"
                    };
                }
                switch (result.OperationStatusCode)
                {
                    case 50009: // Ejemplo: código para no encontrado
                        return new ServiceResponse<Suppliers>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = "el proveedor no existe"
                        };
                    default:
                        return new ServiceResponse<Suppliers>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.ErrorDataBase,
                            Message = result.Message ?? "Error inesperado"
                        };
                }
            }
            catch (Exception)
            {
                return new ServiceResponse<Suppliers>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = result.Message ?? "Ocurrió un error inesperado"

                };
            }
        }

        public async Task<ServiceResponse<Suppliers>> UpdateAsync(int id, UpdateSupplierDto Suppliers)
        {

            try
            {

                var existingId = await _supplierRepository.GetByIdAsync(id);
                if (existingId.Data!.SupplierId == 0 && existingId.Data.SupplierName.IsNullOrEmpty())
                {
                    return new ServiceResponse<Suppliers>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorValidation,
                        Message = "No existe un proveedor  asociado al Id proporcionado"

                    };


                }

                //validar que el nombre enviado para el proveedor no coincida con un  nombre existente
                var existingName = await _supplierRepository.GetByNameAsync(Suppliers.SupplierName);
                if (existingName.Data!.SupplierName != null && existingName.Data.SupplierId != id)
                {
                    return new ServiceResponse<Suppliers>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "ya existe un proveedor con el nombre proporcionado"
                    };
                }

                var datasupplier = new Suppliers()
                {
                    SupplierName = Suppliers.SupplierName,
                    RNC = Suppliers.RNC,
                    Mail = Suppliers.Mail,
                    SupplierPhone = Suppliers.SupplierPhone,
                    SupplierAddress = Suppliers.SupplierAddress,
                    IsActive = Suppliers.IsActive,

                };

                var result = await _supplierRepository.UpdateAsync(id, datasupplier);

                return new ServiceResponse<Suppliers>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Proveedor actualizado correctamente"
                };


            }
            catch (Exception)
            {
                return new ServiceResponse<Suppliers>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al actualizar el proveedor"
                };
            }
        }


        public async Task<ServiceResponse<Suppliers>> GetByNameAsync(string name)
        {
            var result = await _supplierRepository.GetByNameAsync(name);
            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<Suppliers>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Operación exitosa"
                };
            }

            var messageCode = new MessageCodes();
            var message = string.Empty;

            switch (result.OperationStatusCode)
            {
                case 50009:
                    messageCode = MessageCodes.NotFound;
                    message = "No se encontró un proveedor que corresponda al nombre proporcionado";
                    break;

                default:
                    messageCode = MessageCodes.ErrorDataBase;
                    message = "Error en la base de datos al obtener el proveedor.";
                    break;
            }

            // Retorno  para los casos de error o no encontrado
            return new ServiceResponse<Suppliers>
            {
                Data = null,
                IsSuccess = false,
                MessageCode = messageCode,
                Message = message
            };
        }




        public async Task<ServiceResponse<Suppliers>> SetStateAsync(int id, bool state)
        {
            var response = new ServiceResponse<Suppliers>();

            // Validar que el proveedor exista
            var existing = await _supplierRepository.GetByIdAsync(id);
            if (existing == null)
            {
                response.Data = null;
                response.IsSuccess = false;
                response.MessageCode = MessageCodes.ErrorValidation;
                response.Message = "el proveedor no existe";
                return response;
            }

            // Llamar al repositorio para actualizar el estado
            var repoResponse = await _supplierRepository.SetStateAsync(id, state);

            if (repoResponse.Data == null)
            {
                response.Data = null;
                response.IsSuccess = false;
                response.MessageCode = MessageCodes.NotFound;
                response.Message = "No se pudo encontrar un proveedor que coincida con el id proporcionado";
                return response;
            }

            // Construir la respuesta exitosa
            response.Data = repoResponse.Data;
            response.IsSuccess = true;
            response.MessageCode = MessageCodes.Success;
            response.Message = state ? "Proveedor activado" : "proveedor desactivado";

            return response;
        }


    }

}