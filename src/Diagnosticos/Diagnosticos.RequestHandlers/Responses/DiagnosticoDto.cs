using System;
using System.Collections.Generic;

namespace Diagnosticos.RequestHandlers.Responses
{
    public class DiagnosticoDto
    {
        public int Id { get; set; }
        public int Especialidad_Id { get; set; }
        public DateTime Fecha { get; set; }
        public ICollection<PosibleEnfermedadDto> PosiblesEnfermedades { get; set; } = new List<PosibleEnfermedadDto>();
    }
}
