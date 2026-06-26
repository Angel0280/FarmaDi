using FarmaDiBusiness.Interfaces;
using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using FarmaDiDataAccess.Interfaces;

namespace FarmaDiBusiness.Services
{
    public class ProductBatchesService : IProductBatchesService
    {
        private readonly IProductBatchesRepository _batchRepository;

        public ProductBatchesService(IProductBatchesRepository batchRepository)
        {
            _batchRepository = batchRepository;
        }

        public async Task<ServiceResponse<IEnumerable<ProductBatches>>> GetAllAsync()
        {
            try
            {
                // CORREGIDO: Envoltorio defensivo try-catch añadido para evitar caídas del servidor
                var result = await _batchRepository.GetAllAsync();

                if (result.OperationStatusCode == 0)
                {
                    return new ServiceResponse<IEnumerable<ProductBatches>>
                    {
                        Data = result.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = "Operación exitosa"
                    };
                }

                switch (result.OperationStatusCode)
                {
                    case 50009:
                        return new ServiceResponse<IEnumerable<ProductBatches>>
                        {
                            Data = result.Data,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NoData,
                            Message = "No se encontraron registros"
                        };

                    default:
                        return new ServiceResponse<IEnumerable<ProductBatches>>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.ErrorDataBase, // CORREGIDO: Mapeo semántico correcto (Antes NoData)
                            Message = "Ocurrió un error inesperado"
                        };
                }
            }
            catch (Exception)
            {
                return new ServiceResponse<IEnumerable<ProductBatches>>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al recuperar los lotes de productos"
                };
            }
        }

        public async Task<ServiceResponse<ProductBatches>> GetByIdAsync(int id)
        {
            try
            {
                // CORREGIDO: Movido de forma segura al interior del bloque try
                var result = await _batchRepository.GetByIdAsync(id);

                if (result.OperationStatusCode == 0)
                {
                    return new ServiceResponse<ProductBatches>
                    {
                        Data = result.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = result.Message ?? "Operación exitosa"
                    };
                }

                switch (result.OperationStatusCode)
                {
                    case 50009:
                        return new ServiceResponse<ProductBatches>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = "El lote no existe"
                        };

                    // CORREGIDO: Se eliminó el case 50007 (Cantidad inferior) por ser un residuo de copy-paste ajeno a lotes

                    default:
                        return new ServiceResponse<ProductBatches>
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
                return new ServiceResponse<ProductBatches>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    // CORREGIDO: Mensaje estático seguro para evitar una NullReferenceException accidental
                    Message = "Ocurrió un error inesperado al consultar el lote de producto"
                };
            }
        }
    }
}