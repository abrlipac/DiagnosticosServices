using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Personal.Service.EventHandlers.Commands
{
    public class AdminUpdateActivoCommand : INotification
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public bool Activo { get; set; }
    }
}
