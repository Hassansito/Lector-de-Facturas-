using Data;
using Microsoft.EntityFrameworkCore;
using Models.Entities;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.Repositories
{
    public class ClientesPDFRepository : IClientePDF
    {
        private readonly ApplicationDbContext _context;

        public ClientesPDFRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task Add(ClienteEntity cliente)
        {
            _context.Cliente.Add(cliente);

            await _context.SaveChangesAsync();
        }

        public void Delete(ClienteEntity cliente)
        {
            ArgumentNullException.ThrowIfNull(cliente);
            _context.Cliente.Remove(cliente);
        }

        public async Task<List<ClienteEntity>> GetAsync()
        {
            return await _context.Cliente.ToListAsync();
        }

        public async Task<ClienteEntity?> GetByIdAsync(Guid id)
        {
            return await _context.Cliente.FindAsync(id);
        }
        public void Update(ClienteEntity cliente)
        {
            ArgumentNullException.ThrowIfNull(cliente);
            _context.Cliente.Update(cliente);
        }
        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }      
    }
}