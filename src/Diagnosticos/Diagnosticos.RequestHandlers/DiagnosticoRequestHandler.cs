using Diagnosticos.Domain;
using Diagnosticos.Persistence.Database;
using Diagnosticos.RequestHandlers.Commands;
using Diagnosticos.RequestHandlers.Exceptions;
using Diagnosticos.RequestHandlers.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Prolog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Diagnosticos.RequestHandlers
{
    public class DiagnosticoRequestHandler :
        IRequestHandler<DiagnosticoCreateCommand, DiagnosticoDto>,
        INotificationHandler<DiagnosticoUpdateCommand>
    {
        private readonly ApplicationDbContext Context;
        private readonly ILogger<DiagnosticoRequestHandler> Logger;

        public static readonly bool IsRunningFromUnitTest =
            AppDomain.CurrentDomain.GetAssemblies().Any(
                a => a.FullName.ToLowerInvariant().Contains("unit") ||
                    a.FullName.ToLowerInvariant().Contains("test"));

        public DiagnosticoRequestHandler(
            ApplicationDbContext context,
            ILogger<DiagnosticoRequestHandler> logger)
        {
            Context = context;
            Logger = logger;
        }

        /// <summary> 
        /// > La función crea un nuevo objeto Diagnostico, establece sus propiedades y lo guarda en la base de  
        /// datos 
        /// </summary> 
        /// <param name="DiagnosticoCreateCommand">Este es el comando que se está manejando.</param> 
        /// <param name="CancellationToken">Este es un token que se puede utilizar para cancelarla 
        /// operación.</param> 
    public async Task<DiagnosticoDto> Handle(DiagnosticoCreateCommand request, CancellationToken cancellationToken)
    {
      Logger.LogInformation("! Empezó la creación de un nuevo diagnóstico");
            var entry = new Diagnostico();

            // Empieza una transacción
            using (var transaction = await Context.Database.BeginTransactionAsync(cancellationToken))
            {
                PrepararDiagnostico(request, entry);

                Logger.LogInformation("! Guardando el diagnóstico");

                await Context.AddAsync(entry, cancellationToken);
                await Context.SaveChangesAsync(cancellationToken);

                Logger.LogInformation($"! El diagnóstico ha sido creado");

                await transaction.CommitAsync(cancellationToken);
            }
            Logger.LogInformation("! Terminó la creación de un nuevo diagnóstico");

            return new DiagnosticoDto {
                Id = entry.Id,
                Especialidad_Id = entry.Especialidad_Id,
                Fecha = entry.Fecha,
                PosiblesEnfermedades = entry.PosiblesEnfermedades.Select(pe => new PosibleEnfermedadDto {
                    Enfermedad_Id = pe.Enfermedad_Id,
                    Porcentaje = pe.Porcentaje
                }).ToList()
            };
    }

        public async Task Handle(DiagnosticoUpdateCommand notification, CancellationToken cancellationToken)
        {
            Logger.LogInformation("! Empezó la actualización de un diagnóstico");
            var entry = await Context.Diagnosticos
                .Where(x => x.Id == notification.Id)
                .Include(x => x.DetallesDiagnostico)
                .Include(x => x.PosiblesEnfermedades)
                .FirstOrDefaultAsync(cancellationToken);

            using (var transaction = await Context.Database.BeginTransactionAsync(cancellationToken))
            {
                PrepararDiagnostico(notification, entry);

                Logger.LogInformation("! Guardando el diagnóstico");

                Context.Entry(entry).State = EntityState.Modified;
                await Context.SaveChangesAsync(cancellationToken);

                Logger.LogInformation($"! El diagnóstico ha sido actualizado");

                await transaction.CommitAsync(cancellationToken);
            }

            Logger.LogInformation("! Terminó la actualización de un diagnóstico");
        }

        /// <summary> 
        /// > Esta función prepara el encabezado, detalles y resultados de un diagnóstico 
        /// </summary> 
        /// <param name="IDiagnosticoCommand">Esta es la interfaz que implementa el comando.</param> 
        /// <param name="Diagnostico">Esta es la entidad que estoy tratando de salvar.</param> 
        private void PrepararDiagnostico(IDiagnosticoCommand notification, Diagnostico entry)
        {
            Logger.LogInformation("! Preparando los detalles");
            PrepararDetalles(entry, notification);

            Logger.LogInformation("! Preparando los resultados");
            PrepararEnfermedades(entry, notification);

            Logger.LogInformation("! Preparando la cabecera");
            PrepararHeader(entry, notification);
        }

        /// <summary> 
        /// Toma un objeto Diagnostico y un objeto de notificación, y establece la propiedad DetallesDiagnostico 
        /// del objeto Diagnostico en una lista de objetos DetalleDiagnostico, cada uno de los cuales tiene el 
        /// mismo Diagnostico_Id que el objeto Diagnostico, y cada uno de los cuales tiene una Pregunta_Id y una 
        /// Respuesta que son las mismas que las propiedades correspondientes del objeto de notificación 
        /// </summary> 
        /// <param name="Diagnostico">La entidad que se guardará en la base de datos.</param> 
        /// <param name="IDiagnosticoCommand">Esta es la interfaz que implementa el comando.</param> 
        public static void PrepararDetalles(Diagnostico entry, IDiagnosticoCommand notification)
        {
            entry.DetallesDiagnostico = notification.DetallesDiagnostico.Select(x => new DetalleDiagnostico
            {
                Diagnostico_Id = entry.Id,
                Pregunta_Id = x.Pregunta_Id,
                Respuesta = x.Respuesta
            }).ToList();
        }

        /// <summary> 
        /// > Toma una lista de enfermedades y una lista de síntomas, y devuelve una lista de enfermedades con 
        /// sus respectivos porcentajes 
        /// </summary> 
        /// <param name="Diagnostico">La entidad que se está creando.</param> 
        /// <param name="IDiagnosticoCommand">Esta es la interfaz que implementa el comando.</param> 
        public void PrepararEnfermedades(Diagnostico entry, IDiagnosticoCommand notification)
        {
            var enfermedadesDiagnostico = DeterminarEnfermedades(notification);
            entry.PosiblesEnfermedades = enfermedadesDiagnostico.Select(x => new PosibleEnfermedad
            {
                Diagnostico_Id = entry.Id,
                Enfermedad_Id = x.Enfermedad_Id,
                Porcentaje = (decimal)x.Porcentaje
            }).ToList();
        }

        /// <summary> 
        /// > Esta función se utiliza para preparar la información de cabecera de la entidad Diagnostico 
        /// </summary> 
        /// <param name="Diagnostico">La entidad que se creará o actualizará.</param> 
        /// <param name="IDiagnosticoCommand">Esta es la interfaz que implementan todos los comandos.</param> 
        public static void PrepararHeader(Diagnostico entry, IDiagnosticoCommand notification)
        {
            // Header information
            if (notification is DiagnosticoCreateCommand createCommand)
            {
                // Si el comando es de creación de diagnóstico 
                entry.Fecha = DateTime.Now;
                entry.Paciente_Id = createCommand.Paciente_Id;
                entry.Especialidad_Id = createCommand.Especialidad_Id;
            }
            else if (notification is DiagnosticoUpdateCommand updateCommand)
            {
                // Si el comando es de actualización/edición de diagnóstico
                entry.Fecha = DateTime.Now;
                entry.Id = updateCommand.Id;
            }
        }

        /// <summary> 
        /// Toma una lista de síntomas y devuelve una lista de enfermedades que coinciden con los síntomas 
        /// </summary> 
        /// <param name="IDiagnosticoCommand">Este es el comando que se envía al servicio.</param> 
        /// <returns> 
        /// Una lista de objetos de EnfermedadDiagnóstico. 
        /// </returns> 
        public List<EnfermedadDiagnostico> DeterminarEnfermedades(IDiagnosticoCommand notification)
        {
            var prolog = new PrologEngine(persistentCommandHistory: false);
            var filename = "enfermedad.pl";

            var productionPrologPath = Path.GetFullPath($"./pl/{filename}");
            var localDevPrologPath = Path.GetFullPath($"./../Diagnosticos.RequestHandlers/{filename}");
            var localTestingPrologPath = Path.GetFullPath($"./../../../../Diagnosticos.RequestHandlers/{filename}");

            var isRunningFromProduction = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "production";

            var absolutePrologPath = isRunningFromProduction ? productionPrologPath
                : !IsRunningFromUnitTest ? localDevPrologPath
                : localTestingPrologPath;

            var enfermedades = Context.Enfermedades.Select(x => new EnfermedadDiagnostico
            {
                Enfermedad_Id = x.Id,
                Nombre = x.NombreClave,
                Cantidad = 0,
                Porcentaje = 0,
                CantSintomas = x.CantidadSintomas
            }).ToList();

            if (notification.DetallesDiagnostico == null || notification.DetallesDiagnostico.Count <= 0)
                throw new DiagnosticosDiagnosticoCreateCommandException($"No hay detalles de diagnostico en el diagnostico.");

            var detallesSintoma = notification.DetallesDiagnostico.ToList()
                .Where(x => x.Respuesta.Equals("Sí")).Select(x => x.Pregunta_Id).ToList();
            var preguntas = Context.Preguntas.ToList().Where(x => detallesSintoma.Contains(x.Id));

            foreach (var pregunta in preguntas)
            {
                var solutions = prolog
                    .GetAllSolutions(absolutePrologPath, $"enfermedadde(Z, {pregunta.PalabraClave})")
                    .NextSolution;

                foreach (var solution in solutions)
                {
                    var coincidencia = solution.NextVariable.First().Value;

                    if (enfermedades.Select(x => x.Nombre).Contains(coincidencia))
                        enfermedades.Single(x => x.Nombre == coincidencia).Cantidad++;
                }
            }

            foreach (var enfermedad in enfermedades)
                enfermedad.Porcentaje = enfermedad.Cantidad * 100 / enfermedad.CantSintomas;

            var maxPorcentajes = enfermedades.OrderByDescending(x => x.Porcentaje).Take(4).ToList();

            if (maxPorcentajes[0].Porcentaje <= 0)
                throw new DiagnosticosDiagnosticoCreateCommandException($"No hay detalles de diagnostico con sintomas predefinidos en el diagnostico");

            return maxPorcentajes;
        }

    // Clase de apoyo para guardar los resultados individuales temporalmente
    public class EnfermedadDiagnostico
        {
            public int Enfermedad_Id;
            public string Nombre;
            public int Cantidad;
            public double Porcentaje;
            public double CantSintomas;
        }
    }
}
