using Common.Result;
using Identity.RequestHandlers.Requests;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Identity.Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("identity")]
    public class IdentityController : ControllerBase
    {
        private readonly IMediator Mediator;

        public IdentityController(IMediator mediator)
        {
            Mediator = mediator;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("admin")]
        public async Task<IActionResult> SignUpAdmin(UsuarioAdminCreateRequest request)
        {
            if (ModelState.IsValid)
            {
                var result = await Mediator.Send(request);

                if (!result.Succeeded)
                    return BadRequest(result);

                result.Content = $"Los datos del usuario '{request.UserName}' fueron registrados";
                return Ok(result);
            }

            List<string> errors = GetValidationErrors();
            return BadRequest(new Result<string>() { Errors = errors });
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> SignUp(UsuarioCreateRequest request)
        {
            if (ModelState.IsValid)
            {
                var result = await Mediator.Send(request);

                if (!result.Succeeded)
                    return BadRequest(result);

                result.Content = $"Los datos del usuario '{request.UserName}' fueron registrados";
                return Ok(result);
            }

            List<string> errors = GetValidationErrors();
            return BadRequest(new Result<string>() { Errors = errors });
        }

        [AllowAnonymous]
        [HttpPost("auth")]
        public async Task<IActionResult> SignIn(UsuarioLoginRequest request)
        {
            if (ModelState.IsValid)
            {
                var result = await Mediator.Send(request);

                if (!result.Succeeded)
                {
                    result.Errors.Add("Acceso denegado");
                    return BadRequest(result);
                }
                return Ok(result);
            }
            List<string> errors = GetValidationErrors();
            return BadRequest(new Result<string>() { Errors = errors });
        }

        private List<string> GetValidationErrors()
        {
            return ModelState.Values
                .SelectMany(v => v.Errors
                .Select(b => b.ErrorMessage))
                .ToList();
        }
    }
}
