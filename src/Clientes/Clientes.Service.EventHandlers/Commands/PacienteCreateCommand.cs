using Clientes.Common;
using Clientes.Service.Queries.DTOs;
using Common.Result;
using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace Clientes.Service.EventHandlers.Commands
{
    public class PacienteCreateCommand : IRequest<Result<PacienteDto>>
    {
        [Required]
        [StringLength(8)]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "Ingrese un DNI válido")]
        public string Dni { get; set; }
        [Required]
        [RegularExpression(@"\d+", ErrorMessage = "El Id. de usuario no es válido")]
        public string Usuario_Id { get; set; }
        [Required]
        [RegularExpression(@"^[a-zA-Záéíóúñ ]{2,127}$", ErrorMessage = "Ingrese nombres válidos")]
        public string Nombres { get; set; }
        [Required]
        [RegularExpression(@"^[a-zA-Záéíóúñ ]{2,127}$", ErrorMessage = "Ingrese apellidos válidos")]
        public string Apellidos { get; set; }
        [Required]
        public Sexo Sexo { get; set; }
        [Required]
        public DateTime FechaNacimiento { get; set; }
        [Required]
        [RegularExpression(@"^[a-zA-Záéíóúñ ]{2,63}$", ErrorMessage = "La región no es válida")]
        public string Region { get; set; }
        [Required]
        [EmailAddress(ErrorMessage = "El email no es válido")]
        public string Email { get; set; }
        [Required]
        [StringLength(9)]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "Ingrese un número de celular válido")]
        public string Celular { get; set; }
        public bool Activo { get; set; }
    }
}
