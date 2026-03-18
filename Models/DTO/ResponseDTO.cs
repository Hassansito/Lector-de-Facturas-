using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DTO
{
    public class ResponseDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public long NumeroCliente { get; set; }
        public string Ciclo { get; set; }
        public DateTime CreatedDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Monto { get; set; }
        public string Entidad { get; set; }
        public string Provincia { get; set; }
    }
}
