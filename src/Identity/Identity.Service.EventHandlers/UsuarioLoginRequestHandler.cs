using Common.Result;
using Identity.Domain;
using Identity.Persistence.Database;
using Identity.RequestHandlers.Requests;
using Identity.RequestHandlers.Responses;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.RequestHandlers
{
    public class UsuarioLoginRequestHandler :
        IRequestHandler<UsuarioLoginRequest, Result<IdentityAccess>>
    {
        private readonly SignInManager<Usuario> SignInManager;
        private readonly ApplicationDbContext Context;
        private readonly IConfiguration Configuration;

        public UsuarioLoginRequestHandler(
            SignInManager<Usuario> signInManager,
            ApplicationDbContext context,
            IConfiguration configuration)
        {
            SignInManager = signInManager;
            Context = context;
            Configuration = configuration;
        }

        public async Task<Result<IdentityAccess>> Handle(UsuarioLoginRequest request, CancellationToken cancellationToken)
        {
            var result = new Result<IdentityAccess>() { Content = new IdentityAccess() };

            var user = await Context.Users.SingleAsync(x => x.UserName == request.UserName, cancellationToken: cancellationToken);
            var response = await SignInManager.CheckPasswordSignInAsync(user, request.Password, false);

            result.Succeeded = response.Succeeded;

            result.Content.AccessToken = response.Succeeded ?
                await GenerateToken(user)
                : "";

            return result;
        }

        private async Task<string> GenerateToken(Usuario user)
        {
            var secretKey = Configuration.GetSection("SecretKey").Value; // obtiene la clave secreta de la aplicación
            var key = Encoding.ASCII.GetBytes(secretKey); // decodifica la clave secreta

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.NombreCompleto)
            }; // instancia los claims

            var roles = await Context.Roles
                .Where(x => x.RolesUsuario.Any(y => y.UserId == user.Id))
                .ToListAsync(); // obtiene los roles del usuario

            // agrega los roles a los claims
            foreach (var role in roles)
                claims.Add(
                    new Claim(ClaimTypes.Role, role.Name)
                );

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            }; // instanciación del token (expira en 1 hora)

            var tokenHandler = new JwtSecurityTokenHandler();
            var createdToken = tokenHandler.CreateToken(tokenDescriptor); // creación del token

            return tokenHandler.WriteToken(createdToken); // retorna el token
        }
    }
}
