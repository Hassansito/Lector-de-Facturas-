using Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface ICLienteService
    {
        Task<List<ResponseDTO>> GetAllAsync();
        Task<ResponseDTO> UpdateAsync(Guid id, UpdateClienteDTO dto);
        Task DeleteAsync(Guid id);
    }
}
