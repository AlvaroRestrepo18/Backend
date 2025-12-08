using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;

namespace TechNova.API.Utils
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class HasPermissionAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _permiso;

        public HasPermissionAttribute(string permiso)
        {
            _permiso = permiso;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            // Usuario NO autenticado
            if (user?.Identity == null || !user.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Permisos cargados por el middleware
            var permisos = context.HttpContext.Items["Permisos"] as string[];

            if (permisos == null || !permisos.Contains(_permiso))
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }
}
