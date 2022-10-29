using Identity.Domain;
using Identity.Persistence.Database;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Common.Result;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Identity.RequestHandlers.Requests;
using MediatR;

namespace Identity.RequestHandlers
{
    public class UsuarioCreateRequestHandler : IRequestHandler<UsuarioCreateRequest, Result<string>>
    {
        private readonly UserManager<Usuario> UserManager;
        private readonly ApplicationDbContext Context;
        private readonly int PacienteRoleId = 2;

        public UsuarioCreateRequestHandler(
            UserManager<Usuario> userManager, ApplicationDbContext applicationDbContext)
        {
            UserManager = userManager;
            Context = applicationDbContext;
        }

        public async Task<Result<string>> Handle(UsuarioCreateRequest request, CancellationToken cancellationToken)
        {
            var entry = request.Adapt<Usuario>();

            var identityResult = await UserManager.CreateAsync(entry, request.Password); // creación del usuario y contraseña

            if (!identityResult.Succeeded)
                return GetResult(identityResult);

            var role = Context.Roles.Find(PacienteRoleId);
            var user = Context.Users.ToList().Where(x => x.UserName.Equals(entry.UserName)).SingleOrDefault();

            RolUsuario rolUsuario = new()
            {
                Rol = role,
                Usuario = user
            }; // asignación del rol paciente

            await Context.AddAsync(rolUsuario, cancellationToken);
            await Context.SaveChangesAsync(cancellationToken);

            return GetResult(identityResult);
        }

        private static Result<string> GetResult(IdentityResult identityResult)
        => new()
        {
            Succeeded = identityResult.Succeeded,
            Errors = identityResult.Errors.Select(x => x.Description).ToList(),
        };
    }
}
