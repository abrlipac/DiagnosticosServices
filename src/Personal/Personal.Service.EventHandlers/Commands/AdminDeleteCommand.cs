using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Personal.Service.EventHandlers.Commands
{
    public class AdminDeleteCommand : INotification
    {
        [Required]
        public int Id { get; set; }
    }
}
