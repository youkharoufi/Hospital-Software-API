using System.ComponentModel.DataAnnotations.Schema;
using AspNetCore.Identity.MongoDbCore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.VisualBasic;

namespace Hospital_Software.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Firstname { get; set; }

        public string Lastname { get; set; }

        public int? BadgeNumber { get; set; }

        public string? Token { get; set; }


        [ForeignKey("Name")]
        public string RoleName { get; set; }

        public ApplicationRole AppRole { get; set; }
    }
}
