using Common.Result;
using Identity.Persistence.Database;
using Identity.Queries.DTOs;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Identity.Queries
{
    public interface IUsuarioQueryService
    {
        Task<DataCollection<UsuarioDto>> GetAllAsync(int page, int take, IEnumerable<string> users = null);
        Task<Result<UsuarioDto>> GetAsync(string userName);
    }
    public class UsuarioQueryService : IUsuarioQueryService
    {
        private readonly ApplicationDbContext Context;

        public UsuarioQueryService(
            ApplicationDbContext context)
        {
            Context = context;
        }

        public async Task<DataCollection<UsuarioDto>> GetAllAsync(int page, int take, IEnumerable<string> users = null)
        {
            var collection = await Context.Users
                .Where(x => users == null || users.Contains(x.Id.ToString()))
                .OrderBy(x => x.NombreCompleto)
                .GetPagedAsync(page, take);

            return collection.Adapt<DataCollection<UsuarioDto>>();
        }

        public async Task<Result<UsuarioDto>> GetAsync(string userName)
        {
            var user = await Context.Users.SingleOrDefaultAsync(x => x.UserName == userName);
            if (user == null)
            {
                var error = new List<string>
                {
                    $"No se encontró al usuario '{userName}'"
                };
                return new Result<UsuarioDto>
                {
                    Succeeded = false,
                    Errors = error,
                };
            }

            return new Result<UsuarioDto>
            {
                Succeeded = true,
                Content = user.Adapt<UsuarioDto>()
            };
        }
    }
}
