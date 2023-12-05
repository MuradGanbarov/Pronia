using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Pronia.Utilites.Enums;

namespace Pronia.Models
{
    public class AppUser : IdentityUser
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public GenderType Gender { get; set; }
        //Gender saxlayarsan
    }
}
