using Diagnosticos.Queries.DTOs;
using Diagnosticos.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Diagnosticos.Queries.Exceptions;
using System;
using Common.Result;
using Diagnosticos.RequestHandlers.Commands;
using Diagnosticos.RequestHandlers.Exceptions;

namespace Diagnosticos.Api.Controllers
{
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("diagnosticos")]
    public class DiagnosticosController : ControllerBase
    {
        private readonly IDiagnosticoQueryService _diagnosticoQueryService;
        private readonly IMediator _mediator;

        public DiagnosticosController(
            IMediator mediator,
            IDiagnosticoQueryService diagnosticoQueryService)
        {
            _mediator = mediator;
            _diagnosticoQueryService = diagnosticoQueryService;
        }

        [HttpGet]
        public async Task<DataCollection<DiagnosticoDto>> GetAll(int? paciente, int page = 1, int take = 10) 
        {
            return await _diagnosticoQueryService.GetAllAsync(paciente, page, take);
        }

        [HttpGet("enfermedadesDiagnosticadas")]
        public async Task<DataCollection<EnfermedadDiagnosticoDto>> GetAllEnfermedadesDiagnosticadas(int? paciente, int page = 1, int take = 10)
        {
            return await _diagnosticoQueryService.GetAllEnfermedadesDiagnosticadasAsync(paciente, page, take);
        }

        [HttpGet("enfermedades")]
        public async Task<DataCollection<EnfermedadDto>> GetAllEnfermedades(int page = 1, int take = 10)
        {
            return await _diagnosticoQueryService.GetAllEnfermedadesAsync(page, take);
        }

        [HttpGet("especialidades")]
        public async Task<DataCollection<EspecialidadDto>> GetAllEspecialidades(int page = 1, int take = 10)
        {
            return await _diagnosticoQueryService.GetAllEspecialidadesAsync(page, take);
        }

        [HttpGet("preguntas")]
        public async Task<DataCollection<PreguntaDto>> GetAllPreguntas(int espId, int page = 1, int take = 10)
        {
            return await _diagnosticoQueryService.GetAllPreguntasAsync(espId, page, take);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DiagnosticoDto>> Get(int id)
        {
            try
            {
                return await _diagnosticoQueryService.GetAsync(id);
            }
            catch (DiagnosticosGetDiagnosticoException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("resultados/{id}")]
        public async Task<ActionResult<ResultadoDiagnosticoDto>> GetResultado(int id)
        {
            try
            {
                return await _diagnosticoQueryService.GetResultadoAsync(id);
            }
            catch (DiagnosticosGetDiagnosticoException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary> 
        /// La función recibe una notificación, la publica y devuelve una respuesta HTTP 
        /// </summary> 
        /// <param name="DiagnosticoCreateCommand">Este es el comando que se enviará al mediador.</param> 
        /// <returns> 
        /// El resultado del comando. 
        /// </returns> 
        [HttpPost]
        public async Task<IActionResult> Create(DiagnosticoCreateCommand notification)
        {
            try
            {
                var response = await _mediator.Send(notification); // Envía el comando de Diagnostico 
                return Ok(response);
            }
            catch (DiagnosticosDiagnosticoCreateCommandException ex)
            {
                // Para una excepción de Diagnostico
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                // Para cualquier otra excepción
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update(DiagnosticoUpdateCommand notification)
        {
            try
            {
                await _mediator.Publish(notification);
                return Ok();
            }
            catch (DiagnosticosDiagnosticoCreateCommandException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
