using Clientes.Domain;
using Clientes.Persistence.Database;
using Clientes.Service.EventHandlers.Commands;
using Clientes.Service.Queries.DTOs;
using Common.Result;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Clientes.Service.EventHandlers
{
    public class PacienteEventHandler :
        IRequestHandler<PacienteCreateCommand, Result<PacienteDto>>,
        IRequestHandler<PacienteUpdateContactInfoCommand, Result<PacienteDto>>
    {
        private readonly ApplicationDbContext _context;

        public PacienteEventHandler(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<PacienteDto>> Handle(PacienteCreateCommand command, CancellationToken cancellationToken)
        {
            var paciente = command.Adapt<Paciente>();

            try
            {
                await _context.AddAsync(paciente, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception e)
            {
                var exMessage = e.InnerException.Message;
                string errorMessage = "Ha ocurrido un error";

                if (exMessage.Contains("IX_Pacientes_Usuario_Id") && exMessage.Contains(paciente.Usuario_Id))
                    errorMessage = $"El usuario con Id. '{paciente.Usuario_Id}' ya se ha registrado previamente";

                if (exMessage.Contains("IX_Pacientes_Dni") && exMessage.Contains(paciente.Dni))
                    errorMessage = $"Ya se registró un paciente con el DNI '{paciente.Dni}'";

                var error = new List<string>
                {
                    errorMessage
                };

                return new Result<PacienteDto>
                {
                    Succeeded = false,
                    Errors = error
                };
            }
            return new Result<PacienteDto>
            {
                Succeeded = true,
                Content = paciente.Adapt<PacienteDto>()
            };
        }

        public async Task<Result<PacienteDto>> Handle(PacienteUpdateContactInfoCommand command, CancellationToken cancellationToken)
        {
            var originalPaciente =
                await _context.Pacientes
                    .AsNoTracking()
                    .SingleOrDefaultAsync(e => e.Id == command.Id,
                        cancellationToken: cancellationToken);

            if (originalPaciente == null)
            {
                var error = new List<string>
                {
                    $"No se encontró al paciente con Id {command.Id}"
                };

                return new Result<PacienteDto>
                {
                    Succeeded = false,
                    Errors = error
                };
            }

            var updatedPaciente = originalPaciente;

            updatedPaciente.Email = command.Email;
            updatedPaciente.Celular = command.Celular;

            try
            {
                _context.Update(updatedPaciente);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception e)
            {
                var error = new List<string>
                {
                    e.Message
                };

                return new Result<PacienteDto>
                {
                    Succeeded = false,
                    Errors = error
                };
            }
            return new Result<PacienteDto>
            {
                Succeeded = true,
                Content = updatedPaciente.Adapt<PacienteDto>()
            };
        }
    }
}
