using Clientes.Service.Queries.DTOs;
using Common.Result;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Clientes.Service.EventHandlers.Commands
{
    public class PacienteUpdateContactInfoCommand : IRequest<Result<PacienteDto>>
    {
        [Required]
        public int Id { get; set; }
        [EmailAddress(ErrorMessage = "El email no es válido")]
        public string Email { get; set; }
        [StringLength(9)]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "Ingrese un número de celular válido")]
        public string Celular { get; set; }
    }
}
