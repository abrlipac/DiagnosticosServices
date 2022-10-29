using HealthChecks.UI.Client;
using Identity.Domain;
using Identity.Persistence.Database;
using Identity.Queries;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Reflection;
using System.Text;

namespace Identity.Api
{
    public class Startup
    {

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        private readonly string MyAllowSpecificOrigins = "MyAllowSpecificOrigins";

        // Este método es llamado por el tiempo de ejecución. Utilice este método para agregar servicios al contenedor.
        public void ConfigureServices(IServiceCollection services)
        {
            // conexión con la tabla 'Identity' de la base de datos SQL Server
            services.AddDbContext<ApplicationDbContext>(
                options => options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection"),
                    x => x.MigrationsHistoryTable("__EFMigrationsHistory", "Identity")
                )
            );

            // agrega monitoreo de la aplicación y de la base de datos
            services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy())
                .AddDbContextCheck<ApplicationDbContext>(typeof(ApplicationDbContext).Name);

            // agrega la funcionalidad de Identity a las clases
            services.AddIdentity<Usuario, Rol>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // configuración de las opciones de las contraseñas
            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;
            });

            services.AddTransient<IUsuarioQueryService, UsuarioQueryService>();

            // agrega el Cors para hacer peticiones HTTP desde otros orígenes
            services.AddCors(options =>
            {
                options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin();
                      });
            });

            services.AddControllers();

            // evitar la invalidación del modelo
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            services.AddMediatR(Assembly.Load("Identity.RequestHandlers"));

            var secretKey = Encoding.ASCII.GetBytes(
                Configuration.GetValue<string>("SecretKey")
            );

            // habilita la autorización por Jwt-bearer
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(secretKey),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });
        }

        // Este método es llamado por el tiempo de ejecución. Utilice este método para configurar la canalización de solicitudes HTTP.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider provider)
        {
            if (env.IsProduction())
            {
                // Aplica las migraciones en la base de datos
                provider.GetService<ApplicationDbContext>()
                    .Database.Migrate();
            }

            app.UseRouting();

            // Habilita el Cors
            app.UseCors(MyAllowSpecificOrigins);

            app.UseAuthorization();
            app.UseAuthentication();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers()
                .RequireCors(MyAllowSpecificOrigins);
                endpoints.MapHealthChecks("/healthcheck", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                }).RequireCors(MyAllowSpecificOrigins);
            });
        }
    }
}
