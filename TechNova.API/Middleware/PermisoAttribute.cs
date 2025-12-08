using System;

namespace TechNova.API.Middleware
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class PermisoAttribute : Attribute
    {
        public string Permiso { get; }

        public PermisoAttribute(string permiso)
        {
            Permiso = permiso;
        }
    }
}
