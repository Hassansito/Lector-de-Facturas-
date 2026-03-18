using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Entities
{
    public class Usuario
    {
        public Guid Id { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }
}
