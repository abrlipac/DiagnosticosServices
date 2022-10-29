using Common.Result;
using Identity.Queries;
using Identity.Queries.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Identity.Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("usuarios")]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioQueryService UsuarioQueryService;

        public UsuarioController(
            IUsuarioQueryService usuarioQueryService)
        {
            UsuarioQueryService = usuarioQueryService;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<DataCollection<UsuarioDto>> GetAll(
            int page = 1,
            int take = 10,
            string ids = null)
        {
            IEnumerable<string> users = ids?.Split(',');
            return await UsuarioQueryService.GetAllAsync(page, take, users);
        }

        [AllowAnonymous]
        [HttpGet("{username}")]
        public async Task<Result<UsuarioDto>> Get(string username)
        {
            return await UsuarioQueryService.GetAsync(username);
        }
    }
}
