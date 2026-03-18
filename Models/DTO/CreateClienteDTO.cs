using System.ComponentModel.DataAnnotations;

namespace Models.DTO
{
    public class CreateClienteDTO
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
        public string Name { get; set; }

        [Required(ErrorMessage = "El ciclo es obligatorio")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El ciclo debe tener entre 2 y 100 caracteres")]
        public string Ciclo { get; set; }

        [Required(ErrorMessage = "La entidad es obligatoria")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "La entidad debe tener entre 2 y 100 caracteres")]
        public string Entidad { get; set; }
    }
}
