using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Personal.Service.EventHandlers.Commands
{
    public class AdminCreateCommand : INotification
    {
        [Required]
        [RegularExpression(@"\d+", ErrorMessage = "El Id. de usuario no es válido")]
        public string Usuario_Id { get; set; }
        [Required]
        [StringLength(8)]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "Ingrese un DNI válido")]
        public string Dni { get; set; }
        [Required]
        [RegularExpression(@"^[a-zA-Záéíóúñ ]{2,127}$", ErrorMessage = "Ingrese nombres válidos")]
        public string Nombres { get; set; }
        [Required]
        [RegularExpression(@"^[a-zA-Záéíóúñ ]{2,127}$", ErrorMessage = "Ingrese apellidos válidos")]
        public string Apellidos { get; set; }
        public bool Activo { get; set; }
    }
}
