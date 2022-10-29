using System.Collections.Generic;

namespace Diagnosticos.RequestHandlers.Commands
{
    public interface IDiagnosticoCommand
    {
        public ICollection<DetalleDiagnosticoCommand> DetallesDiagnostico { get; set; }
    }
}