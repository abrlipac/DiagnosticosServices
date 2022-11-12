using Clientes.Service.EventHandlers.Commands;
using Clientes.Service.Queries;
using Clientes.Service.Queries.DTOs;
using Common.Result;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Clientes.Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("pacientes")]
    public class PacienteController : ControllerBase
    {
        private readonly IPacienteQueryService _pacienteQueryService;
        private readonly IMediator _mediator;

        public PacienteController(
            IMediator mediator,
            IPacienteQueryService pacienteQueryService)
        {
            _mediator = mediator;
            _pacienteQueryService = pacienteQueryService;
        }

        [HttpGet]
        public async Task<DataCollection<PacienteDto>> GetAll(
            int page = 1,
            int take = 10,
            string ids = null,
            string dni = null,
            string usuarioId = null)
        {
            IEnumerable<int> pacientesIds = null;

            if (!string.IsNullOrEmpty(ids))
                pacientesIds = ids.Split(',').Select(x => Convert.ToInt32(x));

            return await _pacienteQueryService.GetAllAsync(dni, usuarioId, page, take, pacientesIds);
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var paciente = await _pacienteQueryService.GetAsync(id);
            if (paciente is null)
                return NotFound($"No se ha encontrado a un paciente con Id {id}");

            return Ok(paciente);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Create(PacienteCreateCommand command)
        {
            if (ModelState.IsValid)
            {
                var result = await _mediator.Send(command);

                if (!result.Succeeded)
                    return BadRequest(result.Errors);

                return CreatedAtAction(nameof(Get), new {id = result.Content.Id}, result.Content);
            }
            return BadRequest(ModelState.Values.SelectMany(v => v.Errors.SelectMany(v1 => v1.ErrorMessage)));
        }

        [HttpPatch]
        public async Task<IActionResult> UpdateContactInfo(PacienteUpdateContactInfoCommand notification)
        {
            if (ModelState.IsValid)
            {
                var result = await _mediator.Send(notification);
                if (!result.Succeeded)
                    return BadRequest(result.Errors);

                return Ok(result);
            }
            return BadRequest(ModelState.Values.SelectMany(v => v.Errors));
        }
    }
}
