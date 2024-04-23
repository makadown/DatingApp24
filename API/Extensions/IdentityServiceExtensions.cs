using System.Threading.Tasks;
using System.Text;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace API.Extensions
{
    /// <summary>
    /// Clase auxiliar relacionada con identity
    /// </summary>
    public static class IdentityServiceExtensions
    {
        /// <summary>
        /// Método de extensión para la interface IServiceCollection.
        /// Para agregar configuracion de autenticacion.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static IServiceCollection AddIdentityService(
            this IServiceCollection services,
            IConfiguration config
        )
        {
            // en aplicaciones mvc / razor pages, se usaria el default AddIdentity
            // pero en este caso se usa el core
            services.AddIdentityCore<AppUser>(opt => {
                opt.Password.RequireNonAlphanumeric = false;
                opt.Password.RequireDigit = false;
                opt.Password.RequireLowercase = false;
                opt.Password.RequireUppercase = false;
            })
            .AddRoles<AppRole>()
            .AddRoleManager<RoleManager<AppRole>>()
            .AddSignInManager<SignInManager<AppUser>>()
            .AddRoleValidator<RoleValidator<AppRole>>()
            .AddEntityFrameworkStores<DataContext>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new
                            SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"])),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                    };

                    // Autenticando a SignalR
                    options.Events = new JwtBearerEvents {
                        OnMessageReceived = context => {
                            // debe ser este específico string!!!!
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) &&
                                path.StartsWithSegments("/hubs")) {
                                    context.Token = accessToken;
                                }
                            return Task.CompletedTask;
                        }
                    };
                });
            
            // Policy's que se agregarán a controlador de Admin
            services.AddAuthorization( opt => {
                opt.AddPolicy("RequireAdminRole", 
                        policy => policy.RequireRole("Admin") );
                opt.AddPolicy("ModeratePhotoRole", 
                        policy => policy.RequireRole("Admin", "Moderator") );
            });
            
            return services;
        }
    }
}