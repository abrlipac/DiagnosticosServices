using Common.Result;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Identity.RequestHandlers.Requests
{
    public class UsuarioAdminCreateRequest : IRequest<Result<string>>
    {
        [Required(ErrorMessage = "El campo '{0}' no puede estar vacío")]
        [RegularExpression(@"^[a-zA-Záéíóúñ ]{2,255}$", ErrorMessage = "El campo '{0}' no es válido")]
        public string NombreCompleto { get; set; }
        [Required(ErrorMessage = "El campo '{0}' no puede estar vacío")]
        [RegularExpression(@"\w+", ErrorMessage = "El campo '{0}' solo puede contener letras y números")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "El campo '{0}' no puede estar vacío")]
        [MinLength(6, ErrorMessage = "El campo '{0}' debe tener un mínimo de 6 caracteres")]
        public string Password { get; set; }
    }
}
