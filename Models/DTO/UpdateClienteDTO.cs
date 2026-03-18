using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTO
{
    public class UpdateClienteDTO
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
        public string Name { get; set; }

        [Required(ErrorMessage = "La entidad es obligatoria")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "La entidad debe tener entre 2 y 100 caracteres")]
        public string Entidad { get; set; }

        [Required(ErrorMessage = "La provincia es obligatoria")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "La provincia debe tener entre 2 y 100 caracteres")]
        public string Provincia { get; set; }
    }
}
