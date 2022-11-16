using Diagnosticos.Persistence.Database;
using Diagnosticos.Queries;
using HealthChecks.UI.Client;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
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

namespace Diagnosticos.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        private readonly string MyAllowSpecificOrigins = "MyAllowSpecificOrigins";

        // Este m�todo es llamado por el tiempo de ejecuci�n. Utilice este m�todo para agregar servicios al contenedor.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();

            // conexi�n con la tabla 'Diagnosticos' de la base de datos SQL Server
            services.AddDbContext<ApplicationDbContext>(
                options => options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection"),
                    x => x.MigrationsHistoryTable("__EFMigrationsHistory", "Diagnosticos")
                )
            );

            // agrega monitoreo de la aplicaci�n y de la base de datos
            services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy())
                .AddDbContextCheck<ApplicationDbContext>(typeof(ApplicationDbContext).Name);

            services.AddMediatR(Assembly.Load("Diagnosticos.RequestHandlers"));

            services.AddTransient<IDiagnosticoQueryService, DiagnosticoQueryService>();

            // agrega el Cors para hacer peticiones HTTP desde otros or�genes
            services.AddCors(options =>
            {
                options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin();
                      });
            });

            services.AddControllers();

            // evitar la invalidaci�n del modelo
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            var secretKey = Encoding.ASCII.GetBytes(
                Configuration.GetValue<string>("SecretKey")
            );

            // habilita la autorizaci�n por Jwt-bearer
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(x =>
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

            services.AddApplicationInsightsTelemetry();
        }

        // Este m�todo es llamado por el tiempo de ejecuci�n. Utilice este m�todo para configurar la canalizaci�n de solicitudes HTTP.
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
                endpoints.MapHealthChecks("/healthcheck", new HealthCheckOptions
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                }).RequireCors(MyAllowSpecificOrigins);
            });
        }
    }
}
