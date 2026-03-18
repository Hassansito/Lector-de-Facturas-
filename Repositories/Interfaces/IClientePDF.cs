using Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IClientePDF
    {
        Task Add(ClienteEntity cliente);
        Task<List<ClienteEntity>> GetAsync();
        void Update(ClienteEntity cliente); 
        void Delete(ClienteEntity cliente);
        Task<bool> SaveChangesAsync();
        Task<ClienteEntity?> GetByIdAsync(Guid id);
    }
}
