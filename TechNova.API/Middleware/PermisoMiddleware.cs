using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Linq;
using System.Threading.Tasks;

namespace TechNova.API.Middleware
{
    public class PermisoMiddleware
    {
        private readonly RequestDelegate _next;

        public PermisoMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();

            // Si no hay endpoint (ej: estáticos) seguir
            if (endpoint == null)
            {
                await _next(context);
                return;
            }

            // Leer el atributo [Permiso("X")]
            var permisoAtributo = endpoint.Metadata.GetMetadata<PermisoAttribute>();
            if (permisoAtributo == null)
            {
                await _next(context);
                return;
            }

            var permisoRequerido = permisoAtributo.Permiso;

            // Leer permisos del usuario obtenidos del token
            var permisosUsuario = context.Items["Permisos"] as string[]
                                  ?? new string[0];

            if (!permisosUsuario.Contains(permisoRequerido))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = $"No tienes permiso para acceder: {permisoRequerido}"
                });
                return;
            }

            await _next(context);
        }
    }
}
