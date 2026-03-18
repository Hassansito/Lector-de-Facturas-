using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Entities
{
    public class ClienteEntity
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public long NumeroCliente {  get; set; }
        public required string Ciclo { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Monto { get; set; }
        public required  string Entidad { get; set; }
        public required string Provincia {  get; set; }
    }
}
