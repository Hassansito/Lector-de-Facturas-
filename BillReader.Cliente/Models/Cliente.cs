using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BillReader.Cliente.Models
{
    public class ClienteModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Name { get; set; } = string.Empty; // Valor por defecto para evitar null

        public long NumeroCliente { get; set; }

        [Required(ErrorMessage = "El ciclo es obligatorio")]
        public string Ciclo { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Monto { get; set; }

        [Required(ErrorMessage = "La entidad es obligatoria")]
        public string Entidad { get; set; } = string.Empty;

        [Required(ErrorMessage = "La provincia es obligatoria")]
        public string Provincia { get; set; } = string.Empty;
    }
}