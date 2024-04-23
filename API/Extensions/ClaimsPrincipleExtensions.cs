using System.Security.Claims;

namespace API.Extensions
{
    /// <summary>
    /// Extensión para obtener metodos de clase ClaimsPrincipal
    /// </summary>
    public static class ClaimsPrincipleExtensions
    {
        /// <summary>
        /// Método para obtener el nombre de usuario
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static string GetUsername(this ClaimsPrincipal user) {
            return user.FindFirst(ClaimTypes.Name)?.Value;
        }

        public static int GetUserId(this ClaimsPrincipal user) 
        { 
            return int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        }
    }
}