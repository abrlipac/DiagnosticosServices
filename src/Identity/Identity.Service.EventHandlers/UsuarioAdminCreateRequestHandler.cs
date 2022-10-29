using Common.Result;
using Identity.Domain;
using Identity.Persistence.Database;
using Identity.RequestHandlers.Requests;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.RequestHandlers
{
    public class UsuarioAdminCreateRequestHandler : IRequestHandler<UsuarioAdminCreateRequest, Result<string>>
    {
        private readonly UserManager<Usuario> UserManager;
        private readonly ApplicationDbContext Context;

        private readonly int AdminRoleId = 1;

        public UsuarioAdminCreateRequestHandler(
            UserManager<Usuario> userManager,
            ApplicationDbContext applicationDbContext)
        {
            UserManager = userManager;
            Context = applicationDbContext;
        }

        public async Task<Result<string>> Handle(UsuarioAdminCreateRequest request, CancellationToken cancellationToken)
        {
            var entry = request.Adapt<Usuario>();

            var identityResult = await UserManager.CreateAsync(entry, request.Password); // creación del usuario y contraseña

            if (!identityResult.Succeeded) // si no se pudo registrar al usuario
                return GetResult(identityResult);

            var role = Context.Roles.Find(AdminRoleId);
            var user = Context.Users.ToList().Where(x => x.UserName.Equals(entry.UserName)).SingleOrDefault(); // busca al usuario creado

            RolUsuario rolUsuario = new()
            {
                Rol = role,
                Usuario = user
            }; // asignación del rol admin

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
