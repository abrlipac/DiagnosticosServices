using System.ComponentModel.DataAnnotations;

namespace Diagnosticos.RequestHandlers.Commands
{
    public class DetalleDiagnosticoCommand
    {
        [Required]
        public int Pregunta_Id { get; set; }
        [Required]
        public string Respuesta { get; set; }
    }
}
