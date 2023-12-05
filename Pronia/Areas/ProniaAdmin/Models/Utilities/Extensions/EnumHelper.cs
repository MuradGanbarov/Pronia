using Microsoft.AspNetCore.Authorization;
using Pronia.Utilites.Enums;

namespace Pronia.Areas.ProniaAdmin.Models.Utilities.Extensions
{
    public class AuthorizeRoles : AuthorizeAttribute
    {
        public class AuthorizeRolesAttribute : AuthorizeAttribute
        {
            public AuthorizeRolesAttribute(params UserRole[] allowedRoles)
            {
                var allowedRolesAsStrings = allowedRoles.Select(x => Enum.GetName(typeof(UserRole), x));
                Roles = string.Join(",", allowedRolesAsStrings);
            }
        }



    }
}
