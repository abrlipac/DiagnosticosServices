using System;

namespace Diagnosticos.RequestHandlers.Exceptions
{
    public class DiagnosticosDiagnosticoCreateCommandException : Exception
    {
        public DiagnosticosDiagnosticoCreateCommandException(string message) : base(message)
        {

        }
    }
}
