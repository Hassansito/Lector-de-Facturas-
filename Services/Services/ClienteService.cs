using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Models.DTO;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services.Services
{
    public class ClienteService : ICLienteService
    {
        private readonly IClientePDF _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<ClienteService> _logger;

        public ClienteService(IClientePDF repository, IMapper mapper, ILogger<ClienteService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<List<ResponseDTO>> GetAllAsync()
        {
            var clientes = await _repository.GetAsync();

            
            if (clientes == null)
                return new List<ResponseDTO>();

            
            return _mapper.Map<List<ResponseDTO>>(clientes);
        }

        public async Task<ResponseDTO> UpdateAsync(Guid id, UpdateClienteDTO dto)
        {
            
            var cliente = await _repository.GetByIdAsync(id);
            if (cliente == null)
                throw new KeyNotFoundException($"No se encontró el cliente con ID {id}");

           
            _mapper.Map(dto, cliente); 
           
            _repository.Update(cliente);
            var guardado = await _repository.SaveChangesAsync();

            if (!guardado)
                throw new DbUpdateException("No se pudo guardar los cambios del cliente.");

            return _mapper.Map<ResponseDTO>(cliente);
        }
        public async Task DeleteAsync(Guid id)
        {            
            var cliente = await _repository.GetByIdAsync(id);
            if (cliente == null)
                throw new KeyNotFoundException($"No se encontró el cliente con ID {id}");

            var filePath = cliente.RutaArchivoFactura;

            _repository.Delete(cliente);
            bool guardado = await _repository.SaveChangesAsync();
            if (!guardado)
                throw new DbUpdateConcurrencyException("El cliente fue eliminado por otro usuario.");

            if (!string.IsNullOrEmpty(filePath))
            {
                try
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                        _logger.LogInformation("Archivo de factura eliminado: {FilePath}", filePath);
                    }
                    else
                    {
                        _logger.LogWarning("No se encontró el archivo para eliminar: {FilePath}", filePath);
                    }
                }
                catch (Exception ex)
                {
                    // Sólo logueamos el error, no lanzamos excepción para no afectar la eliminación del cliente
                    _logger.LogError(ex, "Error al eliminar archivo físico {FilePath}", filePath);
                }
            }
        }

    }
}
